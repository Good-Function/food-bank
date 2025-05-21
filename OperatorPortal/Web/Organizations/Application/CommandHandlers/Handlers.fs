module Organizations.Application.Handlers

open System
open Organizations.Application.Commands

type ChangeZrodlaZywnosci = TeczkaId * ZrodlaZywnosci -> Async<unit>
type ChangeAdresyKsiegowosci = TeczkaId * AdresyKsiegowosci -> Async<unit>
type ChangeDaneAdresowe = TeczkaId * DaneAdresowe -> Async<unit>
type ChangeKontakty = TeczkaId * Kontakty -> Async<unit>
type ChangeBeneficjenci = TeczkaId * Beneficjenci -> Async<unit>
type ChangeDokumenty = TeczkaId * Dokumenty -> Async<unit>
type ChangeWarunkiPomocy = TeczkaId * WarunkiPomocy -> Async<unit>
type DeleteDocument = TeczkaId * string -> Async<unit>
type GenerateDownloadUri = TeczkaId * string -> Uri
