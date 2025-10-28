module Organizations.Application.ReadModels.ReadAuditTrail

open Organizations.Application.Audit

type ReadAuditTrail = int64 * string option -> Async<AuditTrail list>