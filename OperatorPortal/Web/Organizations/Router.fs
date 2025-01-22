module Organizations.Router

open Oxpecker
open RenderBasedOnHtmx

let renderOrganizations: EndpointHandler =
    render Template.Partial Template.FullPage
    
let Endpoints = [
    route "/" renderOrganizations
]