module Organizations.Templates.Kontakty

open Organizations.Application
open Layout.Fields
open Oxpecker.ViewEngine

let View (kontakty: ReadModels.Kontakty) (teczka: int64) =
    article () {
        editableHeader "Kontakty" $"/organizations/{teczka}/kontakty/edit"
        readonlyField "www / facebook" kontakty.WwwFacebook
        readonlyField "Telefon" kontakty.Telefon
        readonlyField "Przedstawiciel" kontakty.Przedstawiciel
        readonlyField "Kontakt" kontakty.Kontakt
        readonlyField "E-mail" kontakty.Email
        readonlyField "Dostępność" kontakty.Dostepnosc
        readonlyField "Osoba do kontaktu" kontakty.OsobaDoKontaktu
        readonlyField "Tel. osoby kontaktowej" kontakty.TelefonOsobyKontaktowej
        readonlyField "E-mail do osoby kontaktowej" kontakty.MailOsobyKontaktowej
        readonlyField "Osoba odbierająca żywność" kontakty.OsobaOdbierajacaZywnosc
        readonlyField "Telefon do os. odbierającej" kontakty.TelefonOsobyOdbierajacej
    }

let Form (kontakty: ReadModels.Kontakty) (teczka: int64) =
    form () {
        article (class' = "focus-dim") {
            activeEditableHeader "Kontakty" $"/organizations/{teczka}/kontakty"
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
