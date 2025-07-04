module Permissions

open System.Security.Claims

type Permission =
    | EditOrganization
    | ViewOrganization
    | ManageUsers
    
let rolePermissions =
    Map.ofList [
        "Admin", [ EditOrganization; ViewOrganization; ManageUsers]
        "Editor", [ EditOrganization; ViewOrganization; ]
        "Reader", [ ViewOrganization ]
    ]
    
let can (requiredPermission: Permission) (user: ClaimsPrincipal) =
    let role = user.FindFirstValue(ClaimTypes.Role)
    match role with
    | null -> false
    | r ->
        match rolePermissions.TryFind r with
        | Some perms -> perms |> List.contains requiredPermission
        | None -> false