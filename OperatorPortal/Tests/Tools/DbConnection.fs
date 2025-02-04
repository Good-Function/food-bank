module Tools.DbConnection

open PostgresPersistence.DapperFsharp

let connectDb = connectDB("Host=localhost;Port=5432;User Id=postgres;Password=Strong!Passw0rd;Database=food_bank;")