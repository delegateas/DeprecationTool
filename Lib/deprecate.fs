namespace Lib

open System
open Microsoft.Xrm.Sdk
open Types
open Functions
open Requests

module Deprecate =
  let createOrUpdateDescriptionStamp (description: string) (wasSearchable: bool) =
    let cleanDescription = removeDescriptionTimestamp description
    let searchable = if wasSearchable then WAS_SEARCHABLE_YES else WAS_SEARCHABLE_NO
    let deprecationDate = sprintf "\n(Deprecated: %A, was searchable: %s)" DateTime.Now searchable
    cleanDescription + deprecationDate

  let safeAddDeprecationPrefix (displayName: string) (prefix: string) =
    if startsWithPrefix displayName prefix 
    then displayName
    else (prefix + displayName)

  let safeRemoveDeprecationPrefix (displayName: string) (prefix: string) =
    let prefixLength = prefix.Length
    if startsWithPrefix displayName prefix
    then displayName.[prefixLength..]
    else displayName

  (* An attribute is deprecated when 
      1. The display name has been prepended with a deprecation prefix, such as "zz_"
      2. Searchable is set to no (IsValidForAdvancedFind)
      3. The description has a deprecation datetime-stamp defined by our RegExp at the beginning.
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

    let previousSearchable = isDescriptionSearchable (labelToString attr.Description)
    attr.IsValidForAdvancedFind <- BooleanManagedProperty(previousSearchable)

    let newDescription = removeDescriptionTimestamp (labelToString attr.Description)
    attr.Description <- Label(newDescription, attrMetadata.locale)
    attr.Description.UserLocalizedLabel <- LocalizedLabel(newDescription, attrMetadata.locale)

    let newDisplayName = safeRemoveDeprecationPrefix(labelToString attr.DisplayName) displayNamePrefix
    attr.DisplayName <- Label(newDisplayName, attrMetadata.locale)
    attr.DisplayName.UserLocalizedLabel <- LocalizedLabel(newDisplayName, attrMetadata.locale)

    attributeUpdateRequest attrMetadata

  let pendingChanges (attrs: MetaDataWithCheck[]) =
    attrs 
    |> Array.filter(fun x -> x.deprecationState <> x.metaData.deprecationState)

  let hasPendingChanges (attrs: MetaDataWithCheck[]) =
    (pendingChanges attrs).Length > 0

  let decideAction (checkedState: DeprecationState) (attr: MetaData) = 
    match checkedState  with
    | DeprecationState.Deprecated -> Deprecate attr
    | DeprecationState.Favored    -> Favor attr
    | _ -> Favor attr

  let buildAction (prefix: string) (attr: Action) =
    match attr with
    | Deprecate x -> deprecateAttribute x prefix
    | Favor x -> favorAttribute x prefix
  
  let decideAndExecuteOperations (proxy: IOrganizationService) (attrs: MetaDataWithCheck[]) (prefix: string) =
    let builderWithPrefix = buildAction prefix

    pendingChanges attrs
    |> Array.Parallel.map(fun x -> decideAction x.deprecationState x.metaData)
    |> Array.Parallel.map(fun x -> builderWithPrefix x)
    |> Array.Parallel.map(fun x -> x :> OrganizationRequest)
    |> Array.chunkBySize(1000)
    |> Array.map(fun x -> executeRequests proxy x)
