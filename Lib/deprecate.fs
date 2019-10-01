namespace Lib

open System
open System.IO
open System.Collections.Generic
open System.Text.RegularExpressions
open Microsoft.Xrm.Sdk
open Microsoft.Xrm.Sdk.Client
open Microsoft.Xrm.Sdk.Messages
open Microsoft.Xrm.Sdk.Query
open Microsoft.Xrm.Sdk.Metadata

module Deprecate =
  type LogicalName = string

  type MetaData = {
      entityLName: LogicalName
      locale: int
      attribute: AttributeMetadata
  }

  type EntityMetaDataMap = Map<LogicalName, MetaData[]>

  type SolutionMetaDataMap = Map<LogicalName, EntityMetaDataMap>

  type DeprecationState = Deprecated
                        | Partial // Partial deprecation means that the field hase 2 of 3 deprecation attributes
                        | Favored /// same as not deprecated

  type DeprecationActions = DescriptionStamp
                          | SearchableToggle
                          | DisplayNamePrefix

  type ResDict = IDictionary<String, IDictionary<string, MetaData[]>>

  let deprecationStampPattern = 
    @"\n(\(Deprecated:)\s(?<date>\d{2,}\/\d{2,}\/\d{4,}\s\d{2,}.\d{2,}.\d{2,})(,\ssearch:\s(?<searchable>1|0))?(\))"


  let getResponse<'T when 'T :> OrganizationResponse> (proxy:IOrganizationService) request =
    (proxy.Execute(request)) :?> 'T

  let metadataToDeprecationType entityLName (attr: AttributeMetadata) =
      {
          MetaData.entityLName = entityLName
          locale = attr.Description.UserLocalizedLabel.LanguageCode
          attribute = attr
      }


  let getEntityAttributesFromId (proxy:IOrganizationService) metadataId filterPrefix =
    let request = RetrieveEntityRequest()
    request.MetadataId <- metadataId
    request.EntityFilters <- Microsoft.Xrm.Sdk.Metadata.EntityFilters.All
    request.RetrieveAsIfPublished <- true

    let resp = getResponse<RetrieveEntityResponse> proxy request
    let curriesDeprecationType = metadataToDeprecationType resp.EntityMetadata.LogicalName

    let filteredMetaData = 
      resp.EntityMetadata.Attributes 
      |> Array.filter (fun x -> x.AttributeOf = null)
      |> Array.filter (fun x -> x.LogicalName.StartsWith(filterPrefix))
      |> Array.map (curriesDeprecationType)


    (resp.EntityMetadata.LogicalName, filteredMetaData)


  let internal retrieveMultiple proxy logicalName (query:QueryExpression) = 
    query.PageInfo <- PagingInfo()

    let rec retrieveMultiple' 
      (proxy:IOrganizationService) (query:QueryExpression) page cookie =
      seq {
        query.PageInfo.PageNumber <- page
        query.PageInfo.PagingCookie <- cookie
        let resp = proxy.RetrieveMultiple(query)
        yield! resp.Entities

        match resp.MoreRecords with
        | true -> yield! retrieveMultiple' proxy query (page + 1) resp.PagingCookie
        | false -> ()
      }
    retrieveMultiple' proxy query 1 null


  let internal getEntitiesFilter 
    proxy (logicalName:string)
    (cols:string list) (filter:Map<string,obj>) =
    
    let f = FilterExpression()
    filter |> Map.iter(fun k v -> f.AddCondition(k, ConditionOperator.Equal, v))

    let q = QueryExpression(logicalName)
    if cols.Length = 0 then q.ColumnSet <- ColumnSet(true)
    else q.ColumnSet <- ColumnSet(Array.ofList cols)
    q.Criteria <- f
    
    retrieveMultiple proxy logicalName q
    |> Array.ofSeq

  let retrieveSolutionEntities proxy solutionName =
    let solutionFilter = [("uniquename", solutionName)] |> Map.ofList
    let solutions = 
      getEntitiesFilter proxy "solution" 
        ["solutionid"; "uniquename"] solutionFilter
    
    solutions
    |> Array.ofSeq
    |> Array.Parallel.map (fun sol ->
      let solutionComponentFilter = 
        [ ("solutionid", sol.Attributes.["solutionid"])
          ("componenttype", 1 :> obj) // 1 = Entity
        ] |> Map.ofList

      let entityMetadata = 
        getEntitiesFilter proxy "solutioncomponent" 
          ["solutionid"; "objectid"; "componenttype"] solutionComponentFilter
        |> Array.map (fun sc -> 
          getEntityAttributesFromId proxy (sc.Attributes.["objectid"] :?> Guid) "dg_")
        |> dict

      (sol.Attributes.["uniquename"].ToString(), entityMetadata)
    )
    |> dict

  let attributeUpdateRequest (proxy:IOrganizationService) (modifiedAttrMetadata: MetaData) =
    let req = UpdateAttributeRequest()
    req.Attribute <- modifiedAttrMetadata.attribute
    req.EntityName <- modifiedAttrMetadata.entityLName
    req.MergeLabels <- false

    (proxy.Execute(req)) :?> UpdateAttributeResponse

  let parseDescriptionStamp (description: string) =
    let dateMatch = Regex.Match(description, deprecationStampPattern).Groups.["date"].Value

    match dateMatch with
    | "" -> None
    | s  -> Some(s)


  let getDescriptionSearchable (description: string) = 
    match Regex.Match(description, deprecationStampPattern).Groups.["searchable"].Value with
    | "0" -> false
    | _   -> true // if no searchable vlaue is found, just return 1, implying we should reenable search
  

  let removeDescriptionTimestamp (description: string) =
    Regex.Replace(description, deprecationStampPattern, "");

  let createOrUpdateDescriptionStamp (description: string) (wasSearchable: bool) =
    let cleanDescription = removeDescriptionTimestamp description
    let searchable = if wasSearchable then "1" else "0"
    let deprecationDate = sprintf "\n(Deprecated: %A, search: %s)" DateTime.Now searchable
    cleanDescription + deprecationDate

  let isSearchable (attrMetaData: MetaData) =
    attrMetaData.attribute.IsValidForAdvancedFind.Value

  let safeAddDeprecationPrefix (displayName: string) (prefix: string) =
    if displayName.StartsWith prefix 
    then displayName
    else (prefix + displayName)

  let safeRemoveDeprecationPrefix (displayName: string) (prefix: string) =
    let prefixLength = prefix.Length
    if displayName.StartsWith prefix 
    then displayName.[prefixLength..]
    else displayName


  let labelToString (label: Label) =
    label.UserLocalizedLabel.Label.ToString()


  (* An attribute is deprecated when 
      1. The display name has been prepended with a deprecation prefix, such as "zz_"
      2. Searchable is set to no (IsValidForAdvancedFind)
      3. The description has a deprecation datetime-stamp
  *)
  let deprecateAttribute (proxy:IOrganizationService) (attrMetadata: MetaData) (displayNamePrefix: string) =
    // null check af attrmetadata?
    let attr = attrMetadata.attribute

    let newDescription = 
      createOrUpdateDescriptionStamp(labelToString attr.Description) attr.IsValidForAdvancedFind.Value
    attr.Description <- Label(newDescription, attrMetadata.locale)

    attr.IsValidForAdvancedFind <- BooleanManagedProperty(false)

    let newDisplayName = safeAddDeprecationPrefix (labelToString attr.DisplayName) displayNamePrefix
    attr.DisplayName <- Label(newDisplayName, attrMetadata.locale)

    (attributeUpdateRequest proxy attrMetadata).Results <> null

  /// If we wish to remove the deprecation of the attribute, we favor it
  let favorAttribute (proxy:IOrganizationService) (attrMetadata: MetaData) (displayNamePrefix: string) =
    let attr = attrMetadata.attribute

    let previousSearchable = getDescriptionSearchable (labelToString attr.Description)
    attr.IsValidForAdvancedFind <- BooleanManagedProperty(previousSearchable)

    let newDescription = removeDescriptionTimestamp (labelToString attr.Description)
    attr.Description <- Label(newDescription, attrMetadata.locale)

    let newDisplayName = safeRemoveDeprecationPrefix(labelToString attr.DisplayName) displayNamePrefix
    attr.DisplayName <- Label(newDisplayName, attrMetadata.locale)

    (attributeUpdateRequest proxy attrMetadata).Results <> null

