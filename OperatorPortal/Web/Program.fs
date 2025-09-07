module Program

open System
open Azure.Storage.Blobs
open Layout
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Authentication.OpenIdConnect
open Microsoft.AspNetCore.HttpOverrides
open Microsoft.Identity.Web
open Microsoft.IdentityModel.Tokens
open PostgresPersistence.DapperFsharp
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Oxpecker
open Settings

let protect =  configureEndpoint _.RequireAuthorization()

let testApiAuth: EndpointHandler =
    fun ctx -> task {
        let auth = ctx.TryGetHeaderValue "Authorization"
        return! ctx.WriteJson {| response = "pong"; auth = auth |}
    }

let endpoints 
    (orgDeps: Organizations.CompositionRoot.Dependencies)
    (loginDeps: Login.CompositionRoot.Dependencies)
    (appDeps: Applications.CompositionRoot.Dependencies)
    (usersDeps: Users.CompositionRoot.Dependencies)
    =
    [
        GET [
            route "/api/pingp" testApiAuth |> protect
            route "/api/ping" testApiAuth
            route "/" <| redirectTo "/organizations" false
        ]
        subRoute "/login" (Login.Router.Endpoints loginDeps)
        subRoute "/fragments" FragmentsRouter.Endpoints
        subRoute "/organizations" (Organizations.Router.Endpoints orgDeps) |> protect
        subRoute "/applications" (Applications.Router.Endpoints appDeps) |> protect
        subRoute "/team" (Users.Router.Endpoints usersDeps) |> protect
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
          .AddIniFile($"settings.{builder.Environment.EnvironmentName}.ini", true)
          .AddEnvironmentVariables()
          .Build()
          .Get<Settings>()
    let dbConnect = connectDB(settings.DbConnectionString)
    let blobServiceClient = (BlobServiceClient settings.BlobStorageConnectionString)
    builder.Services
        .AddRouting()
        .AddOxpecker()
        .AddAuthorization()
        .AddAuthentication(fun options ->
            options.DefaultScheme <- OpenIdConnectDefaults.AuthenticationScheme
            options.DefaultChallengeScheme <- OpenIdConnectDefaults.AuthenticationScheme)
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, fun options ->
            options.Authority <- $"{settings.AzureAd.Instance}{settings.AzureAd.TenantId}/v2.0"
            options.Audience <- settings.AzureAd.ClientId 
            options.TokenValidationParameters <- TokenValidationParameters(
                ValidateIssuer = true
            )
        )
        .AddMicrosoftIdentityWebApp(fun options ->
            options.Instance <- settings.AzureAd.Instance
            options.TenantId <- settings.AzureAd.TenantId
            options.ClientId <- settings.AzureAd.ClientId
            options.ClientSecret <- settings.AzureAd.ClientSecret 
            options.CallbackPath <- settings.AzureAd.CallbackPath
            options.SaveTokens <- true
            options.ResponseType <- "code" 
            options.Scope.Add("https://graph.microsoft.com/User.ReadBasic.All");
    ) |> ignore
    builder.Services.Configure<ForwardedHeadersOptions>(fun (options: ForwardedHeadersOptions) ->
        options.ForwardedHeaders <- ForwardedHeaders.XForwardedProto ||| ForwardedHeaders.XForwardedHost
        options.KnownNetworks.Clear()
        options.KnownProxies.Clear()
    ) |> ignore
    let app = builder.Build()
    if app.Environment.EnvironmentName <> "Production"
        then app.Use Authentication.fakeAuthenticate |> ignore
    else
        Migrations.main [|settings.DbConnectionString; AppDomain.CurrentDomain.BaseDirectory|] |> ignore
    app.Use Culture.middleware |> ignore
    app
        .UseForwardedHeaders()
        .UseRouting()
        .UseStaticFiles()
        .UseAuthentication()
        .UseAuthorization()
        .UseOxpecker(endpoints
                        (Organizations.CompositionRoot.build(dbConnect, blobServiceClient))
                        (Login.CompositionRoot.build dbConnect)
                        (Applications.CompositionRoot.build dbConnect)
                        (Users.CompositionRoot.build settings.AzureAd settings.Users)
                    )
        .Run notFoundHandler
    app
   
createServer().Run()

type Program() = class end