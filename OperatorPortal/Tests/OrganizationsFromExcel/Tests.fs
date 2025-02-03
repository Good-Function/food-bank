module Tests.OrganizationsFromExcel.Tests

open PostgresPersistence
open Xunit
open FsUnit.Xunit
open Organizations.Database.csvLoader
open DapperFsharp
open Organizations.Database.OrganizationRow

[<Literal>]
let ResolutionFolder = __SOURCE_DIRECTORY__

let sampleOrgs = Orgs.GetSample()

[<Fact>]
let ``Sample excel can be parsed to organizations and saved`` () =
    async {
        let row = sampleOrgs.Rows |> Seq.head
        let a = parse row
        
        let con = dbConnect("Host=localhost;Port=5432;User Id=postgres;Password=Strong!Passw0rd;Database=food_bank;")
        
        let! db = con()
        
        let! rows = db |> dbQuery<OrganizationRow>("SELECT * FROM organizacje")
        
        true |> should equal false
    }
