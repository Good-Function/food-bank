module Organizations.Database.OrganizationsDao

open System.Data
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