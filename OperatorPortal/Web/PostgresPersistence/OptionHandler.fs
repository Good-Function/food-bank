module PostgresPersistence.OptionHandler

open System
open Dapper

type DateOnlyOptionHandler() =
  inherit SqlMapper.TypeHandler<option<DateOnly>>()
  
  override _.SetValue(param, value) =
    let valueOrNull =
      match value with
      | Some date -> box (date.ToDateTime(TimeOnly(0, 0)))
      | None -> null

    param.Value <- valueOrNull
    
  override _.Parse(value) =
    if Object.ReferenceEquals(value, null) || value = box DBNull.Value then
      None
    else
      Some(DateOnly.FromDateTime(value :?> DateTime))
    

type DateOnlyHandler() =
  inherit SqlMapper.TypeHandler<DateOnly>()
  
  override _.SetValue(param, date) =
    param.Value <- date.ToDateTime(TimeOnly(0, 0))
    
  override _.Parse(value) =
    DateOnly.FromDateTime(value :?> DateTime)

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
    SqlMapper.AddTypeHandler(DateOnlyHandler())
    SqlMapper.AddTypeHandler(DateOnlyOptionHandler())
    SqlMapper.AddTypeHandler(OptionHandler<string>())
    SqlMapper.AddTypeHandler(OptionHandler<DateTime>())
