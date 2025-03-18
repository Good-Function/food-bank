module Layout.ProfilePopover

open Oxpecker.ViewEngine

let Template =
        div(popover="auto", id="ProfilePopover") {
            aside () {
                nav() {
                    li(style="display: block;") {
                        a(href="/settings/password-reset") {
                            "Zmiana hasła"
                        }
                    }
                    li(style="display: block;") {
                        a(href="/settings/csv-import") {
                            "Import csv"
                        }
                    }
                }
            }
        }