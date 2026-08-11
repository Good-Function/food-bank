module Organizations.EventSourcing.Events

open System

type EventAudit = {
    Who: string
    OccurredAt: DateTime
}

type KontaktyChangedV1 = {
    TeczkaId: int64
    Email: string
    Telefon: string
    OsobaDoKontaktu: string
    TelefonOsobyKontaktowej: string
    MailOsobyKontaktowej: string
    OsobaOdbierajacaZywnosc: string
    TelefonOsobyOdbierajacej: string
    Kontakt: string
    Przedstawiciel: string
    Dostepnosc: string
    WwwFacebook: string
    Audit: EventAudit
}

type ZrodlaZywnosciChangedV1 = {
    TeczkaId: int64
    Bazarki: bool
    FEPZ2024: bool
    OdbiorKrotkiTermin: bool
    Machfit: bool
    Sieci: bool
    TylkoNaszMagazyn: bool
    Audit: EventAudit
}

type AdresyKsiegowosciChangedV1 = {
    TeczkaId: int64
    KsiegowanieAdres: string
    NazwaOrganizacjiKsiegowanieDarowizn: string
    TelOrganProwadzacegoKsiegowosc: string
    Audit: EventAudit
}

type DaneAdresoweChangedV1 = {
    TeczkaId: int64
    AdresPlacowkiTrafiaZywnosc: string
    AdresRejestrowy: string
    NazwaOrganizacjiPodpisujacejUmowe: string
    Powiat: string
    GminaDzielnica: string
    NazwaPlacowkiTrafiaZywnosc: string
    Audit: EventAudit
}

type BeneficjenciChangedV1 = {
    TeczkaId: int64
    Beneficjenci: string
    LiczbaBeneficjentow: int
    Audit: EventAudit
}

type WarunkiPomocyChangedV1 = {
    TeczkaId: int64
    HACCP: bool
    Kategoria: string
    RodzajPomocy: string
    Sanepid: bool
    SposobUdzielaniaPomocy: string
    TransportKategoria: string
    TransportOpis: string
    WarunkiMagazynowe: string
    Audit: EventAudit
}

type OrganizationCreatedV1 = {
    TeczkaId: int64
    IdentyfikatorEnova: string
    NIP: string
    Regon: string
    KrsNr: string
    FormaPrawna: string
    OPP: bool
    DaneAdresowe: DaneAdresoweChangedV1
    Kontakty: KontaktyChangedV1
    ZrodlaZywnosci: ZrodlaZywnosciChangedV1
    AdresyKsiegowosci: AdresyKsiegowosciChangedV1
    Beneficjenci: BeneficjenciChangedV1
    WarunkiPomocy: WarunkiPomocyChangedV1
    Audit: EventAudit
}

type OrganizationEvent =
    | OrganizationCreated of OrganizationCreatedV1
    | KontaktyChanged of KontaktyChangedV1
    | ZrodlaZywnosciChanged of ZrodlaZywnosciChangedV1
    | AdresyKsiegowosciChanged of AdresyKsiegowosciChangedV1
    | DaneAdresoweChanged of DaneAdresoweChangedV1
    | BeneficjenciChanged of BeneficjenciChangedV1
    | WarunkiPomocyChanged of WarunkiPomocyChangedV1

module OrganizationEvent =
    let getEventType = function
        | OrganizationCreated _ -> "OrganizationCreatedV1"
        | KontaktyChanged _ -> "KontaktyChangedV1"
        | ZrodlaZywnosciChanged _ -> "ZrodlaZywnosciChangedV1"
        | AdresyKsiegowosciChanged _ -> "AdresyKsiegowosciChangedV1"
        | DaneAdresoweChanged _ -> "DaneAdresoweChangedV1"
        | BeneficjenciChanged _ -> "BeneficjenciChangedV1"
        | WarunkiPomocyChanged _ -> "WarunkiPomocyChangedV1"

    let getAudit = function
        | OrganizationCreated e -> e.Audit
        | KontaktyChanged e -> e.Audit
        | ZrodlaZywnosciChanged e -> e.Audit
        | AdresyKsiegowosciChanged e -> e.Audit
        | DaneAdresoweChanged e -> e.Audit
        | BeneficjenciChanged e -> e.Audit
        | WarunkiPomocyChanged e -> e.Audit
