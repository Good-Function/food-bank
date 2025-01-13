module Program

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Oxpecker

let endpoints = [
    route "/" <| text "Hello Warsaw from F#!"
    route "/health" <| text "up"
]

let createServer () =
    let builder = WebApplication.CreateBuilder()
    builder.Services.AddRouting().AddOxpecker() |> ignore
    let app = builder.Build()
    app.UseRouting().UseOxpecker(endpoints) |> ignore
    app

createServer().Run()
    