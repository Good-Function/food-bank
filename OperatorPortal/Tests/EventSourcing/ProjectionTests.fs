module Tests.EventSourcing.ProjectionTests

open System
open System.Data
open Xunit
open FsUnit.Xunit
open EventSourcing.Projection

// Test event type
type TestEvent =
    | EventA of value: string
    | EventB of value: int
    | EventC of value: bool

// Mock database connection for testing
type MockDbConnection() =
    interface IDbConnection with
        member _.ConnectionString with get() = "" and set(_) = ()
        member _.ConnectionTimeout = 0
        member _.Database = "test"
        member _.State = ConnectionState.Open
        member _.BeginTransaction() = null
        member _.BeginTransaction(_) = null
        member _.ChangeDatabase(_) = ()
        member _.Close() = ()
        member _.CreateCommand() = null
        member _.Open() = ()
        member _.Dispose() = ()

[<Fact>]
let ``combine executes all projections in sequence`` () =
    async {
        // Arrange
        let mutable projection1Called = false
        let mutable projection2Called = false
        let mutable projection3Called = false

        let projection1 : ProjectionFn<TestEvent> =
            fun evt db ->
                async {
                    projection1Called <- true
                }

        let projection2 : ProjectionFn<TestEvent> =
            fun evt db ->
                async {
                    projection2Called <- true
                }

        let projection3 : ProjectionFn<TestEvent> =
            fun evt db ->
                async {
                    projection3Called <- true
                }

        let combined = combine [ projection1; projection2; projection3 ]

        use db = new MockDbConnection()

        // Act
        do! combined (EventA "test") db

        // Assert
        projection1Called |> should be True
        projection2Called |> should be True
        projection3Called |> should be True
    } |> Async.RunSynchronously

[<Fact>]
let ``combine stops on first projection failure`` () =
    async {
        // Arrange
        let mutable projection1Called = false
        let mutable projection2Called = false
        let mutable projection3Called = false

        let projection1 : ProjectionFn<TestEvent> =
            fun evt db ->
                async {
                    projection1Called <- true
                }

        let projection2 : ProjectionFn<TestEvent> =
            fun evt db ->
                async {
                    projection2Called <- true
                    failwith "Projection 2 failed"
                }

        let projection3 : ProjectionFn<TestEvent> =
            fun evt db ->
                async {
                    projection3Called <- true
                }

        let combined = combine [ projection1; projection2; projection3 ]

        use db = new MockDbConnection()

        // Act & Assert
        try
            do! combined (EventA "test") db
            Assert.Fail("Expected exception from projection2")
        with
        | ex ->
            ex.Message |> should equal "Projection 2 failed"
            projection1Called |> should be True
            projection2Called |> should be True
            projection3Called |> should be False  // Should not be called after failure
    } |> Async.RunSynchronously

[<Fact>]
let ``forEvent only executes handler when matcher returns true`` () =
    async {
        // Arrange
        let mutable handlerCalled = false

        let matcher = function
            | EventA _ -> true
            | _ -> false

        let handler : TestEvent -> IDbConnection -> Async<unit> =
            fun evt db ->
                async {
                    handlerCalled <- true
                }

        let projection = forEvent matcher handler

        use db = new MockDbConnection()

        // Act
        do! projection (EventA "test") db

        // Assert
        handlerCalled |> should be True
    } |> Async.RunSynchronously

[<Fact>]
let ``forEvent skips handler when matcher returns false`` () =
    async {
        // Arrange
        let mutable handlerCalled = false

        let matcher = function
            | EventA _ -> true
            | _ -> false

        let handler : TestEvent -> IDbConnection -> Async<unit> =
            fun evt db ->
                async {
                    handlerCalled <- true
                }

        let projection = forEvent matcher handler

        use db = new MockDbConnection()

        // Act
        do! projection (EventB 123) db  // EventB doesn't match

        // Assert
        handlerCalled |> should be False
    } |> Async.RunSynchronously

[<Fact>]
let ``withLogging logs start and success`` () =
    async {
        // Arrange
        let mutable logMessages = []

        let logger msg =
            logMessages <- msg :: logMessages

        let projection : ProjectionFn<TestEvent> =
            fun evt db ->
                async {
                    ()  // Simple success
                }

        let loggingProjection = withLogging logger projection

        use db = new MockDbConnection()

        // Act
        do! loggingProjection (EventA "test") db

        // Assert
        logMessages |> List.rev |> List.length |> should equal 2
        logMessages |> List.exists (fun msg -> msg.Contains("Projecting")) |> should be True
        logMessages |> List.exists (fun msg -> msg.Contains("successfully")) |> should be True
    } |> Async.RunSynchronously

[<Fact>]
let ``withLogging logs failure and rethrows`` () =
    async {
        // Arrange
        let mutable logMessages = []

        let logger msg =
            logMessages <- msg :: logMessages

        let projection : ProjectionFn<TestEvent> =
            fun evt db ->
                async {
                    failwith "Test projection failure"
                }

        let loggingProjection = withLogging logger projection

        use db = new MockDbConnection()

        // Act & Assert
        try
            do! loggingProjection (EventA "test") db
            Assert.Fail("Expected exception to be rethrown")
        with
        | ex ->
            ex.Message |> should equal "Test projection failure"
            logMessages |> List.exists (fun msg -> msg.Contains("Projecting")) |> should be True
            logMessages |> List.exists (fun msg -> msg.Contains("failed")) |> should be True
    } |> Async.RunSynchronously

[<Fact>]
let ``noop projection always succeeds`` () =
    async {
        // Arrange
        use db = new MockDbConnection()

        // Act & Assert - Should not throw
        do! noop (EventA "test") db
        do! noop (EventB 123) db
        do! noop (EventC true) db

        // If we got here, all noops succeeded
        true |> should be True
    } |> Async.RunSynchronously

[<Fact>]
let ``combine with empty list returns noop projection`` () =
    async {
        // Arrange
        let combined = combine []
        use db = new MockDbConnection()

        // Act & Assert - Should not throw
        do! combined (EventA "test") db

        // If we got here, the empty combine succeeded
        true |> should be True
    } |> Async.RunSynchronously

[<Fact>]
let ``forEvent can be combined with other projections`` () =
    async {
        // Arrange
        let mutable eventACalled = false
        let mutable allEventsCalled = false

        let eventAHandler =
            forEvent
                (function | EventA _ -> true | _ -> false)
                (fun evt db -> async { eventACalled <- true })

        let allEventsHandler : ProjectionFn<TestEvent> =
            fun evt db -> async { allEventsCalled <- true }

        let combined = combine [ eventAHandler; allEventsHandler ]

        use db = new MockDbConnection()

        // Act
        do! combined (EventA "test") db

        // Assert
        eventACalled |> should be True
        allEventsCalled |> should be True
    } |> Async.RunSynchronously

[<Fact>]
let ``withLogging preserves projection behavior`` () =
    async {
        // Arrange
        let mutable projectionExecuted = false
        let mutable capturedEvent = None

        let projection : ProjectionFn<TestEvent> =
            fun evt db ->
                async {
                    projectionExecuted <- true
                    capturedEvent <- Some evt
                }

        let loggingProjection = withLogging (fun _ -> ()) projection

        use db = new MockDbConnection()
        let testEvent = EventB 42

        // Act
        do! loggingProjection testEvent db

        // Assert
        projectionExecuted |> should be True
        capturedEvent |> should equal (Some testEvent)
    } |> Async.RunSynchronously
