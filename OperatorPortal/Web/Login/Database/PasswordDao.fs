module Login.Database.PasswordDao

open System.Data
open Login
open Login.Domain
open PostgresPersistence.DapperFsharp

let changePassword (connectDb: unit -> Async<IDbConnection>) (passwordChange: Commands.PasswordChange) =
    let passwordString = passwordChange.NewPassword |> Password.value
    async {
        use! db = connectDb ()
        do! db.Execute """
UPDATE users
SET password = @NewPassword
WHERE id = @UserId""" {|UserId = passwordChange.UserId; NewPassword = passwordString|}
}
