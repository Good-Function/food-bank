module EventStore.Core

open System
open System.Data
open System.Transactions
open Npgsql
open PostgresPersistence.DapperFsharp
open EventStore.Serialization
open EventStore.Types

type private EventRow = {
    event_data: string
    version: int
}

let loadEvents<'EventData>
    (connectDb: unit -> Async<IDbConnection>)
    (aggregateType: string)
    (aggregateId: Guid)
    : Async<'EventData list> =
    async {
        use! db = connectDb()
        let! rows =
            db.QueryBy<EventRow>
                """SELECT event_data, version
                   FROM events
                   WHERE aggregate_type = @aggregateType
                     AND aggregate_id = @aggregateId
                   ORDER BY version ASC"""
                {| aggregateType = aggregateType; aggregateId = aggregateId |}
        return
            rows
            |> List.map (fun row -> deserialize<'EventData> row.event_data)
    }

let getCurrentVersion
    (connectDb: unit -> Async<IDbConnection>)
    (aggregateType: string)
    (aggregateId: Guid)
    : Async<int> =
    async {
        use! db = connectDb()
        let! result =
            db.trySingle<int>
                """SELECT COALESCE(MAX(version), 0)
                   FROM events
                   WHERE aggregate_type = @aggregateType
                     AND aggregate_id = @aggregateId"""
                {| aggregateType = aggregateType; aggregateId = aggregateId |}
        return result |> Option.defaultValue 0
    }

let private insertEvent<'EventData>
    (db: IDbConnection)
    (aggregateType: string)
    (aggregateId: Guid)
    (version: int)
    (event: 'EventData)
    : Async<unit> =
    let eventId = Guid.NewGuid()
    let eventData = serialize event
    let eventType = typeof<'EventData>.Name

    db.Execute
        """INSERT INTO events (
            event_id, aggregate_type, aggregate_id,
            event_type, event_data, version, occurred_at
         ) VALUES (
            @eventId, @aggregateType, @aggregateId,
            @eventType, @eventData::jsonb, @version, @occurredAt
         )"""
        {| eventId = eventId
           aggregateType = aggregateType
           aggregateId = aggregateId
           eventType = eventType
           eventData = eventData
           version = version
           occurredAt = DateTime.UtcNow |}

let private findConcurrencyConflict (ex: exn) : AppendError option =
    let rec find (ex: exn) =
        match ex with
        | :? PostgresException as pgEx when pgEx.SqlState = "23505" ->
            Some (ConcurrencyConflict (-1, -1))
        | :? AggregateException as aggEx ->
            aggEx.InnerExceptions |> Seq.tryPick find
        | _ when ex.InnerException <> null ->
            find ex.InnerException
        | _ -> None
    find ex

let appendEvents<'EventData>
    (connectDb: unit -> Async<IDbConnection>)
    (aggregateType: string)
    (aggregateId: Guid)
    (expectedVersion: int)
    (events: 'EventData list)
    (project: 'EventData -> IDbConnection -> Async<unit>)
    : Async<Result<unit, AppendError>> =
    async {
        use transaction = new TransactionScope(
            TransactionScopeAsyncFlowOption.Enabled)

        try
            use! db = connectDb()

            for i, event in List.indexed events do
                let version = expectedVersion + i + 1
                do! insertEvent db aggregateType aggregateId version event
                do! project event db

            transaction.Complete()
            return Ok ()

        with
        | :? PostgresException as ex when ex.SqlState = "23505" ->
            return Error (ConcurrencyConflict (expectedVersion, -1))
        | :? AggregateException as ex ->
            match findConcurrencyConflict ex with
            | Some error -> return Error error
            | None -> return Error (DatabaseError ex)
        | ex ->
            let eventType = typeof<'EventData>.Name
            return Error (ProjectionFailed (eventType, ex))
    }
