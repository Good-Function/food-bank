module EventStoreTests

open System
open System.Data
open Xunit
open FsUnit.Xunit
open PostgresPersistence.DapperFsharp
open Tools.DbConnection
open EventStore.Types

type TestEvent = {
    Data: string
    Timestamp: DateTime
}

type private EventRow = {
    event_data: string
    version: int
}
let insertEvent (aggregateType: string) (aggregateId: Guid) (version: int) (event: TestEvent) =
    async {
        use! db = connectDb()
        let eventData = EventStore.Serialization.serialize event
        do! db.Execute """INSERT INTO events (
                event_id, aggregate_type, aggregate_id,
                event_type, event_data, version, occurred_at
            ) VALUES (
                gen_random_uuid(), @aggregateType, @aggregateId,
                @eventType, @eventData::jsonb, @version, @occurredAt
            )""" {| aggregateType = aggregateType
                    aggregateId = aggregateId
                    eventType = "TestEvent"
                    eventData = eventData
                    version = version
                    occurredAt = DateTime.UtcNow |}
    }

[<Fact>]
let ``loadEvents returns empty list when no events exist`` () =
    task {
        // Arrange
        let aggregateId = Guid.NewGuid()

        // Act
        let! events = EventStore.Core.loadEvents<TestEvent> connectDb "Order" aggregateId

        // Assert
        events |> should be Empty
    }

[<Fact>]
let ``loadEvents returns events in version order`` () =
    task {
        // Arrange
        let aggregateId = Guid.NewGuid()
        do! insertEvent "Order" aggregateId 3 { Data = "Event3"; Timestamp = DateTime.UtcNow }
        do! insertEvent "Order" aggregateId 1 { Data = "Event1"; Timestamp = DateTime.UtcNow }
        do! insertEvent "Order" aggregateId 2 { Data = "Event2"; Timestamp = DateTime.UtcNow }

        // Act
        let! events = EventStore.Core.loadEvents<TestEvent> connectDb "Order" aggregateId

        // Assert
        let eventsList = events |> List.ofSeq
        eventsList |> should haveLength 3
        eventsList.[0].Data |> should equal "Event1"
        eventsList.[1].Data |> should equal "Event2"
        eventsList.[2].Data |> should equal "Event3"
    }

[<Fact>]
let ``loadEvents filters by aggregate_type and aggregate_id`` () =
    task {
        // Arrange
        let aggregateId1 = Guid.NewGuid()
        let aggregateId2 = Guid.NewGuid()
        do! insertEvent "Order" aggregateId1 1 { Data = "Order1"; Timestamp = DateTime.UtcNow }
        do! insertEvent "Order" aggregateId2 1 { Data = "Order2"; Timestamp = DateTime.UtcNow }
        do! insertEvent "Customer" aggregateId1 1 { Data = "Customer1"; Timestamp = DateTime.UtcNow }

        // Act
        let! events = EventStore.Core.loadEvents<TestEvent> connectDb "Order" aggregateId1

        // Assert
        let eventsList = events |> List.ofSeq
        eventsList |> should haveLength 1
        eventsList.[0].Data |> should equal "Order1"
    }

[<Fact>]
let ``loadEvents deserializes event_data correctly`` () =
    task {
        // Arrange
        let aggregateId = Guid.NewGuid()
        let expectedTimestamp = DateTime(2026, 1, 4, 10, 30, 0, DateTimeKind.Utc)
        do! insertEvent "Order" aggregateId 1 { Data = "TestData"; Timestamp = expectedTimestamp }

        // Act
        let! events = EventStore.Core.loadEvents<TestEvent> connectDb "Order" aggregateId

        // Assert
        let eventsList = events |> List.ofSeq
        eventsList |> should haveLength 1
        eventsList.[0].Data |> should equal "TestData"
        eventsList.[0].Timestamp.Year |> should equal 2026
        eventsList.[0].Timestamp.Month |> should equal 1
        eventsList.[0].Timestamp.Day |> should equal 4
    }

