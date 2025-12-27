module Tools.TestServer

open System.Collections.Generic
open System.Net.Http
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.AspNetCore.Hosting
open Program
open FSharp.Data

type TestWebApplicationFactory() =
    inherit WebApplicationFactory<Program>()
    
    override _.ConfigureWebHost(builder: IWebHostBuilder) =
        builder.UseEnvironment("Test") |> ignore
        base.ConfigureWebHost(builder)

// Singleton instance to reuse across all tests and avoid creating too many file watchers
let private factory = lazy (new TestWebApplicationFactory())

let runTestApi () =
    // WebApplicationFactory.CreateClient() automatically configures HttpClient to handle cookies
    let client = factory.Value.Server.CreateClient()
    client.DefaultRequestHeaders.Add("HX-Request", "true")
    client
    
let authenticate (role: string) (client: HttpClient)=
    client.DefaultRequestHeaders.Add("role", role)
    client

let private addCookiesToClient (client: HttpClient) (response: HttpResponseMessage) =
    if response.Headers.Contains("Set-Cookie") then
        let cookieValues = 
            response.Headers.GetValues("Set-Cookie")
            |> Seq.map (fun c -> c.Split(';').[0].Trim())
            |> Seq.filter (fun c -> c.Contains("="))
            |> String.concat "; "
        if cookieValues <> "" then
            if client.DefaultRequestHeaders.Contains("Cookie") then
                let existing = client.DefaultRequestHeaders.GetValues("Cookie") |> Seq.head
                client.DefaultRequestHeaders.Remove("Cookie") |> ignore
                client.DefaultRequestHeaders.Add("Cookie", $"{existing}; {cookieValues}")
            else
                client.DefaultRequestHeaders.Add("Cookie", cookieValues)


let getAntiforgeryToken (client: HttpClient) (formUrl: string) : Task<string> = task {
    let! response = client.GetAsync(formUrl)
    response.EnsureSuccessStatusCode() |> ignore
    let! html = response.Content.ReadAsStringAsync()
    addCookiesToClient client response
    
    let doc = HtmlDocument.Parse html
    let tokenInput = doc.CssSelect "input[name='__RequestVerificationToken']" |> List.tryHead
    return tokenInput |> Option.map (fun input -> input.AttributeValue("value")) |> Option.defaultValue ""
}

let formDataWithToken (client: HttpClient) (formUrl: string) (fields: (string * string) list) = task {
    let! token = getAntiforgeryToken client formUrl
    let allFields = ("__RequestVerificationToken", token) :: fields
    return 
        allFields
        |> List.map (fun (key, value) -> KeyValuePair<string, string>(key, value))
        |> fun pairs -> new FormUrlEncodedContent(pairs)
}

let putFormWithToken (client: HttpClient) (editUrl: string) (submitUrl: string) (fields: (string * string) list) = task {
    let! data = formDataWithToken client editUrl fields
    return! client.PutAsync(submitUrl, data)
}

let postFormWithToken (client: HttpClient) (formUrl: string) (submitUrl: string) (fields: (string * string) list) = task {
    let! data = formDataWithToken client formUrl fields
    return! client.PostAsync(submitUrl, data)
}
    
