module Organizations.Router

open System.Threading
open Oxpecker
open RenderBasedOnHtmx

let renderOrganizations: EndpointHandler =
    fun ctx ->
        let shouldFetchData =
            match ctx.TryGetQueryValue "data" with
            | Some str when str = "true" -> true
            | _ -> false

        if shouldFetchData then
            Thread.Sleep(1500)

            ctx.WriteHtmlView(
                Template.DataTemplate
                    [ { Name = "Test Org"
                        City = "Pruszków"
                        ContactPerson = "Kazik Barazik" }
                      { Name = "OpenAI"
                        City = "Piaseczno"
                        ContactPerson = "Sam Altman" } ]
            )
        else
            ctx |> render Template.Partial Template.FullPage

let Endpoints = [ route "/" renderOrganizations ]
