module Web.Organizations.Templates.List.Columns

type FilterType =
    | StringFilter
    | NumberFilter

type SortAndFilter = {
    Key: string
    Filter: FilterType option
}

type Column = {
    Label: string
    Width: int
    SortAndFilter: SortAndFilter option
}

let Columns: Column list = [
    { Label = "Teczk."; Width = 82; SortAndFilter = Some { Key="Teczka"; Filter = None } }
    { Label = "Nazwa placówki"; Width = 290; SortAndFilter = Some { Key="NazwaPlacowkiTrafiaZywnosc"; Filter = None } }
    { Label = "Adres placówki"; Width = 300; SortAndFilter = Some { Key="AdresPlacowkiTrafiaZywnosc"; Filter = None } }
    { Label = "Gmina/Dzielnica"; Width = 200; SortAndFilter = Some { Key="GminaDzielnica"; Filter = None } }
    { Label = "Forma prawna"; Width = 175; SortAndFilter = Some { Key="FormaPrawna"; Filter = None } }
    { Label = "Kategoria"; Width = 200; SortAndFilter = Some { Key="Kategoria"; Filter = None } }
    { Label = "Beneficjenci"; Width = 200; SortAndFilter = Some { Key="Beneficjenci"; Filter = None } }
    { Label = "Liczba B."; Width = 155; SortAndFilter = Some { Key="LiczbaBeneficjentow"; Filter = Some NumberFilter } }
    { Label = "Odwiedzono"; Width = 150; SortAndFilter = Some { Key="OstatnieOdwiedzinyData"; Filter = None } }
    { Label = "Kontakt"; Width = 110; SortAndFilter = None }
]