module Program

open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.FileProviders
open Oxpecker

let protect =  configureEndpoint _.RequireAuthorization() 

let endpoints =
    [
        GET [
            route "/" <| redirectTo "/organizations" false
        ]
        subRoute "/login" Login.Router.Endpoints
        subRoute "/organizations" Organizations.Router.Endpoints |> protect
        subRoute "/applications" Applications.Router.Endpoints |> protect
    ]

let notFoundHandler (ctx: HttpContext) =
    ctx.SetStatusCode 404
    ctx.WriteHtmlView Layout.NotFound.Template 

let createServer () =
    let builder = WebApplication.CreateBuilder()
    builder.Services
        .AddRouting()
        .AddOxpecker()
        .AddAuthorization()
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(Authentication.configureAuthenticationCookie) |> ignore
    let app = builder.Build()
    if app.Environment.EnvironmentName <> "Production" then app.Use(Authentication.fakeAuthenticate) |> ignore
    app
        .UseRouting()
        .UseStaticFiles()
        .UseAuthentication()
        .UseAuthorization()
        .UseOxpecker(endpoints)
        .Run(notFoundHandler)
    app
   
createServer().Run()

type Program() = class end