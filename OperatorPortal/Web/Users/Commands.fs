module Users.Commands

open System.Threading.Tasks

[<CLIMutable>]
type NewUser = {
    Email: string
}

type AddUser = NewUser -> Task<unit>
