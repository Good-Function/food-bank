module Login.Router

open System.Threading

open HttpContextExtensions
open Login
open Login.CompositionRoot
open Login.Domain
open Oxpecker
open RenderBasedOnHtmx
open Login.SignInHandler
open Microsoft.AspNetCore.Builder

let protect =  configureEndpoint _.RequireAuthorization()

let renderLogin: EndpointHandler =
    fun ctx -> ctx.WriteHtmlView(ctx.TryGetQueryValue "ReturnUrl" |> Template.LoginTemplate)

let handleLogin (readUser: Email -> Async<User option>) : EndpointHandler =
    fun ctx ->
        task {
            let! form = ctx.BindForm<Dtos.LoginFormDto>()
            let email, password = form.Email, form.Password
            let! user = getUser readUser email
            match user with
            | VerifiedUser password user -> return! user |> signIn ctx
            | _ -> return! ctx.WriteHtmlView Template.LoginError
        }
        
let passwordChange: EndpointHandler =
    fun ctx -> task {
        return ctx |> render (ChangePasswordTemplate.Partial ctx.UserName) (ChangePasswordTemplate.FullPage ctx.UserName)
    }
    
let changePassword (readUser: int64 -> Async<User>) (changePassword: Commands.ChangePassword) : EndpointHandler =
    fun ctx ->
        task {
            let! userInputs = ctx.BindForm<Dtos.PasswordChangeDto>()
            let! user = readUser ctx.UserId
            let errors =
                [
                    if not (userInputs.OldPassword |> matchesPassword user.Password)
                        then Some OldPasswordIsIncorrect
                    if userInputs.NewPassword <> userInputs.NewPasswordConfirmation
                        then Some ConfirmationIsIncorrect
                    if userInputs.NewPassword.Length < MIN_PASSWORD_LENGTH
                        then Some NewPasswordIsTooWeak
                ] |> List.choose id
            if errors = [] then
                let hashedPassword =  userInputs.NewPassword |> computeHash |>  Password.create
                do! changePassword { NewPassword = hashedPassword ; UserId = user.Id }
                return ctx.WriteHtmlView ChangePasswordTemplate.Success
            else 
                return ctx.WriteHtmlView (ChangePasswordTemplate.Form errors)
        }
        
let Endpoints (deps: Dependencies) =
    [
      GET [
        route "/" renderLogin
        route "/password-change" passwordChange |> protect
      ]
      POST [
        route "/" (handleLogin deps.TryFindUserBy)
        route "/password-change" (changePassword deps.ReadUserBy deps.ChangePassword) |> protect
      ]
    ]
