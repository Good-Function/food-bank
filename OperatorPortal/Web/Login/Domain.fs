module Login.Domain

open  System.ComponentModel.DataAnnotations

[<Literal>]
let MIN_PASSWORD_LENGTH = 6

type PasswordChangeErrors =
    | ConfirmationIsIncorrect
    | OldPasswordIsIncorrect
    | NewPasswordIsTooWeak
    member this.ToMessage() =
        match this with
        | ConfirmationIsIncorrect -> "Hasło nie zgadza się."
        | OldPasswordIsIncorrect -> "Stare hasło jest niepoprawne."
        | NewPasswordIsTooWeak -> "Hasło musi mieć przynajmniej 6 znaków."

type EmailError = | InvalidEmail
type Email = private Email of string
type Password = private Password of string

module Password =
    let create (text: string) = Password text
    let value (Password password) = password

module Email =
    let create (text: string) : Result<Email, EmailError> =
        let isValid = EmailAddressAttribute().IsValid text
        if isValid then
            Ok(Email text)
        else
            Error EmailError.InvalidEmail
            
    let value (Email email) = email

type User =
    { Id: int64
      Email: Email
      Password: Password }
