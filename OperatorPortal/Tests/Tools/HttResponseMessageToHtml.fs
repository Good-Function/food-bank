module Tools.HttResponseMessageToHtml

open System.Net.Http
open AngleSharp.Html.Parser
open FSharp.Data

type HttpResponseMessage with
    member this.HtmlContent =
        task {
            let! document = this.Content.ReadAsStringAsync()
            return HtmlParser().ParseDocument document
        }
        
type HttpResponseMessage with
    member this.HtmlContentF =
        task {
            let! document = this.Content.ReadAsStringAsync()
            return FSharp.Data.HtmlDocument.Parse document
        }