module Organizations.Templates.Fields

open System
open Layout
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let editableHeader (title: string) (formPath: string) =
    header (class' = "action-header") {
        span () { title }
        div (class' = "action-header-actions") {
            span (
                hxGet = formPath,
                hxTarget = "closest article",
                hxSwap = "outerHTML"
            ) {
                Icons.Pen
            }
        }
    }

let activeEditableHeader (title: string) (formPath: string) =
     header (class' = "action-header") {
        span () { title }
        div (class' = "action-header-actions") {
            span (
                hxGet = formPath,
                hxTarget = "closest article",
                hxSwap = "outerHTML"
            ) {
                Icons.Cancel
            }

            span (
                hxPut = formPath,
                hxTarget = "closest article",
                hxSwap = "outerHTML"
            ) {
                Icons.Ok
            }
        }
    }

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

let readonlyField (labelText: string) (value: string) =
    p () {
        label () { b () { labelText } }
        small () { value }
    }