module Organizations.Templates.Kontakty

open Layout
open Organizations.Application
open Organizations.Templates.Fields
open Oxpecker.ViewEngine
open Oxpecker.Htmx

let View (kontakty: ReadModels.Kontakty) (teczka: int64) =
    article () {
        header (class' = "action-header") {
            span () { "Kontakty" }

            div (class' = "action-header-actions") {
                span (
                    hxGet = $"/organizations/{teczka}/kontakty/edit",
                    hxTarget = "closest article",
                    hxSwap = "outerHTML"
                ) {
                    Icons.Pen
                }
            }
        }

        field "www / facebook" kontakty.WwwFacebook
        field "Telefon" kontakty.Telefon
        field "Przedstawiciel" kontakty.Przedstawiciel
        field "Kontakt" kontakty.Kontakt
        field "E-mail" kontakty.Email
        field "Dostępność" kontakty.Dostepnosc
        field "Osoba do kontaktu" kontakty.OsobaDoKontaktu
        field "Tel. osoby kontaktowej" kontakty.TelefonOsobyKontaktowej
        field "E-mail do osoby kontaktowej" kontakty.MailOsobyKontaktowej
        field "Osoba odbierająca żywność" kontakty.OsobaOdbierajacaZywnosc
        field "Telefon do os. odbierającej" kontakty.TelefonOsobyOdbierajacej
    }

let Form (kontakty: ReadModels.Kontakty) (teczka: int64) =
    form () {
        article (class' = "focus-dim") {
            header (class' = "action-header") {
                span () { "Kontakty" }

                div (class' = "action-header-actions") {
                    span (
                        hxGet = $"/organizations/{teczka}/kontakty",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Cancel
                    }

                    span (
                        hxPut = $"/organizations/{teczka}/kontakty",
                        hxTarget = "closest article",
                        hxSwap = "outerHTML"
                    ) {
                        Icons.Ok
                    }
                }
            }

            editField "www / facebook" kontakty.WwwFacebook (nameof kontakty.WwwFacebook)
            editField "Telefon" kontakty.Telefon (nameof kontakty.Telefon)
            editField "Przedstawiciel" kontakty.Przedstawiciel (nameof kontakty.Przedstawiciel)
            editField "Kontakt" kontakty.Kontakt (nameof kontakty.Kontakt)
            editField "E-mail" kontakty.Email (nameof kontakty.Email)
            editField "Dostępność" kontakty.Dostepnosc (nameof kontakty.Dostepnosc)
            editField "Osoba do kontaktu" kontakty.OsobaDoKontaktu (nameof kontakty.OsobaDoKontaktu)
            editField "Tel. osoby kontaktowej" kontakty.TelefonOsobyKontaktowej (nameof kontakty.TelefonOsobyKontaktowej)
            editField "E-mail do osoby kontaktowej" kontakty.MailOsobyKontaktowej (nameof kontakty.MailOsobyKontaktowej)
            editField "Osoba odbierająca żywność" kontakty.OsobaOdbierajacaZywnosc (nameof kontakty.OsobaOdbierajacaZywnosc)
            editField "Telefon do os. odbierającej" kontakty.TelefonOsobyOdbierajacej (nameof kontakty.TelefonOsobyOdbierajacej)
        }
    }
