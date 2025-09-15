module Tests.Arranger

open Bogus.Extensions.Poland
open IdGen
open System
open Organizations.Domain.FormaPrawna
open Organizations.Domain.Identifiers
open Organizations.Domain.Organization
open FsToolkit.ErrorHandling

let IdGenerator =
    IdGenerator(0, IdGeneratorOptions(timeSource = DefaultTimeSource(DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc))))

let f = Bogus.Faker()


let getOrThrow (result: Result<'T, 'Error>) =
    Result.mapError (fun error -> InvalidOperationException(sprintf "%A" error)) result
    |> function
        | Ok v -> v
        | Error e -> failwith $"%A{e}"


let AnOrganization () : Organization =
    { Teczka = IdGenerator.CreateId() |> TeczkaId.create |> getOrThrow
      IdentyfikatorEnova = f.Random.Replace("##########")
      NIP = f.Company.Nip() |> Nip.create |> getOrThrow
      Regon = f.Company.Regon() |> Regon.create |> getOrThrow
      FormaPrawna = {
          Nazwa = f.PickRandomParam([| "Fundacja" |])
          Rejestracja = WRejestrzeKRS (f.Random.Replace("##########") |> Krs.create |> getOrThrow)
      }
      OPP = f.Random.Bool()
      DaneAdresowe =
        { NazwaOrganizacjiPodpisujacejUmowe = f.Company.CompanyName()
          AdresRejestrowy = f.Address.FullAddress()
          NazwaPlacowkiTrafiaZywnosc = f.Company.CompanyName()
          AdresPlacowkiTrafiaZywnosc = f.Address.FullAddress()
          GminaDzielnica =
            f.PickRandom(
                [| "Radzymin"
                   "Płońsk"
                   "Maków Mazowiecki"
                   "Ostrołęka"
                   "Płock"
                   "Siedlce"
                   "Radom"
                   "Ciechanów"
                   "Mława"
                   "Przasnysz" |]
            )
          Powiat =
            f.PickRandom(
                [| "płoński"
                   "ostrołęcki"
                   "przasnyski"
                   "ostrowski"
                   "makowski"
                   "ciechanowski"
                   "mławski"
                   "Warszawa"
                   "Radom"
                   "Płock" |]
            ) }
      AdresyKsiegowosci =
        { NazwaOrganizacjiKsiegowanieDarowizn = f.Company.CompanyName()
          KsiegowanieAdres = f.Address.FullAddress()
          TelOrganProwadzacegoKsiegowosc = f.Phone.PhoneNumber() }
      Kontakty =
        { WwwFacebook = f.Internet.UrlWithPath(protocol = "https", domain = "facebook.com")
          Telefon = f.Phone.PhoneNumber()
          Przedstawiciel = f.Person.FullName
          Kontakt = f.Person.Email
          Email = f.Person.Email
          Dostepnosc = f.PickRandom([| "Pn-Pt 12:00-17:00"; "Wt, Śr, Czw: 11:00-16:00" |])
          OsobaDoKontaktu = f.Person.FullName
          TelefonOsobyKontaktowej = f.Person.Phone
          MailOsobyKontaktowej = f.Person.Email
          OsobaOdbierajacaZywnosc = f.Person.FullName
          TelefonOsobyOdbierajacej = f.Person.Phone }
      Beneficjenci =
        { LiczbaBeneficjentow = f.Random.Number(5, 1000)
          Beneficjenci = f.PickRandom [| "Rodziny wielodzietne"; "osoby starsze"; "dom dziecka" |] }
      ZrodlaZywnosci =
        { Sieci = f.Random.Bool()
          Bazarki = f.Random.Bool()
          Machfit = f.Random.Bool()
          FEPZ2024 = f.Random.Bool()
          OdbiorKrotkiTermin = f.Random.Bool()
          TylkoNaszMagazyn = f.Random.Bool() }
      WarunkiPomocy =
        { Kategoria = f.PickRandom [| "Dystrybucja paczek żywnościowych"; "Pomoc żywnościowa" |]
          WarunkiMagazynowe =
            f.PickRandom
                [| "magazyn 10m2, regały, 1 lodówka 1,2 m"
                   "Magazyn 4 palety, pokój wydawania - regały, wiele pomieszczeń w podziemiach" |]
          HACCP = f.Random.Bool()
          Sanepid = f.Random.Bool()
          TransportOpis =
            f.PickRandom
                [| "prywatny pracownika SUV 500 kg"
                   "Pieniądze na transport w projekcie UM - wypożyczany" |]
          TransportKategoria = f.PickRandom [| "Własny"; "Potrzebny" |]
          RodzajPomocy = f.PickRandom [| "P"; "S"; "K" |]
          SposobUdzielaniaPomocy = "Wydawanie paczek 3 razy w tygodniu dla 10-15 osób dziennie" }
      Dokumenty =
        { Wniosek =
            { Date = Some <| f.Date.PastDateOnly(1)
              FileName = None }
          Umowa =
            { Date = Some <| f.Date.PastDateOnly(1)
              FileName = None }
          Rodo =
            { Date = Some <| f.Date.PastDateOnly(3)
              FileName = None }
          Odwiedziny = { Date = None; FileName = None }
          UpowaznienieDoOdbioru = { Date = None; FileName = None } } }

let setOstatnieOdwiedziny (newDate: DateOnly) (org: Organization) =
    { org with
        Organization.Dokumenty.Odwiedziny = { Date = Some <| newDate; FileName = None } }
    
let setEmail(email: string) (org: Organization) =
    { org with
        Organization.Kontakty.Email = email }

let setNazwaPlacowki (newName: string) (org: Organization) =
    { org with
        Organization.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc = newName }
