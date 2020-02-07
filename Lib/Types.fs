namespace Lib
open System
open Microsoft.Xrm.Sdk
open Microsoft.Xrm.Sdk.Metadata
open System.Globalization

module Types = 
  [<Literal>]
  let UNCHECKED = "unchecked";
  [<Literal>]
  let CHECKED = "checked";
  [<Literal>]
  let INDETERMINATE = "indeterminate";

  [<Literal>]
  let YES_IDENTIFIER = "yes";
  [<Literal>]
  let NO_IDENTIFIER = "no";

  [<Literal>]
  let CURRENCY_BASE = "_base";

  let labelToString (label: Label) =
    label.UserLocalizedLabel.Label.ToString()

  type DeprecationState = Favored = 0 // same as not deprecated
                        | Deprecated = 1
                        | Partial = 2 // Partial deprecation means only the name has been prefixed as deprecated.

  type LogicalName = string

  type DeprecationDescription = {
    date: DateTime;
    wasSearchable: bool;
    wasRequired: bool;
  }

  type FieldNames = {
    logicalName: string
    displayName: string
  }

  type MetaData = {
    entityLName: LogicalName;
    locale: int;
    attribute: AttributeMetadata;
    dependantMetaData: MetaData option;
    mutable deprecationState: DeprecationState;
  } with 
    override this.ToString() =
      this.attribute.LogicalName
    member this.ColumnNames() =
      { logicalName = this.attribute.LogicalName
        displayName = labelToString this.attribute.DisplayName }

  type EntityWithFields = {
    entityGuid: Guid;
    fields: MetaData[];
  }

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
    deprecationState: DeprecationState
  }

  type EntityMetaDataMap = Map<LogicalName, MetaData[]>

  type SolutionMetaDataMap = Map<LogicalName, EntityMetaDataMap>

  type Action = Deprecate of Data: MetaData
              | Favor of Data : MetaData