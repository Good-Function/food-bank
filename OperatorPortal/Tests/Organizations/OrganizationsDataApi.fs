module OrganizationsDataApi

open System
open System.Net.Http
open System.Text.Json
open System.Threading.Tasks
open Organizations.Application.ReadModels.OrganizationDetails
open Tests
open Tests.Arranger
open Xunit
open Tools.TestServer
open FsUnit.Xunit
open Organizations.Database.OrganizationsDao

let getAndDeserialize<'T> (api: HttpClient) (url: string) : Task<'T> =
    task {
        let! response = api.GetAsync url
        response.EnsureSuccessStatusCode() |> ignore
        let! body = response.Content.ReadAsStringAsync()
        return JsonSerializer.Deserialize<'T>(
                    body,
                    JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
               )
    }

[<Fact>]
let ``/ogranizations/{data}?email returns data by email`` () =
    task {
        // Arrange
        let org = AnOrganization() |> setEmail (Guid.NewGuid().ToString())
        do! org |> (save Tools.DbConnection.connectDb)
        let api = runTestApi()
        // Act
        let! kontakty = getAndDeserialize<Kontakty> api $"/api/organizations/kontakty?email={org.Kontakty.Email}"
        let! beneficjenci = getAndDeserialize<Beneficjenci> api $"/api/organizations/beneficjenci?email={org.Kontakty.Email}"
        let! daneAdresowe = getAndDeserialize<DaneAdresowe> api $"/api/organizations/dane-adresowe?email={org.Kontakty.Email}"
        let! zrodla = getAndDeserialize<ZrodlaZywnosci> api $"/api/organizations/zrodla-zywnosci?email={org.Kontakty.Email}"
        let! warunki = getAndDeserialize<WarunkiPomocy> api $"/api/organizations/warunki-pomocy?email={org.Kontakty.Email}"
        // Assert
        kontakty.Dostepnosc |> should equal org.Kontakty.Dostepnosc
        kontakty.Email |> should equal org.Kontakty.Email
        kontakty.Kontakt |> should equal org.Kontakty.Kontakt
        kontakty.MailOsobyKontaktowej |> should equal org.Kontakty.MailOsobyKontaktowej
        kontakty.Dostepnosc |> should equal org.Kontakty.Dostepnosc
        kontakty.WwwFacebook |> should equal org.Kontakty.WwwFacebook
        kontakty.OsobaDoKontaktu |> should equal org.Kontakty.OsobaDoKontaktu
        kontakty.OsobaOdbierajacaZywnosc |> should equal org.Kontakty.OsobaOdbierajacaZywnosc
        kontakty.TelefonOsobyKontaktowej |> should equal org.Kontakty.TelefonOsobyKontaktowej
        kontakty.TelefonOsobyOdbierajacej |> should equal org.Kontakty.TelefonOsobyOdbierajacej
        beneficjenci.Beneficjenci |> should equal org.Beneficjenci.Beneficjenci
        daneAdresowe.AdresPlacowkiTrafiaZywnosc |> should equal org.DaneAdresowe.AdresPlacowkiTrafiaZywnosc
        daneAdresowe.AdresRejestrowy |> should equal org.DaneAdresowe.AdresRejestrowy
        daneAdresowe.GminaDzielnica |> should equal org.DaneAdresowe.GminaDzielnica
        daneAdresowe.NazwaOrganizacjiPodpisujacejUmowe |> should equal org.DaneAdresowe.NazwaOrganizacjiPodpisujacejUmowe
        daneAdresowe.NazwaPlacowkiTrafiaZywnosc |> should equal org.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc
        daneAdresowe.Powiat |> should equal org.DaneAdresowe.Powiat
        zrodla.Bazarki |> should equal org.ZrodlaZywnosci.Bazarki
        zrodla.FEPZ2024 |> should equal org.ZrodlaZywnosci.FEPZ2024
        zrodla.Machfit |> should equal org.ZrodlaZywnosci.Machfit
        zrodla.OdbiorKrotkiTermin |> should equal org.ZrodlaZywnosci.OdbiorKrotkiTermin
        zrodla.Sieci |> should equal org.ZrodlaZywnosci.Sieci
        zrodla.TylkoNaszMagazyn |> should equal org.ZrodlaZywnosci.TylkoNaszMagazyn
        warunki.HACCP |> should equal org.WarunkiPomocy.HACCP
        warunki.Kategoria |> should equal org.WarunkiPomocy.Kategoria
        warunki.RodzajPomocy |> should equal org.WarunkiPomocy.RodzajPomocy
        warunki.Sanepid |> should equal org.WarunkiPomocy.Sanepid
        warunki.SposobUdzielaniaPomocy |> should equal org.WarunkiPomocy.SposobUdzielaniaPomocy
        warunki.TransportKategoria |> should equal org.WarunkiPomocy.TransportKategoria
        warunki.TransportOpis |> should equal org.WarunkiPomocy.TransportOpis
        warunki.WarunkiMagazynowe |> should equal org.WarunkiPomocy.WarunkiMagazynowe
    }