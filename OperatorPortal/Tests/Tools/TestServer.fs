module Tools.TestServer

open System.Net.Http
open Microsoft.AspNetCore.Mvc.Testing
open Program

let runTestApi () =
    (new WebApplicationFactory<Program>()).Server.CreateClient()
    
let authenticate (user: string) (client: HttpClient)=
    client.DefaultRequestHeaders.Add(Authentication.FakeAuthenticationHeader, user)
    client
    
