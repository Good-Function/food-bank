module Organizations.EventSourcing.AggregateIdMapping

open System
open System.Security.Cryptography
open System.Text
open Organizations.Domain.Identifiers

let private organizationNamespace =
    Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890")

let teczkaIdToGuid (teczkaId: TeczkaId) : Guid =
    let id = TeczkaId.unwrap teczkaId
    let name = $"Organization-{id}"
    let nameBytes = Encoding.UTF8.GetBytes(name)

    use sha1 = SHA1.Create()
    let namespaceBytes = organizationNamespace.ToByteArray()
    let combined = Array.concat [namespaceBytes; nameBytes]
    let hash = sha1.ComputeHash(combined)

    let guidBytes = hash.[0..15]
    guidBytes.[6] <- (guidBytes.[6] &&& 0x0Fuy) ||| 0x50uy
    guidBytes.[8] <- (guidBytes.[8] &&& 0x3Fuy) ||| 0x80uy

    Guid(guidBytes)
