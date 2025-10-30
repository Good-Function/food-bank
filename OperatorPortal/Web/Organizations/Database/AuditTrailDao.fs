module Organizations.Database.AuditTrailDao

open System
open System.Data
open System.Text.Json
open Organizations.Application.Audit
open Organizations.Application.ReadModels.ReadAuditTrail
open PostgresPersistence.DapperFsharp

type AuditTrailRow =
    { related_entity_id: int64
      who: string
      kind: string
      occured_at: DateTime
      diff: string }

type AuditTrailDao(connectDB: unit -> Async<IDbConnection>) =
    member this.SaveAuditTrail(auditTrail: AuditTrail) : Async<unit> =
        async {
            if auditTrail.Diff.IsEmpty then
                return ()
            else 
                use! db = connectDB ()

                let query =
                    """
                INSERT INTO audit_trail (related_entity_id, who, kind, occured_at, diff)
                VALUES (@related_entity_id, @who, @kind, @occured_at, @diff::jsonb)"""

                let parameters =
                    {| who = auditTrail.Who
                       occured_at = auditTrail.OccuredAt
                       diff = JsonSerializer.Serialize(auditTrail.Diff)
                       kind = auditTrail.Kind
                       related_entity_id = auditTrail.EntityId |}

                do! db.Execute query parameters
        }

    member this.ReadAuditTrail: ReadAuditTrail = fun(entityId: int64, kind: string option) ->
        async {
            use! db = connectDB ()
            let query = """
            SELECT related_entity_id, who, kind, occured_at, diff
            FROM audit_trail
            WHERE related_entity_id = @entityId
              AND (@kind::text IS NULL OR kind ILIKE @kind::text)
            ORDER BY occured_at DESC
            """

            let! rows = db.QueryBy<AuditTrailRow> query {| entityId = entityId; kind = kind |}
            return
                rows
                |> Seq.map (fun r ->
                    { EntityId = r.related_entity_id
                      Who = r.who
                      Kind = r.kind
                      OccuredAt = r.occured_at
                      Diff = JsonSerializer.Deserialize<Map<string,DiffEntry>>(r.diff) })
                |> Seq.toList
        }