[<Fact>]
let ``loadEvents handles multiple aggregates independently`` () =
    task {
        // Arrange
        let aggregateId1 = Guid.NewGuid()
        let aggregateId2 = Guid.NewGuid()
        do! insertEvent "Order" aggregateId1 1 { Data = "Agg1-Event1"; Timestamp = DateTime.UtcNow }
        do! insertEvent "Order" aggregateId1 2 { Data = "Agg1-Event2"; Timestamp = DateTime.UtcNow }
        do! insertEvent "Order" aggregateId2 1 { Data = "Agg2-Event1"; Timestamp = DateTime.UtcNow }

        // Act
        let! events1 = EventStore.Core.loadEvents<TestEvent> connectDb "Order" aggregateId1
        let! events2 = EventStore.Core.loadEvents<TestEvent> connectDb "Order" aggregateId2

        // Assert
        let eventsList1 = events1 |> List.ofSeq
        let eventsList2 = events2 |> List.ofSeq
        eventsList1 |> should haveLength 2
        eventsList1.[0].Data |> should equal "Agg1-Event1"
        eventsList1.[1].Data |> should equal "Agg1-Event2"

        eventsList2 |> should haveLength 1
        eventsList2.[0].Data |> should equal "Agg2-Event1"
    }

[<Fact>]
let ``getCurrentVersion returns 0 when no events exist`` () =
    task {
        // Arrange
        let aggregateId = Guid.NewGuid()

        // Act
        let! version = EventStore.Core.getCurrentVersion connectDb "Order" aggregateId

        // Assert
        version |> should equal 0
    }

[<Fact>]
let ``getCurrentVersion returns max version when events exist`` () =
    task {
        // Arrange
        let aggregateId = Guid.NewGuid()
        do! insertEvent "Order" aggregateId 1 { Data = "Event1"; Timestamp = DateTime.UtcNow }
        do! insertEvent "Order" aggregateId 2 { Data = "Event2"; Timestamp = DateTime.UtcNow }
        do! insertEvent "Order" aggregateId 3 { Data = "Event3"; Timestamp = DateTime.UtcNow }

        // Act
        let! version = EventStore.Core.getCurrentVersion connectDb "Order" aggregateId

        // Assert
        version |> should equal 3
    }

[<Fact>]
let ``getCurrentVersion filters by aggregate_type and aggregate_id`` () =
    task {
        // Arrange
        let aggregateId1 = Guid.NewGuid()
        let aggregateId2 = Guid.NewGuid()
        do! insertEvent "Order" aggregateId1 1 { Data = "Order1"; Timestamp = DateTime.UtcNow }
        do! insertEvent "Order" aggregateId1 2 { Data = "Order2"; Timestamp = DateTime.UtcNow }
        do! insertEvent "Order" aggregateId2 1 { Data = "Order3"; Timestamp = DateTime.UtcNow }
        do! insertEvent "Customer" aggregateId1 1 { Data = "Customer1"; Timestamp = DateTime.UtcNow }

        // Act
        let! version = EventStore.Core.getCurrentVersion connectDb "Order" aggregateId1

        // Assert
        version |> should equal 2
    }

[<Fact>]
let ``appendEvents succeeds when expected version matches`` () =
    task {
        // Arrange
        let aggregateId = Guid.NewGuid()
        do! insertEvent "Order" aggregateId 1 { Data = "Event1"; Timestamp = DateTime.UtcNow }

        let newEvents = [
            { Data = "Event2"; Timestamp = DateTime.UtcNow }
        ]
        let projection = fun _ _ -> async { () }

        // Act
        let! result = EventStore.Core.appendEvents connectDb "Order" aggregateId 1 newEvents projection

        // Assert
        match result with
        | Ok _ -> ()
        | Error msg -> failwith $"Expected Ok but got Error: {msg}"

        let! version = EventStore.Core.getCurrentVersion connectDb "Order" aggregateId
        version |> should equal 2
    }

[<Fact>]
let ``appendEvents fails with concurrency conflict when version mismatch`` () =
    task {
        // Arrange
        let aggregateId = Guid.NewGuid()
        do! insertEvent "Order" aggregateId 1 { Data = "Event1"; Timestamp = DateTime.UtcNow }

        let newEvents = [
            { Data = "Event2"; Timestamp = DateTime.UtcNow }
        ]
        let projection = fun _ _ -> async { () }

        // Act - Try to append at version 0 (but current is 1)
        let! result = EventStore.Core.appendEvents connectDb "Order" aggregateId 0 newEvents projection

        // Assert
        match result with
        | Error (ConcurrencyConflict _) -> ()
        | Error other -> failwith $"Expected ConcurrencyConflict but got: {other}"
        | Ok _ -> failwith "Should have failed with concurrency conflict"

        let! version = EventStore.Core.getCurrentVersion connectDb "Order" aggregateId
        version |> should equal 1
    }

