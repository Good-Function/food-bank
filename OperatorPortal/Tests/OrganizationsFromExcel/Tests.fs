module Tests.OrganizationsFromExcel.Tests

open Xunit
open FsUnit.Xunit
open Organizations.Database.csvLoader
    
[<Literal>]
let ResolutionFolder = __SOURCE_DIRECTORY__      
let sampleOrgs =
    Orgs.GetSample()
    
[<Fact>]
let ``Sample excel can be parsed to organizations and saved`` ()=
    let row = sampleOrgs.Rows |> Seq.head
    let a = parse row
    true |> should equal false