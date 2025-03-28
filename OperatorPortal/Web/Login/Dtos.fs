module Login.Dtos

[<CLIMutable>]
type PasswordChangeDto = {
    OldPassword: string
    NewPassword: string
    NewPasswordConfirmation: string
}

[<CLIMutable>]
type LoginFormDto = { Email: string; Password: string }