[<Fact>]
let ``appendEvents executes projection inline within same transaction`` () =
    task {
        // Arrange
        let aggregateId = Guid.NewGuid()
        let mutable projectionExecuted = false

        let newEvents = [
            { Data = "Event1"; Timestamp = DateTime.UtcNow }
        ]
        let projection = fun (event: TestEvent) (db: IDbConnection) ->
            async {
                projectionExecuted <- true
                // Simple projection that just sets the flag
                ()
            }

        // Act
        let! result = EventStore.Core.appendEvents connectDb "Order" aggregateId 0 newEvents projection

        // Assert
        match result with
        | Ok _ -> ()
        | Error msg -> failwith $"Expected Ok but got Error: {msg}"

        projectionExecuted |> should equal true
    }

[<Fact>]
let ``appendEvents rolls back on projection failure`` () =
    task {
        // Arrange
        let aggregateId = Guid.NewGuid()

        let newEvents = [
            { Data = "Event1"; Timestamp = DateTime.UtcNow }
        ]
        let projection = fun _ _ ->
            async {
                failwith "Projection failed"
            }

        // Act
        let! result = EventStore.Core.appendEvents connectDb "Order" aggregateId 0 newEvents projection

        // Assert
        match result with
        | Error (ProjectionFailed _) -> ()
        | Error (DatabaseError _) -> ()
        | Error other -> failwith $"Expected ProjectionFailed or DatabaseError but got: {other}"
        | Ok _ -> failwith "Should have failed due to projection failure"

        let! version = EventStore.Core.getCurrentVersion connectDb "Order" aggregateId
        version |> should equal 0
    }

[<Fact>]
let ``appendEvents increments version for multiple events`` () =
    task {
        // Arrange
        let aggregateId = Guid.NewGuid()

        let newEvents = [
            { Data = "Event1"; Timestamp = DateTime.UtcNow }
            { Data = "Event2"; Timestamp = DateTime.UtcNow }
            { Data = "Event3"; Timestamp = DateTime.UtcNow }
        ]
        let projection = fun _ _ -> async { () }

        // Act
        let! result = EventStore.Core.appendEvents connectDb "Order" aggregateId 0 newEvents projection

        // Assert
        match result with
        | Ok _ -> ()
        | Error msg -> failwith $"Expected Ok but got Error: {msg}"

        let! version = EventStore.Core.getCurrentVersion connectDb "Order" aggregateId
        version |> should equal 3

        // Verify all events were inserted with correct versions
        let! events = EventStore.Core.loadEvents<TestEvent> connectDb "Order" aggregateId
        let eventsList = events |> List.ofSeq
        eventsList |> should haveLength 3
        eventsList.[0].Data |> should equal "Event1"
        eventsList.[1].Data |> should equal "Event2"
        eventsList.[2].Data |> should equal "Event3"
    }

[<Fact>]
let ``appendEvents handles PostgreSQL unique constraint violation`` () =
    task {
        // Arrange
        let aggregateId = Guid.NewGuid()
        do! insertEvent "Order" aggregateId 1 { Data = "Event1"; Timestamp = DateTime.UtcNow }

        // Manually insert an event at version 2
        do! insertEvent "Order" aggregateId 2 { Data = "Event2"; Timestamp = DateTime.UtcNow }

        let newEvents = [
            { Data = "Event3"; Timestamp = DateTime.UtcNow }
        ]
        let projection = fun _ _ -> async { () }

        // Act - Try to append at version 1 (which would create version 2, but it already exists)
        let! result = EventStore.Core.appendEvents connectDb "Order" aggregateId 1 newEvents projection

        // Assert
        match result with
        | Error (ConcurrencyConflict _) -> ()
        | Error other -> failwith $"Expected ConcurrencyConflict but got: {other}"
        | Ok _ -> failwith "Should have failed with concurrency conflict"
    }
