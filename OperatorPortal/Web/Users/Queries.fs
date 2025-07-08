module Users.Queries

open System.IO
open System.Threading.Tasks
open Domain

type ListUsers = unit -> Task<User list>
type ListRoles = unit -> Task<Role list>
type FetchProfilePhoto = string -> Task<Stream option>