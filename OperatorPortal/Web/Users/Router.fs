module Users.Router

open CompositionRoot
open HttpContextExtensions
open Oxpecker
open RenderBasedOnHtmx

let renderApplications: EndpointHandler =
    fun ctx -> task {
        let username = ctx.UserName
        return ctx |> render (Template.Partial username) (Template.FullPage username)
    }

let Endpoints (deps: Dependencies)= [ route "/" (renderApplications) ]
