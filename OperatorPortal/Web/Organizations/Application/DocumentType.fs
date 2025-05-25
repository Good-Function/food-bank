module Organizations.Application.DocumentType

type DocumentType = Wniosek | Umowa | RODO | Odwiedziny | UpowaznienieDoOdbioru
    with member this.toLabel =
            match this with
                | Wniosek -> "Wniosek"
                | Umowa -> "Umowa"
                | RODO -> "Rodo"
                | Odwiedziny -> "Odwiedziny"
                | UpowaznienieDoOdbioru -> "Upowaznienie do odbioru"
