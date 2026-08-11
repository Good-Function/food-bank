module Tests.EventSourcing.FeatureFlagTests

open System
open Xunit
open FsUnit.Xunit
open EventSourcing.FeatureFlag

[<Fact>]
let ``isEnabled returns false for all IDs when percentage is 0`` () =
    // Arrange
    let config = { Percentage = 0; EnvVarName = "TEST_FLAG_ZERO" }
    let ids = [ "id1"; "id2"; "id3"; "id4"; "id5" ]

    // Act & Assert
    for id in ids do
        isEnabled config id |> should be False

[<Fact>]
let ``isEnabled returns true for all IDs when percentage is 100`` () =
    // Arrange
    let config = { Percentage = 100; EnvVarName = "TEST_FLAG_FULL" }
    let ids = [ "id1"; "id2"; "id3"; "id4"; "id5" ]

    // Act & Assert
    for id in ids do
        isEnabled config id |> should be True

[<Fact>]
let ``isEnabled returns same result for same ID (deterministic)`` () =
    // Arrange
    let config = { Percentage = 50; EnvVarName = "TEST_FLAG_DETERMINISTIC" }
    let aggregateId = "test-123"

    // Act
    let result1 = isEnabled config aggregateId
    let result2 = isEnabled config aggregateId
    let result3 = isEnabled config aggregateId

    // Assert
    result1 |> should equal result2
    result2 |> should equal result3

[<Fact>]
let ``isEnabled distributes roughly evenly at 50%`` () =
    // Arrange
    let config = { Percentage = 50; EnvVarName = "TEST_FLAG_DISTRIBUTION" }
    let ids = [ for i in 1..1000 -> $"id-{i}" ]

    // Act
    let enabledCount =
        ids
        |> List.filter (isEnabled config)
        |> List.length

    let percentage = (float enabledCount / float ids.Length) * 100.0

    // Assert - Should be roughly 50% (allow 5% variance)
    percentage |> should be (greaterThan 45.0)
    percentage |> should be (lessThan 55.0)

[<Fact>]
let ``Environment variable overrides config percentage when set to 0`` () =
    // Arrange
    let envVarName = "TEST_FLAG_ENV_OVERRIDE_0"
    Environment.SetEnvironmentVariable(envVarName, "0")

    try
        let config = { Percentage = 100; EnvVarName = envVarName }
        let id = "test-id"

        // Act
        let result = isEnabled config id

        // Assert
        result |> should be False
    finally
        Environment.SetEnvironmentVariable(envVarName, null)

[<Fact>]
let ``Environment variable overrides config percentage when set to 100`` () =
    // Arrange
    let envVarName = "TEST_FLAG_ENV_OVERRIDE_100"
    Environment.SetEnvironmentVariable(envVarName, "100")

    try
        let config = { Percentage = 0; EnvVarName = envVarName }
        let id = "test-id"

        // Act
        let result = isEnabled config id

        // Assert
        result |> should be True
    finally
        Environment.SetEnvironmentVariable(envVarName, null)

[<Fact>]
let ``Environment variable with invalid value falls back to config percentage`` () =
    // Arrange
    let envVarName = "TEST_FLAG_ENV_INVALID"
    Environment.SetEnvironmentVariable(envVarName, "invalid")

    try
        let config = { Percentage = 100; EnvVarName = envVarName }
        let id = "test-id"

        // Act
        let result = isEnabled config id

        // Assert - Should use config percentage (100)
        result |> should be True
    finally
        Environment.SetEnvironmentVariable(envVarName, null)

[<Fact>]
let ``executeWithFeatureFlag routes to ES handler when enabled`` () =
    async {
        // Arrange
        let mutable esHandlerCalled = false
        let mutable legacyHandlerCalled = false

        let config = { Percentage = 100; EnvVarName = "TEST_FLAG_ES_ENABLED" }
        let aggregateId = "test-id"

        let esHandler = fun () ->
            async {
                esHandlerCalled <- true
                return Ok "ES result"
            }

        let legacyHandler = fun () ->
            async {
                legacyHandlerCalled <- true
                return Ok "Legacy result"
            }

        // Act
        let! result = executeWithFeatureFlag config aggregateId esHandler legacyHandler

        // Assert
        esHandlerCalled |> should be True
        legacyHandlerCalled |> should be False
        match result with
        | Ok value -> value |> should equal "ES result"
        | Error msg -> Assert.Fail($"Expected Ok, got Error: {msg}")
    } |> Async.RunSynchronously

[<Fact>]
let ``executeWithFeatureFlag routes to legacy handler when disabled`` () =
    async {
        // Arrange
        let mutable esHandlerCalled = false
        let mutable legacyHandlerCalled = false

        let config = { Percentage = 0; EnvVarName = "TEST_FLAG_ES_DISABLED" }
        let aggregateId = "test-id"

        let esHandler = fun () ->
            async {
                esHandlerCalled <- true
                return Ok "ES result"
            }

        let legacyHandler = fun () ->
            async {
                legacyHandlerCalled <- true
                return Ok "Legacy result"
            }

        // Act
        let! result = executeWithFeatureFlag config aggregateId esHandler legacyHandler

        // Assert
        esHandlerCalled |> should be False
        legacyHandlerCalled |> should be True
        match result with
        | Ok value -> value |> should equal "Legacy result"
        | Error msg -> Assert.Fail($"Expected Ok, got Error: {msg}")
    } |> Async.RunSynchronously

[<Fact>]
let ``isEnabled with Guid aggregate ID is deterministic`` () =
    // Arrange
    let config = { Percentage = 50; EnvVarName = "TEST_FLAG_GUID" }
    let aggregateId = Guid.Parse("12345678-1234-1234-1234-123456789012")

    // Act
    let result1 = isEnabled config aggregateId
    let result2 = isEnabled config aggregateId
    let result3 = isEnabled config aggregateId

    // Assert
    result1 |> should equal result2
    result2 |> should equal result3

[<Fact>]
let ``isEnabled with int aggregate ID is deterministic`` () =
    // Arrange
    let config = { Percentage = 50; EnvVarName = "TEST_FLAG_INT" }
    let aggregateId = 12345

    // Act
    let result1 = isEnabled config aggregateId
    let result2 = isEnabled config aggregateId
    let result3 = isEnabled config aggregateId

    // Assert
    result1 |> should equal result2
    result2 |> should equal result3
