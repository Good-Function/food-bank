module Organizations.ListTemplate

open Oxpecker.ViewEngine

type OrganizationDto =
    { Name: string
      ContactPerson: string
      City: string }

let Template (data: OrganizationDto list) =
    div () {
        table (class' = "striped") {
            thead () {
                tr () {
                    th () { "Name" }
                    th () { "Contact person" }
                    th () { "City" }
                }
            }
            tbody () {
                for row in data do
                    tr () {
                        td () { row.Name }
                        td () { row.ContactPerson }
                        td () { row.City }
                    }
            }
        }
    }
