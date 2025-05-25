module Organizations.Database.DateOnlyCoder

open System
open Thoth.Json.Net

module DateOnlyCodec =
    let encode (date: DateOnly) : JsonValue =
        Encode.string (date.ToString("yyyy-MM-dd"))

    let decode : Decoder<DateOnly> =
        Decode.string
        |> Decode.andThen (fun str ->
            match DateOnly.TryParse str with
            | true, value -> Decode.succeed value
            | false, _ -> Decode.fail $"Invalid DateOnly string: {str}"
        )
