module Organizations.EventSourcing.Commands

open Organizations.Domain.Identifiers
open Organizations.Domain.FormaPrawna
open Organizations.Domain.Organization

type CreateOrganization = {
    Teczka: TeczkaId
    IdentyfikatorEnova: string
    NIP: Nip
    Regon: Regon
    FormaPrawna: FormaPrawna
    OPP: bool
    DaneAdresowe: DaneAdresowe
    Kontakty: Kontakty
    ZrodlaZywnosci: ZrodlaZywnosci
    AdresyKsiegowosci: AdresyKsiegowosci
    Beneficjenci: Beneficjenci
    WarunkiPomocy: WarunkiPomocy
}

type ChangeKontakty = {
    TeczkaId: TeczkaId
    Kontakty: Kontakty
}

type ChangeZrodlaZywnosci = {
    TeczkaId: TeczkaId
    ZrodlaZywnosci: ZrodlaZywnosci
}

type ChangeAdresyKsiegowosci = {
    TeczkaId: TeczkaId
    AdresyKsiegowosci: AdresyKsiegowosci
}

type ChangeDaneAdresowe = {
    TeczkaId: TeczkaId
    DaneAdresowe: DaneAdresowe
}

type ChangeBeneficjenci = {
    TeczkaId: TeczkaId
    Beneficjenci: Beneficjenci
}

type ChangeWarunkiPomocy = {
    TeczkaId: TeczkaId
    WarunkiPomocy: WarunkiPomocy
}

type OrganizationCommand =
    | CreateOrganization of CreateOrganization
    | ChangeKontakty of ChangeKontakty
    | ChangeZrodlaZywnosci of ChangeZrodlaZywnosci
    | ChangeAdresyKsiegowosci of ChangeAdresyKsiegowosci
    | ChangeDaneAdresowe of ChangeDaneAdresowe
    | ChangeBeneficjenci of ChangeBeneficjenci
    | ChangeWarunkiPomocy of ChangeWarunkiPomocy
