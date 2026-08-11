module Tests.EventSourcing.CommandHandlerTests

open System
open Xunit
open FsUnit.Xunit
open EventSourcing.CommandHandler
open EventStore.Types

// Test types for command handler testing
type TestState = {
    Value: string
    Version: int
}

type TestCommand =
    | CreateTest of value: string
    | UpdateTest of value: string

type TestEvent =
    | TestCreated of value: string
    | TestUpdated of value: string

type TestAggregateId = TestAggregateId of string

// Helper to create test config
let createTestConfig
    (loadEvents: Guid -> Async<TestEvent list>)
    (replay: TestEvent list -> TestState option)
    (decide: TestCommand -> TestState option -> Result<TestEvent list, string>)
    (project: TestEvent -> System.Data.IDbConnection -> Async<unit>)
    (appendEvents: Guid -> int -> TestEvent list -> Async<Result<unit, AppendError>>)
    : CommandHandlerConfig<TestState, TestCommand, TestEvent, TestAggregateId> =
    {
        LoadEvents = loadEvents
        Replay = replay
        Decide = decide
        Project = project
        AppendEvents = appendEvents
        ToGuid = fun (TestAggregateId id) -> Guid.Parse(id)
        GetAggregateId = fun cmd ->
            match cmd with
            | CreateTest _ -> TestAggregateId "00000000-0000-0000-0000-000000000001"
            | UpdateTest _ -> TestAggregateId "00000000-0000-0000-0000-000000000001"
        AggregateType = "Test"
    }

[<Fact>]
let ``handleCommand succeeds when decide returns events and append succeeds`` () =
    async {
        // Arrange
        let mutable eventsLoaded = false
        let mutable eventsAppended = false
        let mutable projectionExecuted = false

        let loadEvents = fun _ ->
            async {
                eventsLoaded <- true
                return []
            }

        let replay = fun _ -> None

        let decide = fun cmd state ->
            match cmd with
            | CreateTest value -> Ok [ TestCreated value ]
            | UpdateTest _ -> Error "Cannot update non-existent state"

        let project = fun evt db ->
            async {
                projectionExecuted <- true
            }

        let appendEvents = fun guid version events ->
            async {
                eventsAppended <- true
                return Ok ()
            }

        let config = createTestConfig loadEvents replay decide project appendEvents
        let command = CreateTest "test-value"

        // Act
        let! result = handleCommand config command

        // Assert
        match result with
        | Ok () ->
            eventsLoaded |> should be True
            eventsAppended |> should be True
        | Error msg ->
            Assert.Fail($"Expected success, got error: {msg}")
    } |> Async.RunSynchronously

[<Fact>]
let ``handleCommand returns error when decide function rejects command`` () =
    async {
        // Arrange
        let loadEvents = fun _ -> async { return [] }
        let replay = fun _ -> None
        let decide = fun cmd state -> Error "Command rejected by business logic"
        let project = fun evt db -> async { () }
        let appendEvents = fun guid version events -> async { return Ok () }

        let config = createTestConfig loadEvents replay decide project appendEvents
        let command = CreateTest "test-value"

        // Act
        let! result = handleCommand config command

        // Assert
        match result with
        | Error msg -> msg |> should equal "Command rejected by business logic"
        | Ok () -> Assert.Fail("Expected error, got success")
    } |> Async.RunSynchronously

[<Fact>]
let ``handleCommand handles optimistic concurrency conflict from EventStore`` () =
    async {
        // Arrange
        let loadEvents = fun _ -> async { return [] }
        let replay = fun _ -> None
        let decide = fun cmd state -> Ok [ TestCreated "test" ]
        let project = fun evt db -> async { () }

        let appendEvents = fun guid version events ->
            async {
                return Error (ConcurrencyConflict (expected = 0, actual = 5))
            }

        let config = createTestConfig loadEvents replay decide project appendEvents
        let command = CreateTest "test-value"

        // Act
        let! result = handleCommand config command

        // Assert
        match result with
        | Error msg ->
            msg |> should haveSubstring "Concurrency conflict"
            msg |> should haveSubstring "expected version 0"
            msg |> should haveSubstring "actual 5"
        | Ok () -> Assert.Fail("Expected concurrency conflict error")
    } |> Async.RunSynchronously

