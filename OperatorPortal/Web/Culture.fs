module Culture

open System
open System.Globalization
open System.Threading.Tasks
open Microsoft.AspNetCore.Http

let middleware =
    Func<HttpContext, RequestDelegate, Task>(fun ctx next ->
        task {
            match ctx.Request.Headers.TryGetValue("Accept-Language") with
            | true, values when not (String.IsNullOrWhiteSpace values.[0]) ->
                try
                    let culture = CultureInfo(values.[0].Split(',').[0])
                    CultureInfo.CurrentCulture <- culture
                    CultureInfo.CurrentUICulture <- culture
                with :? CultureNotFoundException ->
                    ()
            | _ -> ()

            return! next.Invoke(ctx)
        })
