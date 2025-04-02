module Tests.Arranger

open IdGen
open System

let IdGenerator =
    IdGenerator(0, IdGeneratorOptions(timeSource = DefaultTimeSource(DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc))))

let f = Bogus.Faker()

let AnOrganization () : Organizations.Application.ReadModels.OrganizationDetails =
    { Teczka = IdGenerator.CreateId()
      IdentyfikatorEnova = IdGenerator.CreateId()
      NIP = IdGenerator.CreateId()
      Regon = IdGenerator.CreateId()
      KrsNr = IdGenerator.CreateId().ToString()
      FormaPrawna = f.PickRandomParam([| "Fundacja" |])
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
          OdbiórKrotkiTermin = f.Random.Bool()
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
        { Wniosek = Some <| f.Date.PastDateOnly(2)
          UmowaZDn = Some <| f.Date.PastDateOnly(2)
          UmowaRODO = f.PickRandom([| Some <| f.Date.PastDateOnly(2); None |])
          KartyOrganizacjiData = Some <| f.Date.PastDateOnly(2)
          OstatnieOdwiedzinyData = Some <| f.Date.PastDateOnly(1) } }