[<Fact>]
let ``handleCommand handles projection failure error`` () =
    async {
        // Arrange
        let loadEvents = fun _ -> async { return [] }
        let replay = fun _ -> None
        let decide = fun cmd state -> Ok [ TestCreated "test" ]
        let project = fun evt db -> async { () }

        let appendEvents = fun guid version events ->
            async {
                let ex = Exception("Database connection failed")
                return Error (ProjectionFailed (eventType = "TestCreated", error = ex))
            }

        let config = createTestConfig loadEvents replay decide project appendEvents
        let command = CreateTest "test-value"

        // Act
        let! result = handleCommand config command

        // Assert
        match result with
        | Error msg ->
            msg |> should haveSubstring "Projection failed"
            msg |> should haveSubstring "TestCreated"
            msg |> should haveSubstring "Database connection failed"
        | Ok () -> Assert.Fail("Expected projection failure error")
    } |> Async.RunSynchronously

[<Fact>]
let ``handleCommand works with empty event stream (new aggregate)`` () =
    async {
        // Arrange
        let mutable appendedVersion = -1

        let loadEvents = fun _ -> async { return [] }  // Empty stream
        let replay = fun events -> None  // No state from empty events
        let decide = fun cmd state ->
            // Verify state is None for new aggregate
            state |> should equal None
            Ok [ TestCreated "new-aggregate" ]

        let project = fun evt db -> async { () }

        let appendEvents = fun guid version events ->
            async {
                appendedVersion <- version
                return Ok ()
            }

        let config = createTestConfig loadEvents replay decide project appendEvents
        let command = CreateTest "test-value"

        // Act
        let! result = handleCommand config command

        // Assert
        match result with
        | Ok () ->
            appendedVersion |> should equal 0  // Should append at version 0 for new stream
        | Error msg -> Assert.Fail($"Expected success, got error: {msg}")
    } |> Async.RunSynchronously

[<Fact>]
let ``handleCommand replays events correctly before calling decide`` () =
    async {
        // Arrange
        let existingEvents = [
            TestCreated "value1"
            TestUpdated "value2"
            TestUpdated "value3"
        ]

        let mutable replayedEvents = []
        let mutable decidedState = None

        let loadEvents = fun _ -> async { return existingEvents }

        let replay = fun events ->
            replayedEvents <- events
            Some { Value = "replayed-state"; Version = 3 }

        let decide = fun cmd state ->
            decidedState <- state
            Ok [ TestUpdated "value4" ]

        let project = fun evt db -> async { () }
        let appendEvents = fun guid version events -> async { return Ok () }

        let config = createTestConfig loadEvents replay decide project appendEvents
        let command = UpdateTest "new-value"

        // Act
        let! result = handleCommand config command

        // Assert
        match result with
        | Ok () ->
            // Verify replay was called with loaded events
            replayedEvents |> should equal existingEvents
            // Verify decide received replayed state
            decidedState |> should equal (Some { Value = "replayed-state"; Version = 3 })
        | Error msg -> Assert.Fail($"Expected success, got error: {msg}")
    } |> Async.RunSynchronously

[<Fact>]
let ``handleCommand extracts aggregate ID and converts to Guid correctly`` () =
    async {
        // Arrange
        let mutable loadedGuid = Guid.Empty
        let mutable appendedGuid = Guid.Empty

        let expectedGuid = Guid.Parse("00000000-0000-0000-0000-000000000001")

        let loadEvents = fun guid ->
            async {
                loadedGuid <- guid
                return []
            }

        let replay = fun _ -> None
        let decide = fun cmd state -> Ok [ TestCreated "test" ]
        let project = fun evt db -> async { () }

        let appendEvents = fun guid version events ->
            async {
                appendedGuid <- guid
                return Ok ()
            }

        let config = createTestConfig loadEvents replay decide project appendEvents
        let command = CreateTest "test-value"

        // Act
        let! result = handleCommand config command

        // Assert
        match result with
        | Ok () ->
            loadedGuid |> should equal expectedGuid
            appendedGuid |> should equal expectedGuid
        | Error msg -> Assert.Fail($"Expected success, got error: {msg}")
    } |> Async.RunSynchronously

[<Fact>]
let ``handleCommand handles database error from EventStore`` () =
    async {
        // Arrange
        let loadEvents = fun _ -> async { return [] }
        let replay = fun _ -> None
        let decide = fun cmd state -> Ok [ TestCreated "test" ]
        let project = fun evt db -> async { () }

        let appendEvents = fun guid version events ->
            async {
                let ex = Exception("PostgreSQL connection timeout")
                return Error (DatabaseError (error = ex))
            }

        let config = createTestConfig loadEvents replay decide project appendEvents
        let command = CreateTest "test-value"

        // Act
        let! result = handleCommand config command

        // Assert
        match result with
        | Error msg ->
            msg |> should haveSubstring "Database error"
            msg |> should haveSubstring "PostgreSQL connection timeout"
        | Ok () -> Assert.Fail("Expected database error")
    } |> Async.RunSynchronously
