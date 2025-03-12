module Web.Organizations.Templates.Formatters

open System
    
let toInput = function | Some (date: DateOnly) -> date.ToString "yyyy-MM-dd" | _ -> ""
let toDisplay =  function | Some (date: DateOnly) -> date.ToShortDateString() | _ -> "-"