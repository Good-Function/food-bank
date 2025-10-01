module Organizations.FindDiffForAudit

open Microsoft.FSharp.Reflection
open Organizations.Application.Audit

let findDiff<'T> (oldVal: 'T) (newVal: 'T) : Map<string, DiffEntry> =
    if FSharpType.IsRecord typeof<'T> then
        let fields = FSharpType.GetRecordFields(typeof<'T>)
        let oldVals = FSharpValue.GetRecordFields oldVal
        let newVals = FSharpValue.GetRecordFields newVal

        fields
        |> Array.mapi (fun i fieldInfo ->
            let oldField = oldVals.[i]
            let newField = newVals.[i]
            if oldField <> newField then
                Some (fieldInfo.Name, { Old = oldField; New = newField; Type = fieldInfo.PropertyType.Name })
            else None
        )
        |> Array.choose id
        |> Map.ofArray
    else
        Map.empty