module Organizations.Database.OrganizationRow

type OrganizationRow = {
    Teczka: int
    IdentyfikatorEnova: int64
    NIP: int64
    Regon: int64
    KrsNr: string
    FormaPrawna: string
    OPP: bool
    NazwaOrganizacjiPodpisujacejUmowe: string
    AdresRejestrowy: string
    NazwaPlacowkiTrafiaZywnosc: string
    AdresPlacowkiTrafiaZywnosc: string
    GminaDzielnica: string
    Powiat: string
    NazwaOrganizacjiKsiegowanieDarowizn: string
    KsiegowanieAdres: string
    TelOrganProwadzacegoKsiegowosc: string
    WwwFacebook: string
    Telefon: string
    Przedstawiciel: string
    Kontakt: string
    Fax: string
    Email: string
    Dostepnosc: string
    OsobaDoKontaktu: string
    TelefonOsobyKontaktowej: string
    MailOsobyKontaktowej: string
    OsobaOdbierajacaZywnosc: string
    TelefonOsobyOdbierajacej: string
    LiczbaBeneficjentow: int
    Beneficjenci: string
    Sieci: bool
    Bazarki: bool
    Machfit: bool
    FEPZ2024: bool
    Kategoria: string
    RodzajPomocy: string
    SposobUdzielaniaPomocy: string
    WarunkiMagazynowe: string
    HACCP: bool
    Sanepid: bool
    TransportOpis: string
    TransportKategoria: string
    Wniosek: System.DateTime option
    UmowaZDn: System.DateTime option
    UmowaRODO: string
    KartyOrganizacjiData: System.DateTime option
    OstatnieOdwiedzinyData: System.DateTime option
}
