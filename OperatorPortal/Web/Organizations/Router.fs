module Organizations.Router

open System.Threading
open Oxpecker
open RenderBasedOnHtmx

let renderPage = render PageTemplate.Partial PageTemplate.FullPage

let renderOrganizations: EndpointHandler =
    fun ctx ->
        Thread.Sleep(1000)

        ctx.WriteHtmlView(
            ListTemplate.Template
                [ { Name = "Test Org"
                    City = "Pruszków"
                    ContactPerson = "Kazik Barazik" }
                  { Name = "OpenAI"
                    City = "Piaseczno"
                    ContactPerson = "Sam Altman" } ]
        )

let Endpoints = [ route "/" renderPage; route "/list" renderOrganizations ]
