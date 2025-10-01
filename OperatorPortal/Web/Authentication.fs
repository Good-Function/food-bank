module Authentication

open System
open System.Security.Claims
open System.Threading.Tasks
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication.OpenIdConnect
open Microsoft.AspNetCore.Http
open Oxpecker

let fakeAuthenticate =
    Func<HttpContext, RequestDelegate, Task>(fun ctx next ->
        let role = ctx.TryGetHeaderValue("role") |> Option.defaultValue "Editor"
        task {
            let claims = [
                Claim("preferred_username", "developer@bzsos.pl")
                Claim(ClaimTypes.NameIdentifier, "0")
                Claim(ClaimTypes.Role, role)
            ]
            let identity = ClaimsIdentity(claims, OpenIdConnectDefaults.AuthenticationScheme)
            let principal = ClaimsPrincipal(identity)
            ctx.User <- principal
            return! next.Invoke(ctx)
        })

let configureAuthenticationCookie =
    fun (options: CookieAuthenticationOptions) ->
        options.ExpireTimeSpan <- TimeSpan.FromMinutes(20.0)
        options.SlidingExpiration <- true
        options.AccessDeniedPath <- PathString("/Forbidden/")
        options.Cookie.Name <- "authorization-cookie"
        options.LoginPath <- "/login"

        options.Events <-
            CookieAuthenticationEvents(
                OnRedirectToLogin =
                    (fun ctx ->
                        if ctx.Request.Headers.ContainsKey "HX-Request" then
                            ctx.Response.Headers.Add("HX-Redirect", ctx.RedirectUri)
                        else
                            ctx.Response.Redirect(ctx.RedirectUri, false)
                        Task.CompletedTask),
                OnRedirectToAccessDenied =
                    (fun ctx ->
                        ctx.Response.StatusCode <- StatusCodes.Status403Forbidden
                        Task.CompletedTask)
            )
