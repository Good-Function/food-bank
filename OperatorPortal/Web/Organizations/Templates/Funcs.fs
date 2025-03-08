module Organizations.Funcs

open System

let formatDate (dateOpt: DateOnly option) : string =
    match dateOpt with
    | Some date -> date.ToString("dd.MM.yyyy", System.Globalization.CultureInfo("pl-PL"))
    | None -> "-"
