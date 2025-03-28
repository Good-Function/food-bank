module Layout.ProfilePopover

open Oxpecker.ViewEngine

let Template =
    div (popover = "auto", id = "ProfilePopover") {
        aside () {
            nav () {
                ul () {
                    li (style = "display: block;") {
                        a (href = "/login/password-change") {
                            Icons.PassReset
                            "Zmiana hasła"
                        }
                    }

                    li (style = "display: block;") {
                        a (href = "/import") {
                            Icons.UploadFile
                            "Import csv"
                        }
                    }
                }
            }
        }
    }
