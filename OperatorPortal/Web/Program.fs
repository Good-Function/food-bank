module Program

open System
open Microsoft.AspNetCore.Authentication.Cookies
open PostgresPersistence.DapperFsharp
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Oxpecker
open Settings

let protect =  configureEndpoint _.RequireAuthorization()

let endpoints (orgDeps: Organizations.CompositionRoot.Dependencies) =
    [
        GET [
            route "/" <| redirectTo "/organizations" false
        ]
        subRoute "/login" Login.Router.Endpoints
        subRoute "/organizations" (Organizations.Router.Endpoints orgDeps) |> protect
        subRoute "/applications" Applications.Router.Endpoints |> protect
    ]

let notFoundHandler (ctx: HttpContext) =
    ctx.SetStatusCode 404
    ctx.WriteHtmlView (Layout.Head.Template Layout.NotFound.Template "Not Found")

let createServer () =
    let builder = WebApplication.CreateBuilder()

    let settings =
      ConfigurationBuilder()
          .SetBasePath(AppContext.BaseDirectory)
          .AddIniFile("settings.ini", false)
          .AddEnvironmentVariables()
          .Build()
          .Get<Settings>()
    let dbConnect = connectDB(settings.DbConnectionString)
    let orgDeps: Organizations.CompositionRoot.Dependencies = {
        ReadOrganizationSummaries = Organizations.Database.OrganizationsDao.readSummaries dbConnect
    }
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
        .UseOxpecker(endpoints orgDeps)
        .Run(notFoundHandler)
    app
   
createServer().Run()

type Program() = class end