module Users.Commands

open System.Threading.Tasks

[<CLIMutable>]
type NewUser = {
    Email: string
}

type AddUser = NewUser -> Task<unit>
type DeleteUser = Domain.UserId -> Task<unit>
