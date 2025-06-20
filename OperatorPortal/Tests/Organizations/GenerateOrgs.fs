module Tests.Organizations.GenerateOrgs

open Organizations.Database.OrganizationsDao
open Tests
open Xunit

[<Fact(Skip="Use if you need many")>]
let generate() =
        task {
            for _ in 1..200 do        
                let organization = Arranger.AnOrganization()
                do! organization |> (save Tools.DbConnection.connectDb)
                return ()
        } |> Async.AwaitTask |> Async.RunSynchronously