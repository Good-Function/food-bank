module Layout.ProfilePopover

open Oxpecker.ViewEngine

let Template =
        div(popover="auto", id="ProfilePopover") {
            aside () {
                nav() {
                    li(style="display: block;") {
                        a(href="/settings/password-reset") {
                            Icons.PassReset
                            "Zmiana hasła"
                        }
                    }
                    li(style="display: block;") {
                        a(href="/settings/csv-import") {
                            Icons.UploadFile
                            "Import csv"
                        }
                    }
                }
            }
        }