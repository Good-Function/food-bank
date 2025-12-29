module Login.Database.LoginDao

open System.Data
open Login.Domain
open PostgresPersistence.DapperFsharp

type UserRow =
    { Id: int64
      Email: string
      Password: string }

let createEmailOrFail email : Email =
    match Email.create email with
    | Ok email -> email
    | Error _ -> failwithf "Invalid email found in database: %s" email

let tryFindUserBy (connectDb: unit -> Async<IDbConnection>) (userEmail: Email) : Async<User option> =
    let emailString = userEmail |> Email.value
    async {
        use! db = connectDb ()
        let! userRow =
            db.trySingle<UserRow>
                """
SELECT id, email, password FROM users
WHERE email = @email"""
                {| email = emailString |}
        return
            match userRow with
            | Some userRow ->
                Some
                    { Id = userRow.Id
                      Password = userRow.Password |> Password.create
                      Email = userRow.Email |> createEmailOrFail }
            | _ -> None
    }

let readUserBy (connectDb: unit -> Async<IDbConnection>) (id: int64) : Async<User> =
    async {
        use! db = connectDb ()
        let! userRow =
            db.Single<UserRow>
                """
SELECT id, email, password FROM users
WHERE id = @id"""
                {| id = id |}
        return
            { Id = userRow.Id
              Password = userRow.Password |> Password.create
              Email = userRow.Email |> createEmailOrFail }
    }
