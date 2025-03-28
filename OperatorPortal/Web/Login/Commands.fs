module Login.Commands

open Login.Domain

type PasswordChange =
    {
      UserId: int64
      NewPassword: Password
    }

type ChangePassword = PasswordChange -> Async<unit>