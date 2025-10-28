module Organizations.Templates.Formatters

open System
open System.Text.RegularExpressions
    
let toInput = function | Some (date: DateOnly) -> date.ToString "yyyy-MM-dd" | _ -> ""
let toDisplay =  function | Some (date: DateOnly) -> date.ToShortDateString() | _ -> "-"

let toTakNie (isTrue: bool) =
    $"""{if isTrue then "Tak" else "Nie"}"""

let pascalToWords (input: string) =
    Regex.Replace(input, "(?<=[a-z])([A-Z])", " $1")
