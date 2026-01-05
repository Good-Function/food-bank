module EventStore.Types

open System

type AppendError =
    | ConcurrencyConflict of expected: int * actual: int
    | ProjectionFailed of eventType: string * error: exn
    | DatabaseError of error: exn
