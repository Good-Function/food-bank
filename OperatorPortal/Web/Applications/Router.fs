module Applications.Router

open CompositionRoot
open Oxpecker
open RenderBasedOnHtmx

let renderApplications (testRead: Async<string list>): EndpointHandler =
    fun ctx -> task {
        let! rows = testRead
        return ctx |> render (Template.Partial rows) (Template.FullPage rows)
    }

let Endpoints (deps: Dependencies)= [ route "/" (renderApplications deps.TestRead) ]
