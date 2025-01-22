module KestrelTestServer

open Microsoft.AspNetCore.Mvc.Testing
open System
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Hosting.Server
open Microsoft.AspNetCore.Hosting.Server.Features
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Program

type KestrelApplicationFactory(?port: int) =
    inherit WebApplicationFactory<Program>()

    let mutable host: IHost option = None
    let port = defaultArg port 5001

    member this.ServerAddress =
        this.EnsureServer()
        this.ClientOptions.BaseAddress.ToString()

    override this.CreateHost(builder: IHostBuilder) =
        let testHost = builder.Build()
        builder.ConfigureWebHost(fun webHostBuilder ->
            webHostBuilder
                .UseKestrel()
                .ConfigureKestrel(fun configure -> configure.ListenLocalhost port) |> ignore)
        |> ignore

        host <- Some(builder.Build())
        host.Value.Start()
        let server = host.Value.Services.GetRequiredService<IServer>()
        let addresses = server.Features.Get<IServerAddressesFeature>()
        this.ClientOptions.BaseAddress <- addresses.Addresses |> Seq.map Uri |> Seq.last
        testHost.Start()
        testHost

    override this.Dispose _ =
        match host with
        | Some h -> h.Dispose()
        | None -> ()

    member private this.EnsureServer() =
        match host with
        | Some _ -> ()
        | None -> this.CreateDefaultClient() |> ignore