module OrganizationsDataApi

open System.Net
open System.Text.Json
open Organizations.Domain.Organization
open Tests
open Xunit
open Tools.TestServer
open FsUnit.Xunit
open Organizations.Database.OrganizationsDao

[<Fact>]
let ``/ogranizations/kontakty?email returns kontakty by email`` () =
    task {
        // Arrange
        let org =  Arranger.AnOrganization()
        do! org |> (save Tools.DbConnection.connectDb)
        let api = runTestApi()
        // Act
        let! response = api.GetAsync $"/api/organizations/kontakty?email={org.Kontakty.Email}"
        // Assert
        let! body = response.Content.ReadAsStringAsync()
        let kontakty = JsonSerializer.Deserialize<Kontakty>(body, JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase))
        response.StatusCode |> should equal HttpStatusCode.OK
        kontakty |> should equal org.Kontakty
    }