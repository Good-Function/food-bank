module Organizations.Application.ChangeOrgazniationCommands

open Organizations.Application.WriteModels

type ChangeZrodlaZywnosci = TeczkaId * ZrodlaZywnosci -> Async<unit>
type ChangeAdresyKsiegowosci = TeczkaId * AdresyKsiegowosci -> Async<unit>
type ChangeDaneAdresowe = TeczkaId * DaneAdresowe -> Async<unit>
type ChangeKontakty = TeczkaId * Kontakty -> Async<unit>
type ChangeBeneficjenci = TeczkaId * Beneficjenci -> Async<unit>
type ChangeDokumenty = TeczkaId * Dokumenty -> Async<unit>
type ChangeWarunkiPomocy = TeczkaId * WarunkiPomocy -> Async<unit>
