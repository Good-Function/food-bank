module Organizations.Application.ReadModels

type OrganizationSummary = {
    Teczka: int 
    FormaPrawna: string 
    NazwaPlacowkiTrafiaZywnosc: string
    AdresPlacowkiTrafiaZywnosc: string
    GminaDzielnica: string
    Telefon: string
    Kontakt: string
    Email: string
    Dostepnosc: string
    OsobaDoKontaktu: string
    TelefonOsobyKontaktowej: string
    LiczbaBeneficjentow: int
    Kategoria: string
}

type ReadOrganizationSummaries = Async<OrganizationSummary list>