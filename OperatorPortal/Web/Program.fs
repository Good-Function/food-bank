module Program

open System
open Microsoft.AspNetCore.Authentication.Cookies
open PostgresPersistence.DapperFsharp
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Organizations.Database
open Oxpecker
open Settings

let protect =  configureEndpoint _.RequireAuthorization()

let endpoints 
    (orgDeps: Organizations.CompositionRoot.Dependencies)
    (appDeps: Applications.CompositionRoot.Dependencies) =
    [
        GET [
            route "/" <| redirectTo "/organizations" false
        ]
        subRoute "/login" Login.Router.Endpoints
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
    let orgDeps: Organizations.CompositionRoot.Dependencies = {
        ReadOrganizationSummaries = OrganizationsDao.readSummaries dbConnect
        ReadOrganizationDetailsBy = OrganizationsDao.readBy dbConnect
        ChangeDaneAdresowe = OrganizationsDao.changeDaneAdresowe dbConnect
        ChangeKontakty = OrganizationsDao.changeKontakty dbConnect
        ChangeBeneficjenci = OrganizationsDao.changeBeneficjenci dbConnect
        ChangeDokumenty = OrganizationsDao.changeDokumenty dbConnect
    }
    let appDeps: Applications.CompositionRoot.Dependencies = {
        TestRead = Applications.Database.readSchemas dbConnect
    }
    builder.Services
        .AddRouting()
        .AddOxpecker()
        .AddAuthorization()
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(Authentication.configureAuthenticationCookie) |> ignore
    let app = builder.Build()
    Migrations.main [|settings.DbConnectionString; AppDomain.CurrentDomain.BaseDirectory|] |> ignore
    if app.Environment.EnvironmentName <> "Production" then app.Use(Authentication.fakeAuthenticate) |> ignore
    app
        .UseRouting()
        .UseStaticFiles()
        .UseAuthentication()
        .UseAuthorization()
        .UseOxpecker(endpoints orgDeps appDeps)
        .Run(notFoundHandler)
    app
   
createServer().Run()

type Program() = class end
