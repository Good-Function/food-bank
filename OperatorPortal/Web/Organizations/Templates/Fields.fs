module Organizations.Templates.Fields

open Oxpecker.ViewEngine

let editField (labelText: string) (value: string) (name: string) =
    p () {
        label () { b () { labelText } }
        input (value = value, name = name)
    }

let field (labelText: string) (value: string) =
    p () {
        label () { b () { labelText } }
        small () { value }
    }