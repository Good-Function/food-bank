module Layout.ProfilePopover

open Oxpecker.ViewEngine

let Template =
    aside (style="width:170px;") {
        nav () {
            ul () {
                li (style = "display: block;") {
                    a (href = "/organizations/import") {
                        Icons.UploadFile
                        "Import csv"
                    }
                }
            }
        }
    }
