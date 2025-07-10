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
    
let hasPermissionTo (requiredPermission: Permission) (role: string) =
    match rolePermissions.TryFind role with
        | Some perms -> perms |> List.contains requiredPermission
        | None -> false
    
let can (requiredPermission: Permission) (user: ClaimsPrincipal) =
    let role = user.FindFirstValue(ClaimTypes.Role)
    hasPermissionTo requiredPermission role