namespace Lib

open FParsec
open System

module Parser =
  type DeprecationDescription = {
    date: DateTime;
    wasSearchable: bool;
    wasRequired: bool;
  }

  let extendedBoolean = function
    | "yes" -> true
    | "1"   -> true
    | _     -> false

  let decideRequired = function
    | Some(x) -> x
    | None    -> "yes"
  

  let private parser =
      let str_ws s = pstringCI s .>> spaces
      let char_ws c = pchar c .>> spaces
      let anyCharsTill pEnd = manyCharsTill anyChar pEnd
      let keyValue = anyCharsTill (pchar ',' <|> pchar ')')
      let onlyValueAfter prefix = prefix >>. keyValue

      let searchableChoice = str_ws "search:" <|> str_ws "was searchable:"
      let requiredChoice   = str_ws "was required:"

      let date          = onlyValueAfter <| str_ws "Deprecated:"
      let wasSearchable = spaces >>. onlyValueAfter searchableChoice
      let wasRequired   = opt (spaces >>. onlyValueAfter requiredChoice)

      let createDeprecationDescription date search required = {
          date          = DateTime.Parse(date)
          wasSearchable = extendedBoolean search
          wasRequired   = extendedBoolean (decideRequired required)
      }

      let commit = pipe3 date wasSearchable wasRequired createDeprecationDescription

      opt unicodeSpaces >>. char_ws '(' >>. commit .>> (opt <| char_ws ')') .>> unicodeSpaces .>> eof

  let parseDescription log =
      match log |> run parser with
      | Success(v,_,_)   -> v
      | Failure(msg,_,_) -> failwith msg