module Organizations.Application.ReadModels.FilterOperators

open Microsoft.FSharp.Reflection

type TextOperator =
    | Contains
    | NotContains
    member this.Label =
        match this with
        | Contains -> "zawiera"
        | NotContains -> "nie zawiera"
    member this.Symbol =
        match this with
        | Contains -> "ILIKE"
        | NotContains -> "NOT ILIKE"
    static member TryParse(text: string) =
        match text with
        | "zawiera" -> Some Contains
        | "nie zawiera" -> Some NotContains
        | _ -> None
        
type NumberOperator =
    | Equal
    | LessThan
    | GreaterThan
    | LessThanOrEqual
    | GreaterThanOrEqual
    member this.Symbol =
        match this with
        | Equal -> "="
        | LessThan -> "<"
        | GreaterThan -> ">"
        | LessThanOrEqual -> "<="
        | GreaterThanOrEqual -> ">="
    member this.Label = this.Symbol
    static member TryParse(text: string) =
        match text with
        | "=" -> Some Equal
        | "<" -> Some LessThan
        | ">" -> Some GreaterThan
        | "<=" -> Some LessThanOrEqual
        | ">=" -> Some GreaterThanOrEqual
        | _ -> None
        
type FilterOperator =
    | NumberOperator of NumberOperator
    | TextOperator of TextOperator
    static member TryParse(text: string) =
        NumberOperator.TryParse(text)
        |> Option.map NumberOperator
        |> Option.orElse (
            TextOperator.TryParse(text)
            |> Option.map TextOperator
        )
    member this.Symbol =
        match this with
        | NumberOperator op -> op.Symbol
        | TextOperator op -> op.Symbol
        
let textOperators =
    FSharpType.GetUnionCases(typeof<TextOperator>)
    |> Array.map(fun case -> FSharpValue.MakeUnion(case, [||]) :?> TextOperator)
    |> Array.toList
    
let numberOperators =
    FSharpType.GetUnionCases(typeof<NumberOperator>)
    |> Array.map(fun case -> FSharpValue.MakeUnion(case, [||]) :?> NumberOperator)
    |> Array.toList