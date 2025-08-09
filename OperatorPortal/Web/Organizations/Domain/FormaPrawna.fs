module Organizations.Domain.FormaPrawna

open Organizations.Domain.Identifiers
open FsToolkit.ErrorHandling
    
type Rejestracja =
    | PozaRejestrem of NumerRejestrowy: string
    | WRejestrzeKRS of Krs

type FormaPrawna = 
    { Nazwa: string
      Rejestracja: Rejestracja }

module FormaPrawna =
    let tryCreate (formaPrawna: string, krs: string) : Result<FormaPrawna, KrsError> =
        result {
            match formaPrawna with
            | "Organizacja Kościelna" | "PCK" | "Stowarzyszenie zwykłe" ->
                return { Nazwa = formaPrawna; Rejestracja = Rejestracja.PozaRejestrem krs }
            | _ ->
                let! krsValue = Krs.create krs
                return { Nazwa = formaPrawna; Rejestracja = Rejestracja.WRejestrzeKRS krsValue }
        }
