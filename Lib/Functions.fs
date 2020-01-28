namespace Lib

open System
open Microsoft.Xrm.Sdk
open Microsoft.Xrm.Sdk.Metadata
open System.Globalization
open Types
open System.Text.RegularExpressions

module Functions = 
  // This should be done using a parser if it needs to be extended. This is not readable at all.
  // especially if we need to provide backwards compatibility
  let deprecationStampPattern = 
    @"\n?(\(Deprecated:)\s*(?<date>\d{2,}\/\d{2,}\/\d{4,}\s*\d{2,}.\d{2,}.\d{2,}),\s*(was searchable|search):\s*(?<searchable>(1|0)|(yes|no))?(\))"

  let parseDescriptionStamp (description: string) =
    let dateMatch = Regex.Match(description, deprecationStampPattern).Groups.["date"].Value

    match dateMatch with
    | "" -> None
    | s  -> Some(s)

  let isDescriptionSearchable (description: string) = 
    match Regex.Match(description, deprecationStampPattern).Groups.["searchable"].Value with
    | "0"  -> false
    | "no" -> false
    | _   -> true // if no searchable vlaue is found, just return 1, implying we should reenable search


  let DeprecationStateToCheckBoxLiteral = function
    | DeprecationState.Favored -> UNCHECKED
    | DeprecationState.Deprecated -> CHECKED
    | DeprecationState.Partial -> INDETERMINATE
    | _ -> failwith "unexpected state"

  let CheckBoxLiteralToDeprecationState = function
    | x when x = UNCHECKED -> DeprecationState.Favored
    | x when x = CHECKED -> DeprecationState.Deprecated
    | x when x = INDETERMINATE -> DeprecationState.Partial
    | _ -> failwith "unexpected literal"

  let startsWithPrefix (text: string) prefix =
    text.StartsWith(prefix, true, CultureInfo.InvariantCulture)

  let attrStartsWithPrefix (attrMetaData: AttributeMetadata) prefix =
    startsWithPrefix (labelToString attrMetaData.DisplayName) prefix

  let isSearchable (attrMetaData: AttributeMetadata) =
    attrMetaData.IsValidForAdvancedFind.Value |> not

  let hasDeprecationDescription (attr: AttributeMetadata) = 
    match (parseDescriptionStamp (labelToString attr.Description)) with
    | Some(x) -> true
    | _ -> false

  let isDeprecated (attr: AttributeMetadata) prefix =
    (isSearchable attr) && (hasDeprecationDescription attr) && (attrStartsWithPrefix attr prefix)
  
  let isPartiallyDeprecated (attr: AttributeMetadata) prefix =
    (attrStartsWithPrefix attr prefix)

  let removeDescriptionTimestamp (description: string) =
    Regex.Replace(description, deprecationStampPattern, "");

  let getDeprecationState (attr: AttributeMetadata) prefix =
    match attr with
    | x when (isDeprecated x prefix) -> DeprecationState.Deprecated
    | x when (isPartiallyDeprecated x prefix) -> DeprecationState.Partial
    | _ -> DeprecationState.Favored


  // if prefix is empty, it crashes. 
  // We need to check the given attribute metadata contains info or we can get a null pointer exeception.
  let convertMetaDataType entityLName prefix (attr: AttributeMetadata) =
      {
          MetaData.entityLName = entityLName
          locale = attr.Description.UserLocalizedLabel.LanguageCode
          attribute = attr
          deprecationState = (getDeprecationState attr prefix) 
      }

  let filterValidAttribute (x: AttributeMetadata) =
       x.Description <> null
    && x.Description.UserLocalizedLabel <> null
    && x.DisplayName<> null
    && x.DisplayName.UserLocalizedLabel <> null
    && x.IsValidForAdvancedFind <> null