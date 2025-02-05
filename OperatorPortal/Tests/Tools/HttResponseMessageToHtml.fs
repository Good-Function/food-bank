module Tools.HttResponseMessageToHtml

open System.IO
open System.Net.Http
open FSharp.Data
        
type HttpResponseMessage with
    member this.HtmlContent =
        task {
            let! document = this.Content.ReadAsStringAsync()
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "test.txt"), document)
            return HtmlDocument.Parse document
        }