module Tools.TestServer

open System.Net.Http
open Microsoft.AspNetCore.Mvc.Testing
open Program

let runTestApi () =
    let client = (new WebApplicationFactory<Program>()).Server.CreateClient()
    client.DefaultRequestHeaders.Add("HX-Request", "true")
    client
    
let Admin = "Admin"
let Reader = "Reader"
let Editor = "Editor"

type Role = Admin | Reader | Editor
    
let authenticate (role: Role) (client: HttpClient)=
    Authentication.role <- role.ToString()
    client
    
