module Organizations.Database.OrganizationsDao

open System.Data
open PostgresPersistence.DapperFsharp
open Organizations.Application.ReadModels


let readSummaries (connectDB: unit -> Async<IDbConnection>) (searchTerm: string) =
    async {
        let! db = connectDB()
        let! summaries = db.Query<OrganizationSummary> """
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
        let data = summaries
                |> List.filter (fun sum ->
                        sum.Teczka.ToString().Contains searchTerm || sum.NazwaPlacowkiTrafiaZywnosc.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
        return data
    }
    
let readBy (connectDB: unit -> Async<IDbConnection>) (teczka: int) =
    async {
        let! db = connectDB()
        return! db.Single<OrganizationDetails> """
SELECT * FROM organizacje WHERE teczka = @teczka
""" {|teczka = teczka|}
    }