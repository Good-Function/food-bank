module OrganizationsDataApi

open System
open System.Net.Http
open System.Text
open System.Text.Json
open System.Threading.Tasks
open Organizations.Application.Commands
open Organizations.Database
open Organizations.Domain.Identifiers
open Tests
open Tests.Arranger
open Tools
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
    
let putAndSerialize (api: HttpClient) (url: string) (data: obj) (userEmail: string) =
    task {
        use payload = new StringContent(JsonSerializer.Serialize data, Encoding.UTF8, "application/json")
        use request = new HttpRequestMessage(HttpMethod.Put, url)
        request.Content <- payload
        request.Headers.Add("X-User-Email", userEmail)
        let! response = api.SendAsync request
        response.EnsureSuccessStatusCode() |> ignore
    }

[<Fact>]
let ``/api/ogranizations/{id}/{data} returns data by email`` () =
    task {
        // Arrange
        let org = AnOrganization() |> setEmail (Guid.NewGuid().ToString())
        do! org |> (save Tools.DbConnection.connectDb)
        let id = org.Teczka |> TeczkaId.unwrap
        let api = runTestApi()
        // Act
        let! kontakty = getAndDeserialize<Kontakty> api $"/api/organizations/{id}/kontakty"
        let! beneficjenci = getAndDeserialize<Beneficjenci> api $"/api/organizations/{id}/beneficjenci"
        let! daneAdresowe = getAndDeserialize<DaneAdresowe> api $"/api/organizations/{id}/dane-adresowe"
        let! zrodla = getAndDeserialize<ZrodlaZywnosci> api $"/api/organizations/{id}/zrodla-zywnosci"
        let! warunki = getAndDeserialize<WarunkiPomocy> api $"/api/organizations/{id}/warunki-pomocy"
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
    
let createOrgAndApi() = task {
    let org = AnOrganization()
    do! org |> (save DbConnection.connectDb)
    let id = org.Teczka |> TeczkaId.unwrap
    let api = runTestApi()
    return (id, api)
}
    
[<Fact>]
let ``PUT /api/organizations/{id}/dane-adresowe updates data and stores audit with user email`` () = task {
    let userEmail = "test@gmail.com"
    let! id, api = createOrgAndApi()
    let randomStuff = Guid.NewGuid().ToString()
    let change = { NazwaOrganizacjiPodpisujacejUmowe = randomStuff
                   AdresRejestrowy = randomStuff
                   NazwaPlacowkiTrafiaZywnosc = randomStuff
                   AdresPlacowkiTrafiaZywnosc = randomStuff
                   GminaDzielnica = randomStuff
                   Powiat = randomStuff }

    do! putAndSerialize api $"api/organizations/{id}/dane-adresowe" change userEmail
    let! result = getAndDeserialize<Organizations.Application.ReadModels.OrganizationDetails.DaneAdresowe>
                        api $"/api/organizations/{id}/dane-adresowe"

    result.AdresRejestrowy |> should equal change.AdresRejestrowy
    result.NazwaPlacowkiTrafiaZywnosc |> should equal change.NazwaPlacowkiTrafiaZywnosc
    let! audit = AuditTrailDao.AuditTrailDao(DbConnection.connectDb).ReadAuditTrail(id, None)
    audit |> should haveLength 1
    audit.Head.Who |> should equal userEmail
    audit.Head.Kind |> should equal (nameof Organizations.Domain.Organization.DaneAdresowe)
}

[<Fact>]
let ``PUT /api/organizations/{id}/beneficjenci updates data and stores audit with user email`` () = task {
    let userEmail = "test@gmail.com"
    let! id, api = createOrgAndApi()
    let change = { LiczbaBeneficjentow = 200; Beneficjenci = "benef" }

    do! putAndSerialize api $"api/organizations/{id}/beneficjenci" change userEmail
    let! result = getAndDeserialize<Organizations.Application.ReadModels.OrganizationDetails.Beneficjenci>
                        api $"/api/organizations/{id}/beneficjenci"

    result.LiczbaBeneficjentow |> should equal change.LiczbaBeneficjentow
    let! audit = AuditTrailDao.AuditTrailDao(DbConnection.connectDb).ReadAuditTrail(id, None)
    audit |> should haveLength 1
    audit.Head.Who |> should equal userEmail
    audit.Head.Kind |> should equal (nameof Organizations.Domain.Organization.Beneficjenci)
}

[<Fact>]
let ``PUT /api/organizations/{id}/kontakty updates data and stores audit with user email`` () = task {
    let userEmail = "test@gmail.com"
    let! id, api = createOrgAndApi()
    let randomStuff = Guid.NewGuid().ToString()
    let change = { WwwFacebook = randomStuff
                   Telefon = randomStuff
                   Przedstawiciel = randomStuff
                   Kontakt = randomStuff
                   Email = randomStuff
                   Dostepnosc = randomStuff
                   OsobaDoKontaktu = randomStuff
                   TelefonOsobyKontaktowej = randomStuff
                   MailOsobyKontaktowej = randomStuff
                   OsobaOdbierajacaZywnosc = randomStuff
                   TelefonOsobyOdbierajacej = randomStuff }

    do! putAndSerialize api $"api/organizations/{id}/kontakty" change userEmail
    let! result = getAndDeserialize<Organizations.Application.ReadModels.OrganizationDetails.Kontakty>
                        api $"/api/organizations/{id}/kontakty"

    result.Email |> should equal change.Email
    result.OsobaDoKontaktu |> should equal change.OsobaDoKontaktu
    let! audit = AuditTrailDao.AuditTrailDao(DbConnection.connectDb).ReadAuditTrail(id, None)
    audit |> should haveLength 1
    audit.Head.Who |> should equal userEmail
    audit.Head.Kind |> should equal (nameof Organizations.Domain.Organization.Kontakty)
}

[<Fact>]
let ``PUT /api/organizations/{id}/warunki-pomocy updates data and stores audit with user email`` () = task {
    let userEmail = "test@gmail.com"
    let! id, api = createOrgAndApi()
    let randomStuff = Guid.NewGuid().ToString()
    let change = { Kategoria = randomStuff
                   RodzajPomocy = randomStuff
                   SposobUdzielaniaPomocy = randomStuff
                   WarunkiMagazynowe = randomStuff
                   HACCP = false
                   Sanepid = false
                   TransportOpis = randomStuff
                   TransportKategoria = randomStuff }

    do! putAndSerialize api $"api/organizations/{id}/warunki-pomocy" change userEmail
    let! result = getAndDeserialize<Organizations.Application.ReadModels.OrganizationDetails.WarunkiPomocy>
                        api $"/api/organizations/{id}/warunki-pomocy"

    result.HACCP |> should equal change.HACCP
    result.Kategoria |> should equal change.Kategoria
    let! audit = AuditTrailDao.AuditTrailDao(DbConnection.connectDb).ReadAuditTrail(id, None)
    audit |> should haveLength 1
    audit.Head.Who |> should equal userEmail
    audit.Head.Kind |> should equal (nameof Organizations.Domain.Organization.WarunkiPomocy)
}

[<Fact>]
let ``PUT /api/organizations/{id}/adresy-ksiegowosci updates data and stores audit with user email`` () = task {
    let userEmail = "test@gmail.com"
    let! id, api = createOrgAndApi()
    let randomStuff = Guid.NewGuid().ToString()
    let change = { NazwaOrganizacjiKsiegowanieDarowizn = randomStuff
                   KsiegowanieAdres = randomStuff
                   TelOrganProwadzacegoKsiegowosc = randomStuff }

    do! putAndSerialize api $"api/organizations/{id}/adresy-ksiegowosci" change userEmail
    let! result = getAndDeserialize<Organizations.Application.ReadModels.OrganizationDetails.AdresyKsiegowosci>
                        api $"/api/organizations/{id}/adresy-ksiegowosci"

    result.KsiegowanieAdres |> should equal change.KsiegowanieAdres
    let! audit = AuditTrailDao.AuditTrailDao(DbConnection.connectDb).ReadAuditTrail(id, None)
    audit |> should haveLength 1
    audit.Head.Who |> should equal userEmail
    audit.Head.Kind |> should equal (nameof Organizations.Domain.Organization.AdresyKsiegowosci)
}

[<Fact>]
let ``PUT /api/organizations/{id}/zrodla-zywnosci updates data and stores audit with user email`` () = task {
    let userEmail = "test@gmail.com"
    let! id, api = createOrgAndApi()
    let change = { Sieci = true
                   Bazarki = true
                   Machfit = true
                   FEPZ2024 = true
                   OdbiorKrotkiTermin = true
                   TylkoNaszMagazyn = true }

    do! putAndSerialize api $"api/organizations/{id}/zrodla-zywnosci" change userEmail
    let! result = getAndDeserialize<Organizations.Application.ReadModels.OrganizationDetails.ZrodlaZywnosci>
                        api $"/api/organizations/{id}/zrodla-zywnosci"

    result.Sieci |> should equal change.Sieci
    result.Bazarki |> should equal change.Bazarki
    let! audit = AuditTrailDao.AuditTrailDao(DbConnection.connectDb).ReadAuditTrail(id, None)
    audit |> should haveLength 1
    audit.Head.Who |> should equal userEmail
    audit.Head.Kind |> should equal (nameof Organizations.Domain.Organization.ZrodlaZywnosci)
}