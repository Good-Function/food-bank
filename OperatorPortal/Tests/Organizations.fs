module Organizations

open System
open System.Net
open Tools.HttResponseMessageToHtml
open Xunit
open Tools.TestServer
open FsUnit.Xunit
open Organizations.Database.OrganizationsDao
open FSharp.Data

// [<Fact>]
// let ``/ogranizations displays organization's name `` () =
//     task {
//         // Arrange
//         let! dbSummaries = readSummaries Tools.DbConnection.connectDb
//         let dbSummaryTeczkaIds = dbSummaries |> List.map(fun summary -> $"%i{summary.Teczka}")
//         let api = runTestApi().CreateClient()
//         api.DefaultRequestHeaders.Add(Authentication.FakeAuthenticationHeader, "TestUser")
//         // Act
//         let! response = api.GetAsync "/organizations/list"
//         let! x = response.Content.ReadAsStringAsync()
//         printfn "%s" x
//         // Assert
//         let! doc = response.HtmlContent()
//         let summaries =
//             doc.CssSelect "tbody tr"
//             |> Seq.map(fun row -> row.Descendants "td" |> Seq.map(_.InnerText()))
//             |> Seq.filter(fun row -> dbSummaryTeczkaIds |> List.contains (row |> Seq.head))
//             |> Seq.toList
//         response.StatusCode |> should equal HttpStatusCode.OK
//         (dbSummaries, summaries) ||> List.iter2(fun dbSummary summary ->
//             $"{dbSummary.Teczka}" |> should equal (summary |> Seq.item 0)
//             dbSummary.NazwaPlacowkiTrafiaZywnosc |> should equal (summary |> Seq.item 1)
//             dbSummary.AdresPlacowkiTrafiaZywnosc |> should equal (summary |> Seq.item 2)
//             dbSummary.GminaDzielnica |> should equal (summary |> Seq.item 3)
//             dbSummary.FormaPrawna |> should equal (summary |> Seq.item 4)
//             dbSummary.Telefon |> should equal (summary |> Seq.item 5)
//             dbSummary.Email |> should equal (summary |> Seq.item 6)
//             dbSummary.Kontakt |> should equal (summary |> Seq.item 7)
//             dbSummary.OsobaDoKontaktu |> should equal (summary |> Seq.item 8)
//             dbSummary.TelefonOsobyKontaktowej |> should equal (summary |> Seq.item 9)
//             dbSummary.Dostepnosc |> should equal (summary |> Seq.item 10)
//             dbSummary.Kategoria |> should equal (summary |> Seq.item 11)
//             $"%i{dbSummary.LiczbaBeneficjentow}" |> should equal (summary |> Seq.item 12)
//              ) 
//     }