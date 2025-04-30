module Organizations.Domain.Identifiers

open System

type NonEmptyStringError = StringIsEmpty of string
type TeczkaIdError = InvalidTeczkaId
type KrsError = InvalidKrs
type RegonError = InvalidRegon
type NipError = InvalidNip

type NotEmptyString = private NotEmptyString of string
type TeczkaId = private TeczkaId of int64
type Krs = private Krs of string
type Regon = private Regon of string
type Nip = private Nip of string

module Regon =
    let create (input: string) =
        let cleaned = input.Replace("-", "").Trim()
        let len = cleaned.Length
        if (len = 9 || len = 14) && cleaned |> Seq.forall Char.IsDigit then Ok (Regon cleaned)
        else Error InvalidRegon

    let unwrap (Regon s) = s

module Krs =
    let create (input: string) =
        let cleaned = input.Replace("-", "").Trim()
        if cleaned.Length = 10 && cleaned |> Seq.forall Char.IsDigit then Ok(Krs cleaned)
        else Error InvalidKrs
        
    let unwrap (Krs s) = s

module NotEmptyString =
    let create str fieldName =
        if String.IsNullOrWhiteSpace(str) then Error (StringIsEmpty fieldName)
        else Ok (NotEmptyString str)

    let unwrap (NotEmptyString s) = s

module TeczkaId =
    let create (input: int64) =
        if input > 0 then Ok (TeczkaId input)
        else Error InvalidTeczkaId
        
    let parse (input: string) =
        match Int64.TryParse input with
        | true, result when result > 0 -> Ok (TeczkaId result)
        | _ -> Error InvalidTeczkaId
        
    let unwrap (TeczkaId s) = s

module Nip =
    let create (input: string) =
        let cleaned = input.Replace("-", "").Trim()
        if cleaned.Length = 10 && cleaned |> Seq.forall Char.IsDigit then Ok (Nip cleaned)
        else Error InvalidNip

    let unwrap (Nip s) = s