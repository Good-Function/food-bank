module Login.Router

open System.Net
open System.Security.Claims
open System.Threading.Tasks
open Microsoft.AspNetCore.Authentication.Cookies
open Oxpecker
open Microsoft.AspNetCore.Authentication

let renderLogin: EndpointHandler =
    fun ctx ->
        ctx.WriteHtmlView (ctx.TryGetQueryValue "ReturnUrl" |> Template.LoginTemplate)

let handleLogin: EndpointHandler =
    fun ctx ->
        task {
            let! form = ctx.BindForm<LoginDto.LoginFormDto>()
            let username, password = form.Email, form.Password
            let returnUrl =
                match ctx.TryGetQueryValue "ReturnUrl" with
                | None -> "/organizations"
                | Some url -> url
            if username = "test@test.test" then
                let claims = 
                    [
                        Claim(ClaimTypes.Name, username)
                        Claim("FullName", "Anana Kofana")
                        Claim(ClaimTypes.Role, "Administrator")
                    ]
                let claimsIdentity = ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)       
                do! ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, ClaimsPrincipal(claimsIdentity))
                ctx.SetStatusCode(HttpStatusCode.OK |> int)
                ctx.Response.Headers.Add("HX-Redirect", returnUrl)
                return! Task.CompletedTask
            else
                return! ctx.WriteHtmlView Template.LoginError
        }
    
let Endpoints = [
    GET [
        route "/" renderLogin
    ]
    POST [
        route "/" handleLogin
    ]   
]