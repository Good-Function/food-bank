module Login.SignInHandler

open System.Net
open System.Security.Claims
open Login.Domain
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http
open Oxpecker
open BCrypt.Net

type ErrorDuringLogin = EmailError | UserNotFound

let getUser (readUser: Email -> Async<User option>) (userEmail: string)=
    asyncResult {
        let! userEmail = Email.create userEmail
                         |> Result.mapError(fun _ -> ErrorDuringLogin.EmailError)
        return! readUser userEmail
                |> AsyncResult.requireSome ErrorDuringLogin.UserNotFound
    }
    
let computeHash = BCrypt.HashPassword
    
let matchesPassword hashedPassword passwordToVerify: bool =
    BCrypt.Verify(passwordToVerify, hashedPassword |> Password.value)
    
let signIn (ctx: HttpContext) (user: User)  =
    task {
        let returnUrl =
            match ctx.TryGetQueryValue "ReturnUrl" with
            | None -> "/organizations"
            | Some url -> url
        let claims = 
            [
                Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                Claim(ClaimTypes.Name, user.Email |> Email.value)
                Claim(ClaimTypes.Role, "Administrator")
            ]
        let claimsIdentity = ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)       
        do! ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, ClaimsPrincipal(claimsIdentity))
        ctx.SetStatusCode(HttpStatusCode.OK |> int)
        ctx.Response.Headers.Add("HX-Redirect", returnUrl)
    }
    
let (|VerifiedUser|_|) (password: string) (user: Result<User, ErrorDuringLogin>)=
    match user with
    | Ok user when (password |> matchesPassword user.Password) -> Some user
    | _ -> None