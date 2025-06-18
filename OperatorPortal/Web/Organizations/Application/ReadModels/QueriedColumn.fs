module Organizations.Application.ReadModels.QueriedColumn

open Microsoft.FSharp.Reflection

type QueriedColumn = 
  | Teczka
  | FormaPrawna
  | NazwaPlacowkiTrafiaZywnosc
  | AdresPlacowkiTrafiaZywnosc
  | GminaDzielnica
  | Beneficjenci
  | LiczbaBeneficjentow
  | Kategoria
  | OstatnieOdwiedzinyData
  member this.Label =
      match this with
      | Teczka -> "Teczk."
      | NazwaPlacowkiTrafiaZywnosc -> "Nazwa placówki"
      | AdresPlacowkiTrafiaZywnosc -> "Adres placówki"
      | GminaDzielnica -> "Gmina/Dzielnica"
      | FormaPrawna -> "Forma prawna"
      | Kategoria -> "Kategoria"
      | Beneficjenci -> "Beneficjenci"
      | LiczbaBeneficjentow -> "Liczba B."
      | OstatnieOdwiedzinyData -> "Odwiedzono"
  static member All =
        FSharpType.GetUnionCases(typeof<QueriedColumn>)
        |> Array.map(fun case -> FSharpValue.MakeUnion(case, [||]) :?> QueriedColumn)
        |> Array.toList
