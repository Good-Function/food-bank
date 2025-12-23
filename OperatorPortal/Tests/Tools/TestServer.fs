module Tools.TestServer

open System.Net.Http
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.AspNetCore.Hosting
open Program

type TestWebApplicationFactory() =
    inherit WebApplicationFactory<Program>()
    
    override _.ConfigureWebHost(builder: IWebHostBuilder) =
        builder.UseEnvironment("Test") |> ignore
        base.ConfigureWebHost(builder)

// Singleton instance to reuse across all tests and avoid creating too many file watchers
let private factory = lazy (new TestWebApplicationFactory())

let runTestApi () =
    let client = factory.Value.Server.CreateClient()
    client.DefaultRequestHeaders.Add("HX-Request", "true")
    client
    
let authenticate (role: string) (client: HttpClient)=
    client.DefaultRequestHeaders.Add("role", role)
    client
    
