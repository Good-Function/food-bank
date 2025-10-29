module Organizations.Templates.Audit

open Organizations.Application.Audit
open Oxpecker.ViewEngine

let View (auditTrail: AuditTrail list) =
    dialog (open' = true) {
        article () {
            header () { "Historia zmian" }
            small () {
                form (method = "dialog") {
                    auditTrail |> function [] -> "Brak" | _ -> ""
                    ul (class' = "timeline") {
                        for trail in auditTrail do
                            li () {
                                Fragment() {
                                    b (style="color:var(--pico-primary)") {
                                        trail.OccuredAt.ToShortDateString() + " " + trail.OccuredAt.ToShortTimeString()
                                    }

                                    span () { " - " + trail.Who }
                                }

                                for KeyValue(key, value) in trail.Diff do
                                    br ()
                                    b () { key |> Formatters.pascalToWords }
                                    br ()
                                    Fragment() { $"{value.Old} → {value.New}" }
                            }
                    }
                    input (type' = "submit", formnovalidate = true, value = "Zamknij")
                }
            }
        }
    }
