module HttpContextExtensions

open System
open System.Security.Claims
open Microsoft.AspNetCore.Http

type HttpContext with
    member this.UserName =
        this.User.FindFirstValue(ClaimTypes.Name)
        
type HttpContext with
    member this.UserId =
        this.User.FindFirstValue(ClaimTypes.NameIdentifier) |> Int64.Parse
        
type HttpContext with
    member this.TryGetFirstFile =
        if this.Request.HasFormContentType then
            this.Request.Form.Files |> Seq.tryHead
        else
            None