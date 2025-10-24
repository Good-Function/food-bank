module AuditTrailTests

open System
open Organizations.Application.Audit
open Organizations.Database.AuditTrailDao
open Organizations.FindDiffForAudit
open Xunit
open FsUnit.Xunit

type Test = { Name: string; Age: int }

[<Fact>]
let ``Audit Trail diff finds fields which value differs and dao can persist the difference`` () =
    task {
        // Arrange
        let auditTrailDao = AuditTrailDao Tools.DbConnection.connectDb

        let oldValue =
            { Name = Guid.NewGuid().ToString()
              Age = 32 }

        let newValue =
            { Name = Guid.NewGuid().ToString()
              Age = 32 }

        let auditTrail: AuditTrail =
            { Who = "marcin"
              OccuredAt =DateTime.UtcNow
              Kind = "Test"
              EntityId = 100
              Diff = findDiff oldValue newValue }
        // Act
        do! auditTrailDao.SaveAuditTrail auditTrail
        // Assert
        let! rows = auditTrailDao.ReadAuditTrail auditTrail.EntityId

        let oldState =
            rows
            |> List.tryFind (fun row ->
                row.Diff["Name"].Old.ToString() = oldValue.Name
                && row.Diff["Name"].New.ToString() = newValue.Name)
        auditTrail.Diff.Count |> should equal 1
        oldState |> should not' (be None)
    }