module Organizations.Application.Audit

open System

type DiffEntry = { Old: obj; New: obj; Type: string }
type Audit = { Who: string; OccuredAt: DateTime }

type AuditTrail =
    { Who: string
      OccuredAt: DateTime
      EntityId: int64
      Kind: string
      Diff: Map<string, DiffEntry> }
