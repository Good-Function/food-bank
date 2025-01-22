module Applications.Router

open Oxpecker
open RenderBasedOnHtmx

let renderApplications: EndpointHandler =
    render Template.Partial Template.FullPage
    
let Endpoints = [
    route "/" renderApplications
]