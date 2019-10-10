﻿namespace Lib

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
  open System.Web.UI.WebControls.WebParts

  type DeprecationState = 
                        | Favored=0 // same as not deprecated
                        | Deprecated=1
                        | Partial=2 // Partial deprecation means only the name has been prefixed as deprecated.

  type LogicalName = string

  type MetaData = {
      entityLName: LogicalName;
      locale: int;
      attribute: AttributeMetadata;
      mutable deprecationState: DeprecationState;
  } with 
    override this.ToString() =
      this.attribute.SchemaName

  type SolutionData = {
    id: Guid
    uniqueName: string
  } with 
    override this.ToString() =
      this.uniqueName
  
  type DisplayValue = {
    Display: string
    Value: string
  } with
    override this.ToString() = 
      this.Display
   
  type MetaDataWithCheck = {
    metaData: MetaData
    isChecked: DeprecationState
  }
  
//  type ResDict = IDictionary<String, IDictionary<string, MetaData[]>>

  type EntityMetaDataMap = Map<LogicalName, MetaData[]>

  type SolutionMetaDataMap = Map<LogicalName, EntityMetaDataMap>


  type Action = Deprecate of Data: MetaData
              | Favor of Data : MetaData


  let labelToString (label: Label) =
    label.UserLocalizedLabel.Label.ToString()

  let deprecationStampPattern = 
    @"\n(\(Deprecated:)\s(?<date>\d{2,}\/\d{2,}\/\d{4,}\s\d{2,}.\d{2,}.\d{2,})(,\ssearch:\s(?<searchable>1|0))?(\))"

  let startsWithPrefix (attrMetaData: AttributeMetadata) prefix =
    (labelToString attrMetaData.DisplayName).StartsWith(prefix)

  let isSearchable (attrMetaData: AttributeMetadata) =
    attrMetaData.IsValidForAdvancedFind.Value |> not

  let parseDescriptionStamp (description: string) =
    let dateMatch = Regex.Match(description, deprecationStampPattern).Groups.["date"].Value

    match dateMatch with
    | "" -> None
    | s  -> Some(s)

  let hasDeprecationDescription (attr: AttributeMetadata) = 
    match (parseDescriptionStamp (labelToString attr.Description)) with
    | Some(x) -> true
    | _ -> false

  let isDeprecated (attr: AttributeMetadata) prefix =
    (isSearchable attr) && (hasDeprecationDescription attr) && (startsWithPrefix attr prefix)
  
  let isPartiallyDeprecated (attr: AttributeMetadata) prefix =
    (startsWithPrefix attr prefix)

  let getDeprecationState (attr: AttributeMetadata) prefix =
    match attr with
    | x when (isDeprecated x prefix) -> DeprecationState.Deprecated
    | x when (isPartiallyDeprecated x prefix) -> DeprecationState.Partial
    | _ -> DeprecationState.Favored


  let getResponse<'T when 'T :> OrganizationResponse> (proxy:IOrganizationService) request =
    (proxy.Execute(request)) :?> 'T
  
  // if prefix is empty, it crashes. 
  // We need to check the given attribute metadata contains info or we can get a null pointer exeception.
  let convertMetaDataType entityLName prefix (attr: AttributeMetadata) =
      {
          MetaData.entityLName = entityLName
          locale = attr.Description.UserLocalizedLabel.LanguageCode
          attribute = attr
          deprecationState = (getDeprecationState attr prefix) 
      }

  let getEntityAttributesFromId (proxy:IOrganizationService) metadataId filterPrefix deprecationPrefix =
    let request = RetrieveEntityRequest()
    request.MetadataId <- metadataId
    request.EntityFilters <- Microsoft.Xrm.Sdk.Metadata.EntityFilters.All
    request.RetrieveAsIfPublished <- true

    let resp = getResponse<RetrieveEntityResponse> proxy request
    let curriedDeprecationType = convertMetaDataType resp.EntityMetadata.LogicalName deprecationPrefix

    let filteredMetaData = 
      resp.EntityMetadata.Attributes 
      |> Array.filter (fun x -> x.AttributeOf = null)
      |> Array.filter (fun x -> x.LogicalName.StartsWith(filterPrefix))
      |> Array.map (curriedDeprecationType)


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
    
    // this is not pretty, look at it later
    if logicalName = "solution" then f.FilterOperator <- LogicalOperator.Or

    filter |> Map.iter(fun k v -> f.AddCondition(k, ConditionOperator.Equal, v))

    let q = QueryExpression(logicalName)
    if cols.Length = 0 then q.ColumnSet <- ColumnSet(true)
    else q.ColumnSet <- ColumnSet(Array.ofList cols)
    q.Criteria <- f
    
    retrieveMultiple proxy logicalName q
    |> Array.ofSeq

  let retrieveSolutionEntities proxy (solutions: SolutionData[]) entityPrefix deprecationPrefix =
    solutions
    |> Array.Parallel.map (fun sol ->
      let solutionComponentFilter = 
        [ ("solutionid", sol.id.ToString() :> obj)
          ("componenttype", 1 :> obj) // 1 = Entity
        ] |> Map.ofList

      let entityMetadata = 
        getEntitiesFilter proxy "solutioncomponent" 
          ["solutionid"; "objectid"; "componenttype"] solutionComponentFilter
        |> Array.map (fun sc -> 
          getEntityAttributesFromId proxy (sc.Attributes.["objectid"] :?> Guid) entityPrefix deprecationPrefix)
        |> dict
      (sol.uniqueName, entityMetadata)
    )
    |> dict

  let retrieveSolutionNames proxy (ignore: string[]) prefix =
    let ignoreSet = ignore |> Set.ofArray
    getEntitiesFilter proxy "solution" 
        ["solutionid"; "uniquename"; "ismanaged"] Map.empty
    |> Seq.filter(fun x -> ignoreSet.Contains(x.Attributes.["uniquename"].ToString()) |> not)
    |> Seq.filter(fun x -> (x.Attributes.["ismanaged"] :?> bool) |> not)
    |> Seq.filter(fun x -> x.Attributes.["uniquename"].ToString().StartsWith(prefix))
    |> Seq.map(fun x -> 
      { SolutionData.id = (x.Attributes.["solutionid"] :?> Guid) 
        uniqueName = x.Attributes.["uniquename"].ToString() 
      }
    )
    |> Array.ofSeq

  let attributeUpdateRequest (modifiedAttrMetadata: MetaData) =
    let req = UpdateAttributeRequest()
    req.Attribute <- modifiedAttrMetadata.attribute
    req.EntityName <- modifiedAttrMetadata.entityLName
    req.MergeLabels <- false

    req

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

  let safeAddDeprecationPrefix (displayName: string) (prefix: string) =
    if displayName.StartsWith prefix 
    then displayName
    else (prefix + displayName)

  let safeRemoveDeprecationPrefix (displayName: string) (prefix: string) =
    let prefixLength = prefix.Length
    if displayName.StartsWith prefix 
    then displayName.[prefixLength..]
    else displayName


  let executeRequests (proxy: IOrganizationService) (reqs: OrganizationRequest[]) =
    let req = ExecuteMultipleRequest()
    req.Requests <- OrganizationRequestCollection()
    req.Requests.AddRange(reqs)
    req.Settings <- ExecuteMultipleSettings()
    req.Settings.ContinueOnError <- true
    printfn "Firing execute multiple"
    let resp = proxy.Execute(req) :?> ExecuteMultipleResponse
    printfn "Execute multiple finished"
    if resp.IsFaulted then
      printfn "Printing errror"
      resp.Responses
      |> Array.ofSeq
      |> Array.iter (fun r -> printfn "ERROR: %i, %A" r.RequestIndex r.Fault.Message)

    resp


  (* An attribute is deprecated when 
      1. The display name has been prepended with a deprecation prefix, such as "zz_"
      2. Searchable is set to no (IsValidForAdvancedFind)
      3. The description has a deprecation datetime-stamp
  *)
  let deprecateAttribute (attrMetadata: MetaData) (displayNamePrefix: string) =
    // null check af attrmetadata?
    let attr = attrMetadata.attribute
    attrMetadata.deprecationState <- DeprecationState.Deprecated

    let newDescription = 
      createOrUpdateDescriptionStamp(labelToString attr.Description) attr.IsValidForAdvancedFind.Value
    attr.Description <- Label(newDescription, attrMetadata.locale)
    attr.Description.UserLocalizedLabel <- LocalizedLabel(newDescription, attrMetadata.locale)

    attr.IsValidForAdvancedFind <- BooleanManagedProperty(false)

    let newDisplayName = safeAddDeprecationPrefix (labelToString attr.DisplayName) displayNamePrefix
    attr.DisplayName <- Label(newDisplayName, attrMetadata.locale)
    attr.DisplayName.UserLocalizedLabel <- LocalizedLabel(newDisplayName, attrMetadata.locale)

    (attributeUpdateRequest attrMetadata)

  /// If we wish to remove the deprecation of the attribute, we favor it
  let favorAttribute (attrMetadata: MetaData) (displayNamePrefix: string) =
    let attr = attrMetadata.attribute
    attrMetadata.deprecationState <- DeprecationState.Favored

    let previousSearchable = getDescriptionSearchable (labelToString attr.Description)
    attr.IsValidForAdvancedFind <- BooleanManagedProperty(previousSearchable)

    let newDescription = removeDescriptionTimestamp (labelToString attr.Description)
    attr.Description <- Label(newDescription, attrMetadata.locale)
    attr.Description.UserLocalizedLabel <- LocalizedLabel(newDescription, attrMetadata.locale)

    let newDisplayName = safeRemoveDeprecationPrefix(labelToString attr.DisplayName) displayNamePrefix
    attr.DisplayName <- Label(newDisplayName, attrMetadata.locale)
    attr.DisplayName.UserLocalizedLabel <- LocalizedLabel(newDisplayName, attrMetadata.locale)

    attributeUpdateRequest attrMetadata


  let decideAction (attr: MetaData) = 
    match attr.deprecationState  with
    | Deprecated -> Favor attr
    | _ -> Deprecate attr

  let buildAction (prefix: string) (attr: Action) =
    match attr with
    | Deprecate x -> deprecateAttribute x prefix
    | Favor x -> favorAttribute x prefix
  
  //let isDeprecationState = function DeprecationState.Deprecated _ -> true | _ -> false

  let decideOperations (proxy: IOrganizationService) (attrs: MetaDataWithCheck[]) (prefix: string) =
    let builderWithPrefix = buildAction prefix
    attrs
    |> Array.filter(fun x -> x.isChecked <> x.metaData.deprecationState)
    |> Array.Parallel.map(fun x -> decideAction x.metaData)
    |> Array.Parallel.map(fun x -> builderWithPrefix x)
    |> Array.Parallel.map(fun x -> x :> OrganizationRequest)
    |> Array.chunkBySize(1000)
    |> Array.map(fun x -> executeRequests proxy x)

