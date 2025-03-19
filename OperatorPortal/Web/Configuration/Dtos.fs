module Configuration.Dtos

[<CLIMutable>]
type PasswordChangeDto = {
    OldPassword: string
    NewPassword: string
    NewPasswordConfirmation: string
}