module Tools.HttResponseMessageToHtml

open System.Net.Http
open FSharp.Data
        
type HttpResponseMessage with
    member this.HtmlContent() =
        task {
            let! document = this.Content.ReadAsStringAsync()
            return HtmlDocument.Parse document
        }