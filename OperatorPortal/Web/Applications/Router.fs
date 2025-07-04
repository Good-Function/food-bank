module Applications.Router

open CompositionRoot
open HttpContextExtensions
open Oxpecker
open RenderBasedOnHtmx

let renderApplications (testRead: Async<string list>): EndpointHandler =
    fun ctx -> task {
        let username = ctx.UserName
        let! rows = testRead
        return ctx |> render (Template.Partial rows username) (Template.FullPage rows username)
    }

let Endpoints (deps: Dependencies)= [ route "/" (renderApplications deps.TestRead) ]
