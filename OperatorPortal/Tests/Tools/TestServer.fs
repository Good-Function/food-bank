module Tools.TestServer

open Microsoft.AspNetCore.Mvc.Testing
open Program

let runTestApi () =
    (new WebApplicationFactory<Program>()).Server
    
