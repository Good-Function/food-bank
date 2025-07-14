module Users.Router

open System
open CompositionRoot
open HttpContextExtensions
open Microsoft.AspNetCore.Http
open Oxpecker
open RenderBasedOnHtmx
open Users
open Permissions

let photo (fetchPhoto: Queries.FetchProfilePhoto) (userId: string) : EndpointHandler =
    fun ctx ->
        task {
            match! fetchPhoto userId with
            | Some photo ->
                ctx.SetContentType "image/jpeg"
                do! photo.CopyToAsync(ctx.Response.Body)
            | None ->
                ctx.SetContentType "image/png"
                let path = "wwwroot/img/no-profile-picture.png"
                do! ctx.WriteFileStream(false, path, None, None)
        }

let deleteUser (deleteUser: Commands.DeleteUser) (userId: string) : EndpointHandler =
    fun ctx ->
        task {
            let userId: Domain.UserId = Guid(userId)
            do! deleteUser userId
            ctx.SetStatusCode(StatusCodes.Status200OK)
        }

let page: EndpointHandler =
    fun ctx ->
        task {
            let username = ctx.UserName
            let permissions = rolePermissions[ctx.UserRole]

            return
                ctx
                |> render (Index.Partial username permissions) (Index.FullPage username permissions)
        }

let users (listUsers: Queries.ListUsers) (listRoles: Queries.ListRoles) : EndpointHandler =
    fun ctx ->
        task {
            let! users = listUsers ()
            let! roles = listRoles ()
            let permissions = rolePermissions[ctx.UserRole]
            return ctx.WriteHtmlView(Templates.UsersTable.View users roles permissions)
        }

let addUser
    (addUser: Commands.AddUser)
    (listUsers: Queries.ListUsers)
    (listRoles: Queries.ListRoles)
    : EndpointHandler =
    fun ctx ->
        task {
            let! newUser = ctx.BindForm<Commands.NewUser>()
            do! addUser newUser
            let! users = listUsers ()
            let! roles = listRoles ()
            let permissions = rolePermissions[ctx.UserRole]
            ctx.SetStatusCode(StatusCodes.Status201Created)
            return ctx.WriteHtmlView(Templates.UsersTable.View users roles permissions)
        }

let assignRole
    (assignRole: Commands.AssignRole)
    (listUsers: Queries.ListUsers)
    (listRoles: Queries.ListRoles)
    (userId: string)
    : EndpointHandler =
    fun ctx ->
        task {
            let permissions = rolePermissions[ctx.UserRole]
            let roleId = ctx.TryGetFormValue("RoleId") |> Option.defaultValue ""
            let userId: Domain.UserId = Guid(userId)
            let roleId: Domain.RoleId = Guid(roleId)
            do! assignRole userId roleId
            let! users = listUsers ()
            let! roles = listRoles ()
            ctx.SetStatusCode(StatusCodes.Status200OK)
            return ctx.WriteHtmlView(Templates.UsersTable.View users roles permissions)
        }

let Endpoints (deps: Dependencies) =
    [ GET
          [ route "/" page
            route "/users" (users deps.ListUsers deps.ListRoles)
            routef "/{%s}/photo" (photo deps.FetchProfilePhoto) ]
      POST
          [ route
                "/users"
                (authorize Permission.ManageUsers
                 >=> (addUser deps.AddUser deps.ListUsers deps.ListRoles)) ]
      DELETE [ routef "/users/{%s}" (authorize Permission.ManageUsers >>=> (deleteUser deps.DeleteUser)) ]
      PUT
          [ routef
                "/users/{%s}/roles"
                (authorize Permission.ManageUsers
                 >>=> (assignRole deps.AssignRole deps.ListUsers deps.ListRoles)) ] ]
