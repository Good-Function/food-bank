module Organizations.Application.ReadModels.OrganizationSummary

open System

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
