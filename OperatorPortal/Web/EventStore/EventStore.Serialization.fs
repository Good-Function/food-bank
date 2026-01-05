module EventStore.Serialization

open System.Text.Json
open System.Text.Json.Serialization

let private options =
    let opts = JsonSerializerOptions()
    opts.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    opts.Converters.Add(JsonFSharpConverter())
    opts

let serialize<'T> (event: 'T) : string =
    JsonSerializer.Serialize(event, options)

let deserialize<'T> (json: string) : 'T =
    JsonSerializer.Deserialize<'T>(json, options)
