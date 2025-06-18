module Organizations.Templates.List.HxIncludes
    open Organizations.Application.ReadModels.QueriedColumn

    let inputNames =
        QueriedColumn.All |> List.collect (fun col ->
            if col = OstatnieOdwiedzinyData
                then [$"[name={col}]"]
                else [$"[name={col}]"; $"[name={col}_op]"]
        )
    
    let all = inputNames |> String.concat ", "
        
    let allExcept (col: QueriedColumn) =
        inputNames |> List.filter(not << _.Contains(string col)) |> String.concat ", "