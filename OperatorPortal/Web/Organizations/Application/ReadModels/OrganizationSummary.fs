module Organizations.Application.ReadModels.OrganizationSummary

open System
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
      | Teczka -> failwith "Teczk."
      | NazwaPlacowkiTrafiaZywnosc -> "Nazwa placówki"
      | AdresPlacowkiTrafiaZywnosc -> "Adres placówki"
      | GminaDzielnica -> "Gmina/Dzielnica"
      | FormaPrawna -> "Forma prawna"
      | Kategoria -> "Kategoria"
      | Beneficjenci -> "Beneficjenci"
      | LiczbaBeneficjentow -> "Liczba B."
      | OstatnieOdwiedzinyData -> "Odwiedzono"
      
let QueriedColumns =
    FSharpType.GetUnionCases(typeof<QueriedColumn>)
    |> Array.map(fun case -> FSharpValue.MakeUnion(case, [||]) :?> QueriedColumn)
    |> Array.toList
    
let QueriedColumnsWithOperators =
    QueriedColumns |> List.collect (fun col -> 
        [string col; string col + "_op"]
    )
    
let QueriedColumnsWithOperatorsExcept (col: QueriedColumn) =
    QueriedColumnsWithOperators |> List.filter(not << _.StartsWith(string col))

type OrganizationSummary =
    { Teczka: int64
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
      Beneficjenci: string
      LiczbaBeneficjentow: int
      Kategoria: string
      OstatnieOdwiedzinyData: DateOnly option }

type Direction =
    | Asc
    | Desc

    override this.ToString() =
        this
        |> function
            | Asc -> "asc"
            | Desc -> "desc"
    member this.Reverse() =
        if this = Asc then
            Desc
        else Asc

    static member FromString(str: string) = if str = "desc" then Desc else Asc


type Filter = { Key: string; Value: obj; Operator: string }
type Query =
    { SearchTerm: string
      SortBy: (string * Direction) option
      Filters: Filter list
    }
    with static member Zero=
            { SearchTerm = ""
              SortBy = None
              Filters = []
            }

type ReadOrganizationSummaries = Query -> Async<OrganizationSummary list>