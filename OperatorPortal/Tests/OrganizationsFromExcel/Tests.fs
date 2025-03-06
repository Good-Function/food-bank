module Tests.OrganizationsFromExcel.Tests

open Organizations.Application.ReadModels
open Xunit
open Organizations.Database.csvLoader
open FsUnit.Xunit

[<Literal>]
let ResolutionFolder = __SOURCE_DIRECTORY__

let sampleOrgs = Orgs.GetSample()

[<Fact>]
let ``Sample excel can be parsed to organizations and saved`` () =
    async {
        // Arrange
        let! db = Tools.DbConnection.connectDb()
        let row = sampleOrgs.Rows |> Seq.head
        // Act
        let parsedRow = parse row
        parsedRow.Email |> should contain '@'
    }
