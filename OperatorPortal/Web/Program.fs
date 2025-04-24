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

let endpoints 
    (orgDeps: Organizations.CompositionRoot.Dependencies)
    (loginDeps: Login.CompositionRoot.Dependencies)
    (appDeps: Applications.CompositionRoot.Dependencies)=
    [
        GET [
            route "/" <| redirectTo "/organizations" false
        ]
        subRoute "/login" (Login.Router.Endpoints loginDeps) 
        subRoute "/organizations" (Organizations.Router.Endpoints orgDeps) |> protect
        subRoute "/applications" (Applications.Router.Endpoints appDeps) |> protect
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
    builder.Services
        .AddRouting()
        .AddOxpecker()
        .AddAuthorization()
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(Authentication.configureAuthenticationCookie) |> ignore
    let app = builder.Build()
    if app.Environment.EnvironmentName <> "Production"
        then app.Use(Authentication.fakeAuthenticate) |> ignore
    else
        Migrations.main [|settings.DbConnectionString; AppDomain.CurrentDomain.BaseDirectory|] |> ignore
    app.Use(Culture.middleware) |> ignore
    app
        .UseRouting()
        .UseStaticFiles()
        .UseAuthentication()
        .UseAuthorization()
        .UseOxpecker(endpoints
                        (Organizations.CompositionRoot.build dbConnect)
                        (Login.CompositionRoot.build dbConnect)
                        (Applications.CompositionRoot.build dbConnect)
                    )
        .Run(notFoundHandler)
    app
   
createServer().Run()

type Program() = class end
