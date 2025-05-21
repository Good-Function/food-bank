module Layout.Fields

open System
open Layout
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria
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
                hxEncoding = "multipart/form-data",
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
    
let passwordField (labelText: string) (name: string) (errorMsg: string option) =
    p () {
        label () { b () { labelText } }
        match errorMsg with
        | Some msg ->
            Fragment() {
                input (name = name, type' = "password", ariaInvalid = "true", ariaDescribedBy = name + "-error")
                small (id = name + "-error") { msg }
            }
        | None -> input (name = name, type' = "password", required=true)
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
    
let dateField (value: DateOnly option) (name: string) =
    let value = value |> function | Some date -> date.ToString "yyyy-MM-dd" | _ -> ""
    input (value = value , name = name, type'="date", style="padding: 5px; margin: 0; height: 40px;")

let readonlyField (labelText: string) (value: string) =
    p () {
        label () { b () { labelText } }
        small () { value }
    }