module EventSourcing.Projection

open System.Data

type ProjectionFn<'Event> = 'Event -> IDbConnection -> Async<unit>

let combine<'Event>
    (projections: ProjectionFn<'Event> list)
    : ProjectionFn<'Event> =
    fun event db ->
        async {
            for projection in projections do
                do! projection event db
        }

let forEvent<'Event>
    (matcher: 'Event -> bool)
    (handler: 'Event -> IDbConnection -> Async<unit>)
    : ProjectionFn<'Event> =
    fun event db ->
        async {
            if matcher event then
                do! handler event db
        }

let noop<'Event> : ProjectionFn<'Event> =
    fun _ _ -> async { () }

let withLogging<'Event>
    (logger: string -> unit)
    (projection: ProjectionFn<'Event>)
    : ProjectionFn<'Event> =
    fun event db ->
        async {
            let eventType = event.GetType().Name
            logger $"Projecting {eventType}"
            try
                do! projection event db
                logger $"Projected {eventType} successfully"
            with ex ->
                logger $"Projection failed for {eventType}: {ex.Message}"
                return raise ex
        }
