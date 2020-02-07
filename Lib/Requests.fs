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
open Microsoft.Crm.Sdk.Messages
open Types
open Functions

module Requests =
  let getResponse<'T when 'T :> OrganizationResponse> (proxy:IOrganizationService) request =
    (proxy.Execute(request)) :?> 'T

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

  let filteredMetaDataMap filteredMetaData = 
    filteredMetaData
    |> Array.Parallel.choose (fun x -> if x.attribute.LogicalName.EndsWith(CURRENCY_BASE) then Some(x) else None)
    |> Array.Parallel.map    (fun x -> (x.attribute.LogicalName, x))
    |> Map.ofArray

  let combinedCurrencyFieldMetaData filteredMetaData baseSuffixMetaDataMap = 
    filteredMetaData
    |> Array.fold (fun (acc, map) x -> combineCurrencyBaseFields x acc map CURRENCY_BASE) ([], baseSuffixMetaDataMap)
    |> (fst >> Array.ofList) // turn first item in tuple, a list, into an array

  let getEntityAttributesFromId (proxy:IOrganizationService) metadataId filterPrefix deprecationPrefix =
    let request = RetrieveEntityRequest()
    request.MetadataId <- metadataId
    request.EntityFilters <- Microsoft.Xrm.Sdk.Metadata.EntityFilters.All
    request.RetrieveAsIfPublished <- true

    let resp = getResponse<RetrieveEntityResponse> proxy request
    let curriedDeprecationType = convertMetaDataType resp.EntityMetadata.LogicalName deprecationPrefix

    let filteredMetaData = 
      resp.EntityMetadata.Attributes 
      |> Array.filter (fun x -> filterValidAttribute x)
      |> Array.filter (fun x -> x.AttributeOf = null)
      |> Array.filter (fun x -> startsWithPrefix x.LogicalName filterPrefix)
      |> Array.Parallel.map (curriedDeprecationType)
    
    // construct a map of every entity with _base suffix to allow constant time lookups
    // when going through entities to decide if they're a currency field or not 
    // giving them some dependantMetaData value if lookup succeeds
    let combinedCurrencyFieldMetaData = 
      filteredMetaData
      |> filteredMetaDataMap
      |> combinedCurrencyFieldMetaData filteredMetaData

    let returnValue = {
      entityGuid = metadataId
      fields = combinedCurrencyFieldMetaData
    }

    (resp.EntityMetadata.LogicalName, returnValue)

  let retrieveSolutionEntities proxy (solutions: SolutionData[]) fieldPrefix deprecationPrefix =
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
          getEntityAttributesFromId proxy (sc.Attributes.["objectid"] :?> Guid) fieldPrefix deprecationPrefix)
        |> dict
        |> (fun x -> Dictionary(x))
      (sol.uniqueName, entityMetadata)
    )
    |> dict

  let retrieveSolutionNames proxy (ignore: string[]) prefix =
    let ignoreSet = ignore |> Set.ofArray
    getEntitiesFilter proxy "solution" 
        ["solutionid"; "uniquename"; "ismanaged"] Map.empty
    |> Seq.filter(fun x -> ignoreSet.Contains(x.Attributes.["uniquename"].ToString()) |> not)
    |> Seq.filter(fun x -> (x.Attributes.["ismanaged"] :?> bool) |> not)
    |> Seq.filter(fun x -> startsWithPrefix (x.Attributes.["uniquename"].ToString()) prefix)
    |> Seq.map(fun x -> 
      { SolutionData.id = (x.Attributes.["solutionid"] :?> Guid) 
        uniqueName = x.Attributes.["uniquename"].ToString() 
      }
    )
    |> Array.ofSeq

  let getDependencyCountForEntity proxy (attr: MetaData) =
    let request = RetrieveDependenciesForDeleteRequest()
    request.ComponentType <- 2 //componenttype 2 = entity
    request.ObjectId <- attr.attribute.MetadataId.Value

    let response = getResponse<RetrieveDependenciesForDeleteResponse> proxy request
    response.EntityCollection.Entities.Count

  let attributeUpdateRequest (modifiedAttrMetadata: MetaData) =
    let req = UpdateAttributeRequest()
    req.Attribute <- modifiedAttrMetadata.attribute
    req.EntityName <- modifiedAttrMetadata.entityLName
    req.MergeLabels <- false

    req

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