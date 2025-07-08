module Users.Router

open System.IO
open CompositionRoot
open HttpContextExtensions
open Oxpecker
open RenderBasedOnHtmx

let photo (fetchPhoto: Queries.FetchProfilePhoto) (userId: string): EndpointHandler =
    fun ctx -> task {
        match! fetchPhoto userId with
        | Some photo ->
            ctx.SetContentType "image/jpeg"
            do! photo.CopyToAsync(ctx.Response.Body)
        | None ->
            ctx.SetContentType "image/png"
            let path = "wwwroot/img/no-profile-picture.png"
            do! ctx.WriteFileStream(false, path, None, None)
    }

let page: EndpointHandler =
    fun ctx -> task {
        let username = ctx.UserName
        return ctx |> render (Index.Partial username) (Index.FullPage username)
    }
    
let users (listUsers: Queries.ListUsers) (listRoles: Queries.ListRoles): EndpointHandler =
    fun ctx -> task {
        let! users = listUsers()
        let! roles = listRoles()
        return ctx.WriteHtmlView (Templates.UsersTable.View users roles)
    }

let Endpoints (deps: Dependencies)= [
    route "/" page
    route "/users" (users deps.ListUsers deps.ListRoles)
    routef "/{%s}/photo" (photo deps.FetchProfilePhoto)
]
