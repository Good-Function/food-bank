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

let unionToArray<'T> =
    FSharpType.GetUnionCases(typeof<'T>)
    |> Array.map (fun case -> FSharpValue.MakeUnion(case, [||]) :?> 'T)

let Template (currentPath: Page option) =
    nav (hxBoost = true) {
        ul () {
            for path in unionToArray<Page> do
                li () {
                    a (
                        href = path.ToPath(),
                        class' = "nav-link " + if Some path = currentPath then "active" else ""
                    ) {
                        path.ToTitle()
                    }
                }
        }
    }