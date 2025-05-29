module Layout.DeletedFileInput

open Oxpecker.ViewEngine

let View (fileInputName: string, fileName: string) =
    Fragment() {
        input (
            type' = "file",
            name = fileInputName,
            accept = ".pdf",
            style = "padding: 0;margin: 0; height: 32px;",
            class' = "in-table-file-input"
        )

        input (type' = "hidden", name = $"Delete{fileInputName}", value = $"{fileName}")
    }
