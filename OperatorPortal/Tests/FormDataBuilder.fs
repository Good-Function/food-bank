module FormDataBuilder

open System.Collections.Generic
open System.Net.Http

type FormDataBuilder() =
    member _.Yield(key, value) = [ (key, value) ]
    member _.Combine(state, item) = item @ state
    member _.Delay(f) = f()
    member _.Run(values) =
        let list =
            values
            |> List.rev
            |> List.map (fun (key, value) -> KeyValuePair<string, string>(key, value))
        new FormUrlEncodedContent(list)

let formData = FormDataBuilder()