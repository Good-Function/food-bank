module Layout.Navigation

open Microsoft.FSharp.Reflection
open Oxpecker.ViewEngine
open Oxpecker.Htmx

type Page =
    | Organizations
    | Applications
    | Team

    member this.ToPath() =
        match this with
        | Organizations -> "/organizations"
        | Applications -> "/applications"
        | Team -> "/team"

    member this.ToTitle() =
        match this with
        | Organizations -> "Organizacje"
        | Applications -> "Wnioski"
        | Team -> "Zespół"
        
    member this.Icon() =
        match this with
        | Organizations -> Icons.book
        | Applications -> Icons.food
        | Team -> Icons.group   

let unionToArray<'T> =
    FSharpType.GetUnionCases(typeof<'T>)
    |> Array.map (fun case -> FSharpValue.MakeUnion(case, [||]) :?> 'T)

let Template (currentPath: Page option) =
    nav (hxBoost = true) {
        ul () {
            for path in unionToArray<Page> do
                li (style="padding: 0px 0px 8px 0px; box-sizing:content-box;", class' = if Some path = currentPath then "active" else "") {
                    a (
                        href = path.ToPath(),
                        style = "text-align:center; height: 40px; padding:0; padding-top:4px;"
                    ) {
                        path.Icon()
                        small () { path.ToTitle() }
                    }
                }
        }
    }