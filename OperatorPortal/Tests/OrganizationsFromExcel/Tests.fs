module OrganizationsFromExcel.Tests

open Organizations.Database.csvLoader
open Xunit
open FsUnit.Xunit

[<Literal>]
let ResolutionFolder = __SOURCE_DIRECTORY__

let CsvProviderTests = Orgs.GetSample()

[<Fact>]
let ``Sample excel can be parsed to organizations and saved`` () =
    async {
        // Arrange
        let row = CsvProviderTests.Rows |> Seq.head
        // Act
        let parsedRow = parse row
        parsedRow.Kontakty.Email |> should contain '@'
    }
