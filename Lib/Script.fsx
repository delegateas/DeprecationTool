// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

#r "bin/Debug/Lib.dll"

open Lib
open Lib.Parser
open System.Text.RegularExpressions
// Define your library scripting code here

parseDescription "
(Deprecated: 10/12/2019 16.20.05, search: 1)"

parseDescription "
(Deprecated: 10/12/2019 16.20.05, was searchable: 0)"

parseDescription "



(Deprecated: 10/12/2019 16.20.05, was searchable: yes, was required: 0)" ;;


let deprecationDescriptionPattern = 
  @"\n?\((?i)Deprecated.*\)$"

Regex.Match("some text here to test stuff
more tech
and soaces () 
(deprecated: 10/12/2019 16.20.05, was searchable: yes, was required: 0)", deprecationDescriptionPattern).Value;;

