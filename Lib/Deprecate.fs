namespace Lib

open System
open Microsoft.Xrm.Sdk
open Types
open Functions
open Requests

module Deprecate =
  let createOrUpdateDescriptionStamp (description: string) (wasSearchable: bool) (wasRequired: bool) =
    let cleanDescription = removeDescriptionTimestamp description
    let deprecationDescription = 
      { date          = DateTime.Now
        wasSearchable = wasSearchable
        wasRequired   = wasRequired }
      |> textFromDeprecationDescription

    cleanDescription + deprecationDescription
    
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
      2. Searchable is set to no (IsValidForAdvancedFind) and businessrequired is set to no
      3. The description has a deprecation datetime-stamp defined by our RegExp at the beginning.
  *)
  let deprecateAttribute (attrMetadata: MetaData) (displayNamePrefix: string) =
    // null check af attrmetadata?
    let attr = attrMetadata.attribute
    attrMetadata.deprecationState <- DeprecationState.Deprecated

    let wasRequired = attr.RequiredLevel.Value = Metadata.AttributeRequiredLevel.ApplicationRequired

    let newDescription = 
      createOrUpdateDescriptionStamp (labelToString attr.Description) attr.IsValidForAdvancedFind.Value wasRequired
    attr.Description <- Label(newDescription, attrMetadata.locale)
    attr.Description.UserLocalizedLabel <- LocalizedLabel(newDescription, attrMetadata.locale)

    attr.IsValidForAdvancedFind <- BooleanManagedProperty(false)
    attr.RequiredLevel <- Metadata.AttributeRequiredLevelManagedProperty(Metadata.AttributeRequiredLevel.None)

    let newDisplayName = safeAddDeprecationPrefix (labelToString attr.DisplayName) displayNamePrefix
    attr.DisplayName <- Label(newDisplayName, attrMetadata.locale)
    attr.DisplayName.UserLocalizedLabel <- LocalizedLabel(newDisplayName, attrMetadata.locale)

    (attributeUpdateRequest attrMetadata)

  /// If we wish to remove the deprecation of the attribute, we favor it
  let favorAttribute (attrMetadata: MetaData) (displayNamePrefix: string) =
    let attr = attrMetadata.attribute
    attrMetadata.deprecationState <- DeprecationState.Favored

    let deprecateDescriptionDetails = descriptionDetails (labelToString attr.Description)
    
    attr.IsValidForAdvancedFind <- BooleanManagedProperty(wasSearchable deprecateDescriptionDetails)
    attr.RequiredLevel <- Metadata.AttributeRequiredLevelManagedProperty(wasRequired deprecateDescriptionDetails)

    let newDescription = removeDescriptionTimestamp (labelToString attr.Description)
    attr.Description <- Label(newDescription, attrMetadata.locale)
    attr.Description.UserLocalizedLabel <- LocalizedLabel(newDescription, attrMetadata.locale)

    let newDisplayName = safeRemoveDeprecationPrefix(labelToString attr.DisplayName) displayNamePrefix
    attr.DisplayName <- Label(newDisplayName, attrMetadata.locale)
    attr.DisplayName.UserLocalizedLabel <- LocalizedLabel(newDisplayName, attrMetadata.locale)

    attributeUpdateRequest attrMetadata

  let actionWithDependant action (attrMetadata: MetaData) (displayNamePrefix: string) =
    match attrMetadata.dependantMetaData with
    | Some(x) -> [| action attrMetadata displayNamePrefix ; 
                    action x displayNamePrefix|]
    | None    -> [| action attrMetadata displayNamePrefix |]

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
    | Deprecate x -> actionWithDependant deprecateAttribute x prefix
    | Favor     x -> actionWithDependant favorAttribute x prefix
  
  let decideAndExecuteOperations (proxy: IOrganizationService) (attrs: MetaDataWithCheck[]) (prefix: string) =
    let builderWithPrefix = buildAction prefix
    let pending = pendingChanges attrs

    pending
    |> Array.Parallel.map (fun x -> decideAction x.deprecationState x.metaData)
    |> Array.Parallel.map (builderWithPrefix)
    // map list of requests to OrganizationRequest and flatten all lists, 
    // note buildAction can return either 1 or 2 OrganizationRequests, that's why it's needed.
    |> Array.Parallel.collect (Array.map (fun x -> x :> OrganizationRequest))
    |> Array.chunkBySize  1000
    |> Array.map(fun x -> executeRequests proxy x)
