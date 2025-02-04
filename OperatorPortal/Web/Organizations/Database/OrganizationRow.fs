module Organizations.Database.OrganizationRow

type OrganizationRow = {
    Teczka: int
    IdentyfikatorEnova: int64
    NIP: int64 
    Regon: int64 
    KrsNr: string 
    FormaPrawna: string // fundacja, stowarzyszenie, org koscielna
    OPP: bool // szczegóły: Organizacja użytku publicznego
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
    FEPZ2024: bool // Fundusze europejskie na pomoc żywnościową, program UE.
    Kategoria: string 
    RodzajPomocy: string 
    SposobUdzielaniaPomocy: string 
    WarunkiMagazynowe: string 
    HACCP: bool // Hard analysis and critical control point - system bezpieczenstwa zywnosci. Np podpisujesz sie ze sprawdziles temperatura w chlodni. Teoretycznie kazda org powinna to miec.
    Sanepid: bool // Zgoda sanepidu. Suche paczki mogą pójść bez sanepidu. 
    TransportOpis: string 
    TransportKategoria: string 
    Wniosek: System.DateTime option 
    UmowaZDn: System.DateTime option  
    UmowaRODO: string 
    KartyOrganizacjiData: System.DateTime option 
    OstatnieOdwiedzinyData: System.DateTime option 
}
