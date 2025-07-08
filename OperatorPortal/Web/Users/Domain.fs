module Users.Domain

open System

type UserId = Guid

type Role =
    { Id: Guid
      Name: string
      Description: string }

type User =
    { Id: UserId
      Mail: string
      RoleId: Guid }
    
type Invitation =
    {
        Id: UserId
        Mail: string
    }