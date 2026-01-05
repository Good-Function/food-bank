module EventSourcing.FeatureFlag

open System
open System.Security.Cryptography
open System.Text

type FeatureFlagConfig = {
    Percentage: int
    EnvVarName: string
}

let isEnabled<'AggregateId>
    (config: FeatureFlagConfig)
    (aggregateId: 'AggregateId)
    : bool =

    let percentage =
        match Environment.GetEnvironmentVariable(config.EnvVarName) with
        | null | "" -> config.Percentage
        | value ->
            match Int32.TryParse(value) with
            | true, pct -> pct
            | false, _ -> config.Percentage

    if percentage <= 0 then
        false
    elif percentage >= 100 then
        true
    else
        let idString = aggregateId.ToString()
        use sha256 = SHA256.Create()
        let hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(idString))
        let hashInt = BitConverter.ToInt32(hashBytes, 0) |> abs
        let bucket = hashInt % 100
        bucket < percentage

let executeWithFeatureFlag<'T, 'AggregateId>
    (config: FeatureFlagConfig)
    (aggregateId: 'AggregateId)
    (esHandler: unit -> Async<Result<'T, string>>)
    (legacyHandler: unit -> Async<Result<'T, string>>)
    : Async<Result<'T, string>> =

    if isEnabled config aggregateId then
        esHandler()
    else
        legacyHandler()
