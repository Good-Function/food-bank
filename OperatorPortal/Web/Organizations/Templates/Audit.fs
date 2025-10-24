module Organizations.Templates.Audit

open Organizations.Application.Audit
open Oxpecker.ViewEngine

let View (auditTrail: AuditTrail list) =
    article () {
        table() {
            thead() {
                tr() {
                    th() { "Who" }
                    th() { "OccuredAt" }
                    th() { "Field" }
                    th() { "Old" }
                    th() { "New" }
                }
            }
            tbody() {
                for trail in auditTrail do
                    for KeyValue(key, value) in trail.Diff do
                        tr() {
                            td() { trail.Who }
                            td() { trail.OccuredAt.ToString "u" }
                            td() { key }
                            td() { value.Old.ToString() }
                            td() { value.New.ToString() }
                        }
            }
        }
    }
