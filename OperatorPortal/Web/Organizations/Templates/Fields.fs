module Organizations.Templates.Fields

open System
open Oxpecker.ViewEngine

let editField (labelText: string) (value: string) (name: string) =
    p () {
        label () { b () { labelText } }
        input (value = value, name = name)
    }
    
let booleanField (labelText: string) (value: bool) (name: string) =
    p () {
        label () { b () { labelText } }
        p (style="display: flex; gap: 25px;") {
            label () {
                input (value = "true", name = name, type'="radio", checked'=value)
                "Tak"
            }
            label () {
                input (value = "false", name = name, type'="radio", checked'=(value = false))
                "Nie"
            }
        }
    }
    
let dateField (labelText: string) (value: DateOnly option) (name: string) =
    let value = value |> function | Some date -> date.ToString "yyyy-MM-dd" | _ -> ""
    p () {
        label () { b () { labelText } }
        input (value = value , name = name, type'="date")
    }

let field (labelText: string) (value: string) =
    p () {
        label () { b () { labelText } }
        small () { value }
    }