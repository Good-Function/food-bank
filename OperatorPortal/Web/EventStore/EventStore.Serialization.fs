module EventStore.Serialization

open Thoth.Json.Net

let serialize<'T> (event: 'T) : string =
    Encode.Auto.toString(0, event, caseStrategy = CaseStrategy.CamelCase)

let deserialize<'T> (json: string) : 'T =
    match Decode.Auto.fromString<'T>(json, caseStrategy = CaseStrategy.CamelCase) with
    | Ok value -> value
    | Error error -> failwith $"Failed to deserialize event: {error}"
