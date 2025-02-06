module Organizations.Database.OrganizationsDao

open System.Data
open Organizations.Database.OrganizationRow
open PostgresPersistence.DapperFsharp
open Organizations.Application.ReadModels

let readSummaries (connectDB: unit -> Async<IDbConnection>) =
    async {
        let! db = connectDB()
        return! db.Query<OrganizationSummary> """
SELECT 
    teczka,
    formaprawna,
    nazwaplacowkitrafiazywnosc,
    adresplacowkitrafiazywnosc,
    gminadzielnica,
    telefon,
    kontakt,
    email,
    dostepnosc,
    osobadokontaktu,
    telefonosobykontaktowej,
    liczbabeneficjentow,
    kategoria
FROM organizacje ORDER BY teczka 
"""
    }
    
let readBy (connectDB: unit -> Async<IDbConnection>) (teczka: int) =
    async {
        let! db = connectDB()
        return! db.Single<OrganizationRow> """
SELECT * FROM organizacje WHERE teczka = @teczka
""" {|teczka = teczka|}
    }