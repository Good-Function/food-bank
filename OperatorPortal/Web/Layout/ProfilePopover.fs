module Layout.ProfilePopover

open Oxpecker.ViewEngine

let Template =
        div(popover="auto", id="ProfilePopover") {
            nav() {
                li() {
                    a(href="/password") {
                        "Zmiana hasła"
                    }
                }
            }
        }