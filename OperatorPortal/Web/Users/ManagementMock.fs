module Users.ManagementMock

open System
open System.Threading.Tasks
open Users.Domain

let editor: Role =
    { Id = Guid.NewGuid()
      Name = "Editor"
      Description = "" }

let admin: Role =
    { Id = Guid.NewGuid()
      Name = "Admin"
      Description = "" }

let reader: Role =
    { Id = Guid.NewGuid()
      Name = "Reader"
      Description = "" }

let roles: Role list = [ reader; editor; admin ]

let mutable users: User list =
    [ { Id = Guid.NewGuid()
        Mail = "admin@bzsoswaw.pl"
        RoleId = admin.Id } ]

let assignRoleToUser (userId: UserId) (appRoleId: Guid) =
    task {
        users <-
            users
            |> List.map (fun user ->
                if user.Id = userId then
                    { user with RoleId = appRoleId }
                else
                    user)
    }

let removeUser (userId: Guid) =
    task { users <- users |> List.filter (fun user -> user.Id <> userId) }


let inviteUser: Commands.AddUser =
    fun newUser -> 
        task {
            let newUser =
                { Id = UserId(Guid.NewGuid().ToString())
                  Mail = newUser.Email
                  RoleId = reader.Id }

            users <- newUser :: users
        }

let fetchPhoto: Queries.FetchProfilePhoto = fun _ -> task { return None }

let fetchAppUsers: Queries.ListUsers = fun () -> task { return users }

let fetchRoles: Queries.ListRoles = fun () -> task { return roles }
