namespace Lib

open FParsec
open System
open Types

module Parser =
  let extendedBoolean = function
    | x when x = YES_IDENTIFIER -> true
    | "1"   -> true
    | _     -> false

  let decideRequired = function
    | Some(x) -> x
    | None    -> YES_IDENTIFIER
  

  let private parser =
      let stringTrimmed s = pstringCI s .>> spaces
      let charTrimmed c = pchar c .>> spaces
      let anyCharsTill pEnd = manyCharsTill anyChar pEnd
      let keyValue = anyCharsTill (pchar ',' <|> pchar ')')
      let onlyValueAfter prefix = prefix >>. keyValue

      let searchableChoice = stringTrimmed "search:" <|> stringTrimmed "was searchable:"
      let requiredChoice   = stringTrimmed "was required:"

      let date          = onlyValueAfter <| stringTrimmed "Deprecated:"
      let wasSearchable = spaces >>. onlyValueAfter searchableChoice
      let wasRequired   = opt (spaces >>. onlyValueAfter requiredChoice)

      let createDeprecationDescription date search required = {
          date          = DateTime.Parse(date)
          wasSearchable = extendedBoolean search
          wasRequired   = extendedBoolean (decideRequired required)
      }

      let description = pipe3 date wasSearchable wasRequired createDeprecationDescription

      opt unicodeSpaces >>. charTrimmed '(' >>. description .>> unicodeSpaces .>> eof

  let parseDescription desc =
      match desc |> run parser with
      | Success(v,_,_) -> Some(v)
      | Failure(_,_,_) -> None