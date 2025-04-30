module Organizations.ImportExcelResultTemplate

open Layout
open Organizations.Application.CreateOrganizationCommandHandler
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria

let Template ((importSummary, errors): ImportResult) =
    div (class'="ease-in") {
        div (style = "text-align:center;") {
            RingChart.plot (importSummary.ImportedCount, importSummary.TotalCount)
            hr ()
            h2 () { "Błędy" }
        }
        hr ()

        for error in errors do
            details () {
                summary(role="button", class'="outline") { $"Wiersz %i{error |> fst}" }

                small () {
                    ul () {
                        for cellError in error |> snd do
                            li () { cellError }
                    }
                }
            }
    }
