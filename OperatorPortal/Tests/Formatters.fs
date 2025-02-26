module Tests.Formatters

open System

let toTakNie (isTrue: bool) =
    $"""{if isTrue then "Tak" else "Nie"}"""

let toDate (dateOpt: DateOnly option) : string =
    match dateOpt with
    | Some date -> date.ToString("dd.MM.yyyy", System.Globalization.CultureInfo("pl-PL"))
    | None -> "-"
