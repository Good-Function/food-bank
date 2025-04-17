module Organizations.ImportExcelResultTemplate

open Layout
open Organizations.Application.Commands
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria

let Template (output: int * RowError list) =
    div (class'="ease-in") {
        div (style = "text-align:center;") {
            RingChart.plot (output |> fst, (output |> fst) + (output |> snd).Length)
            hr ()
            h2 () { "Błędy" }
        }

        hr ()

        for rowError in (output |> snd) do
            details () {
                summary(role="button", class'="outline") { $"Wiersz %i{rowError |> fst}" }

                small () {
                    ul () {
                        for cellError in rowError |> snd do
                            li () { cellError }
                    }
                }
            }
    }
