module Permissions

open System.Security.Claims
open Microsoft.AspNetCore.Http
open Oxpecker

type Permission =
    | EditOrganization
    | ViewOrganization
    | ManageUsers
    
let permissionsMap =
    Map.ofList [
        "Admin", [ EditOrganization; ViewOrganization; ManageUsers]
        "Editor", [ EditOrganization; ViewOrganization; ]
        "Reader", [ ViewOrganization ]
    ]
    
let can (requiredPermission: Permission) (user: ClaimsPrincipal) =
    let role = user.FindFirstValue(ClaimTypes.Role)
    match permissionsMap.TryFind role with
        | Some perms -> perms |> List.contains requiredPermission
        | None -> false
    
let authorize permission : EndpointMiddleware =
     fun (next: EndpointHandler) (ctx: HttpContext) ->
        if  ctx.User |> can permission then
            next ctx
        else
            ctx.SetStatusCode 403
            ctx.WriteText "Brak dostępu"