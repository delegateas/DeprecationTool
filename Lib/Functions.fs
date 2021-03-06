﻿namespace Lib

open System
open Microsoft.Xrm.Sdk
open Microsoft.Xrm.Sdk.Metadata
open System.Globalization
open Types
open Parser
open System.Text.RegularExpressions

module Functions = 
  // This regex is a simple "first pass" of our parser, if we don't have this general structure
  // in the description, we don't have a proper deprecation description tag.
  let deprecationDescriptionPattern = 
    @"\n?\((?i)Deprecated.*\)$"

  let descriptionDetails (description: string) = 
    let firstPass = Regex.Match(description, deprecationDescriptionPattern)
    if firstPass.Success
    then parseDescription firstPass.Value
    else None

  let textFromDeprecationDescription depdesc = 
    let searchable = if depdesc.wasSearchable then YES_IDENTIFIER else NO_IDENTIFIER
    let required = if depdesc.wasRequired then YES_IDENTIFIER else NO_IDENTIFIER
    sprintf "\n(Deprecated: %A, was searchable: %s, was required: %s)" DateTime.Now searchable required

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

  let hasDeprecationDescription (attr: AttributeMetadata) = 
    let rawDescription = labelToString attr.Description
    Regex.Match(rawDescription, deprecationDescriptionPattern).Success

  let isSearchable (attrMetaData: AttributeMetadata) =
    attrMetaData.IsValidForAdvancedFind.Value |> not

  let isDeprecated (attr: AttributeMetadata) prefix =
    (isSearchable attr) && (hasDeprecationDescription attr) && (attrStartsWithPrefix attr prefix)
  
  let isPartiallyDeprecated (attr: AttributeMetadata) prefix =
    (attrStartsWithPrefix attr prefix)

  let removeDescriptionTimestamp (description: string) =
    Regex.Replace(description, deprecationDescriptionPattern, "");

  let getDeprecationState (attr: AttributeMetadata) prefix =
    match attr with
    | x when (isDeprecated x prefix) -> DeprecationState.Deprecated
    | x when (isPartiallyDeprecated x prefix) -> DeprecationState.Partial
    | _ -> DeprecationState.Favored

  let wasSearchable = function
  | Some(x) -> x.wasSearchable
  | _ -> true

  let wasRequired = function
  | Some(x) -> if x.wasRequired 
               then Metadata.AttributeRequiredLevel.ApplicationRequired 
               else Metadata.AttributeRequiredLevel.None
  | _ -> Metadata.AttributeRequiredLevel.None

  // if prefix is empty, it crashes. 
  // We need to check the given attribute metadata contains info or we can get a null pointer exeception.
  let convertMetaDataType entityLName prefix (attr: AttributeMetadata) =
      {
          MetaData.entityLName = entityLName
          locale = attr.Description.UserLocalizedLabel.LanguageCode
          attribute = attr
          dependantMetaData = None
          deprecationState = (getDeprecationState attr prefix) 
      }

  let filterValidAttribute (x: AttributeMetadata) =
       x.Description <> null
    && x.Description.UserLocalizedLabel <> null
    && x.DisplayName<> null
    && x.DisplayName.UserLocalizedLabel <> null
    && x.IsValidForAdvancedFind <> null

  let combineCurrencyBaseFields (x: MetaData) (acc: MetaData list) (map: Map<LogicalName, MetaData>) suffix =
    let logicalName = x.attribute.LogicalName
    if logicalName.EndsWith(suffix) then
      (acc, map)
    else
      let baseEntityOpt = Map.tryFind (logicalName + suffix) map
      let metaDataWithPossibleBase = {x with dependantMetaData = baseEntityOpt}
      (metaDataWithPossibleBase :: acc, map)
