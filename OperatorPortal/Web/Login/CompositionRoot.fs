module Login.CompositionRoot

open System.Data
open Login.Commands
open Login.Domain
open Login

type Dependencies =
    { TryFindUserBy: Email -> Async<User option>
      ReadUserBy: int64 -> Async<User>
      ChangePassword: ChangePassword }

let build (connectDb: unit -> Async<IDbConnection>) : Dependencies =
    { TryFindUserBy = Database.LoginDao.tryFindUserBy connectDb
      ReadUserBy = Database.LoginDao.readUserBy connectDb
      ChangePassword = Database.PasswordDao.changePassword connectDb }
