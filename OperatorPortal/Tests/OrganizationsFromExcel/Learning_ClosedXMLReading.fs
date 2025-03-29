module OrganizationsFromExcel.Learning_ClosedXMLReading

open System.IO
open ClosedXML.Excel
open Xunit
open FsUnit.Xunit

[<Fact>]
let ``Pasrsing excel file`` () =
    task {
        // Arrange
        use stream = File.OpenRead(Path.Combine(__SOURCE_DIRECTORY__, "bank.xlsx"))       
        use workbook = new XLWorkbook(stream)
        let sheet = workbook.Worksheet(1) // Read the first worksheet
        let cellValue = sheet.Cell(1, 1).GetValue<string>() // Read A1 cell
        cellValue |> should equal "teczka"
    }