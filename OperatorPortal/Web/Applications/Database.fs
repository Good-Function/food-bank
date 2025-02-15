module Applications.Database

open System.Data
open PostgresPersistence.DapperFsharp

let readSchemas (connectDB: unit -> Async<IDbConnection>) =
    async {
        let! db = connectDB()
        let! schemas = db.Query<{|schema_name:string|}> """
SELECT nspname as schema_name FROM pg_namespace;
"""
        return schemas |> List.map _.schema_name
    }
    