module PostgresPersistence.OptionHandler

open System
open Dapper

type OptionHandler<'T>() =
  inherit SqlMapper.TypeHandler<option<'T>>()

  override _.SetValue(param, value) =
    let valueOrNull =
      match value with
      | Some x -> box x
      | None -> null

    param.Value <- valueOrNull

  override _.Parse value =
    if Object.ReferenceEquals(value, null) || value = box DBNull.Value then
      None
    else
      Some(value :?> 'T)

module OptionHandler =
  let RegisterTypes () =
    SqlMapper.AddTypeHandler(OptionHandler<string>())
    SqlMapper.AddTypeHandler(OptionHandler<DateTime>())