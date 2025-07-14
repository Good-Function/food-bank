module Tools.TestServer

open System.Net.Http
open Microsoft.AspNetCore.Mvc.Testing
open Program

let runTestApi () =
    let client = (new WebApplicationFactory<Program>()).Server.CreateClient()
    client.DefaultRequestHeaders.Add("HX-Request", "true")
    client
    
let authenticate (role: string) (client: HttpClient)=
    client.DefaultRequestHeaders.Add("role", role)
    client
    
