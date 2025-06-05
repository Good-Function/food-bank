module Tools.TestServer

open System.Net.Http
open Microsoft.AspNetCore.Mvc.Testing
open Program

let runTestApi () =
    (new WebApplicationFactory<Program>()).Server.CreateClient()
    
let authenticate (client: HttpClient)=
    client.DefaultRequestHeaders.Add("HX-Request", "true")
    client
    
