module Users.Domain

open System

type UserId = Guid
type RoleId = Guid

type Role =
    { Id: RoleId
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