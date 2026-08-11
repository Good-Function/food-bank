module EventSourcing.CommandHandler

open System
open EventStore.Types

type CommandHandlerConfig<'State, 'Command, 'Event, 'AggregateId> = {
    LoadEvents: Guid -> Async<'Event list>
    Replay: 'Event list -> 'State option
    Decide: 'Command -> 'State option -> Result<'Event list, string>
    Project: 'Event -> System.Data.IDbConnection -> Async<unit>
    AppendEvents: Guid -> int -> 'Event list -> Async<Result<unit, AppendError>>
    ToGuid: 'AggregateId -> Guid
    GetAggregateId: 'Command -> 'AggregateId
    AggregateType: string
}

let handleCommand<'State, 'Command, 'Event, 'AggregateId>
    (config: CommandHandlerConfig<'State, 'Command, 'Event, 'AggregateId>)
    (command: 'Command)
    : Async<Result<unit, string>> =
    async {
        let aggregateId = config.GetAggregateId command
        let guid = config.ToGuid aggregateId

        let! events = config.LoadEvents guid
        let currentState = config.Replay events
        let currentVersion = events |> List.length

        match config.Decide command currentState with
        | Error err ->
            return Error err

        | Ok newEvents ->
            let! appendResult = config.AppendEvents guid currentVersion newEvents

            return
                match appendResult with
                | Ok () ->
                    Ok ()
                | Error (ConcurrencyConflict (expected, actual)) ->
                    Error $"Concurrency conflict: expected version {expected}, actual {actual}"
                | Error (ProjectionFailed (eventType, error)) ->
                    Error $"Projection failed for {eventType}: {error.Message}"
                | Error (DatabaseError error) ->
                    Error $"Database error: {error.Message}"
    }
