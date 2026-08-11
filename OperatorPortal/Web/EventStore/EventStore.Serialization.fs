module EventStore.Serialization

open Thoth.Json.Net

let private extraCoders =
    Extra.empty
    |> Extra.withInt64

let serialize<'T> (event: 'T) : string =
    Encode.Auto.toString(0, event, caseStrategy = CaseStrategy.CamelCase, extra = extraCoders)

let deserialize<'T> (json: string) : 'T =
    match Decode.Auto.fromString<'T>(json, caseStrategy = CaseStrategy.CamelCase, extra = extraCoders) with
    | Ok value -> value
    | Error error -> failwith $"Failed to deserialize event: {error}"
