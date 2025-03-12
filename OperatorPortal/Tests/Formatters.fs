module Tests.Formatters

let toTakNie (isTrue: bool) =
    $"""{if isTrue then "Tak" else "Nie"}"""
