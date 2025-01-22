module RenderBasedOnHtmx

open Microsoft.AspNetCore.Http
open Oxpecker

let render htmxTemplate fullPageTemplate (ctx: HttpContext) =
    if ctx.Request.Headers.ContainsKey "HX-Request" then
       ctx.WriteHtmlView htmxTemplate
    else
       ctx.WriteHtmlView fullPageTemplate