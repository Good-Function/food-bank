module Layout.RingChart

open Oxpecker.ViewEngine

let plot (value: int, max: int) =
    div(class' = "ring-chart", style = ($"--ring-value: {value}; --ring-max: {max};"))
        .data("value", $"{value}")
        .data ("max", $"{max}") {
        raw
            """
  <svg viewBox="-50 -50 100 100">
    <circle cx="0" cy="0" r="50%"/> 
  </svg>
"""
    }
