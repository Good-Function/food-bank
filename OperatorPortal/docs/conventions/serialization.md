# JSON Serialization Conventions

**Last Updated:** 2026-01-05
**Status:** Active

---

## Overview

This document defines the JSON serialization standards for the OperatorPortal project. All JSON serialization MUST use **Thoth.Json.Net** to ensure consistency across the codebase.

---

## Library: Thoth.Json.Net

**Package:** `Thoth.Json.Net` (version 12.0.0+)

**Why Thoth.Json.Net?**
- **F#-First Design:** Built specifically for F# with excellent support for F# types (discriminated unions, records, options)
- **Type Safety:** Compile-time type checking with auto-generated encoders/decoders
- **Consistent API:** Simple, predictable API across the entire codebase
- **Composable:** Easy to build custom encoders/decoders for domain types
- **Error Handling:** Built-in Result-based error handling for deserialization

**Prohibited Libraries:**
- ❌ `System.Text.Json` - Not F#-friendly, requires extensive configuration for F# types
- ❌ `Newtonsoft.Json` - Legacy library, Thoth provides better F# support
- ❌ `FSharp.SystemTextJson` - Wrapper around System.Text.Json, prefer Thoth for consistency

---

## Basic Usage

### Serialization (Encoding)

```fsharp
open Thoth.Json.Net

// Simple serialization with camelCase
let serialize<'T> (value: 'T) : string =
    Encode.Auto.toString(0, value, caseStrategy = CaseStrategy.CamelCase)

// Example usage
type Person = { FirstName: string; LastName: string; Age: int }

let person = { FirstName = "John"; LastName = "Doe"; Age = 30 }
let json = Encode.Auto.toString(0, person, caseStrategy = CaseStrategy.CamelCase)
// Result: {"firstName":"John","lastName":"Doe","age":30}
```

### Deserialization (Decoding)

```fsharp
open Thoth.Json.Net

// Simple deserialization with error handling
let deserialize<'T> (json: string) : Result<'T, string> =
    Decode.Auto.fromString<'T>(json, caseStrategy = CaseStrategy.CamelCase)

// Example usage with error handling
match Decode.Auto.fromString<Person>(json, caseStrategy = CaseStrategy.CamelCase) with
| Ok person ->
    printfn $"Successfully deserialized: {person.FirstName} {person.LastName}"
| Error error ->
    printfn $"Deserialization failed: {error}"

// Unsafe variant (throws exception on failure)
let deserializeUnsafe<'T> (json: string) : 'T =
    match Decode.Auto.fromString<'T>(json, caseStrategy = CaseStrategy.CamelCase) with
    | Ok value -> value
    | Error error -> failwith $"Failed to deserialize: {error}"
```

---

## Naming Strategy

**Default:** Use **`CaseStrategy.CamelCase`** for all JSON serialization unless explicitly required otherwise.

This ensures consistency with:
- Modern JSON conventions
- JavaScript/TypeScript frontend code
- Most external APIs

**Example:**
```fsharp
// F# record
type Organization = {
    TeczkaId: int64
    NazwaOrganizacji: string
    EmailKontaktowy: string
}

// Serialized JSON (camelCase)
{
  "teczkaId": 123,
  "nazwaOrganizacji": "Organizacja Pomocowa",
  "emailKontaktowy": "kontakt@example.com"
}
```

**When to use `CaseStrategy.PascalCase`:**
- When serializing for PostgreSQL JSONB columns that use PascalCase (legacy compatibility)
- When interfacing with external systems that explicitly require PascalCase

---

## Custom Encoders and Decoders

For domain types that need special handling (e.g., `DateOnly`, single-case unions, value objects), create custom encoders and decoders.

### Example: DateOnly Codec

```fsharp
module DateOnlyCodec =
    open System
    open Thoth.Json.Net

    let encode (date: DateOnly) : JsonValue =
        Encode.string (date.ToString("yyyy-MM-dd"))

    let decode : Decoder<DateOnly> =
        Decode.string
        |> Decode.andThen (fun str ->
            match DateOnly.TryParse str with
            | true, value -> Decode.succeed value
            | false, _ -> Decode.fail $"Invalid DateOnly string: {str}"
        )
```

### Using Custom Codecs

```fsharp
open Thoth.Json.Net

type Event = {
    EventDate: DateOnly
    Description: string
}

// Encoding with custom codec
let extra = Extra.empty |> Extra.withCustom DateOnlyCodec.encode DateOnlyCodec.decode
let json = Encode.Auto.toString(0, event, caseStrategy = CaseStrategy.CamelCase, extra = extra)

// Decoding with custom codec
let result = Decode.Auto.fromString<Event>(json, caseStrategy = CaseStrategy.CamelCase, extra = extra)
```

---

## Common Patterns

### 1. EventStore Serialization

For event sourcing, always use camelCase for JSONB storage:

```fsharp
module EventStore.Serialization

open Thoth.Json.Net

let serialize<'T> (event: 'T) : string =
    Encode.Auto.toString(0, event, caseStrategy = CaseStrategy.CamelCase)

let deserialize<'T> (json: string) : 'T =
    match Decode.Auto.fromString<'T>(json, caseStrategy = CaseStrategy.CamelCase) with
    | Ok value -> value
    | Error error -> failwith $"Failed to deserialize event: {error}"
```

### 2. Database JSONB Columns

For PostgreSQL JSONB columns, match the existing casing strategy in the database:

```fsharp
// If existing JSONB uses PascalCase (legacy)
let jsonb = Encode.Auto.toString(0, data, CaseStrategy.PascalCase, extra)

// For new JSONB columns, prefer camelCase
let jsonb = Encode.Auto.toString(0, data, CaseStrategy.CamelCase, extra)
```

### 3. API Responses

For HTTP API responses, always use camelCase:

```fsharp
let serializeResponse (response: 'T) : string =
    Encode.Auto.toString(0, response, caseStrategy = CaseStrategy.CamelCase)
```

---

## Error Handling

**Always handle deserialization errors explicitly** using the `Result` type:

```fsharp
// ✅ GOOD: Explicit error handling
match Decode.Auto.fromString<MyType>(json, caseStrategy = CaseStrategy.CamelCase) with
| Ok value ->
    // Handle success
    Ok value
| Error error ->
    // Log and handle error
    logger.LogError($"Deserialization failed: {error}")
    Error $"Invalid JSON: {error}"

// ❌ BAD: Throwing exceptions for control flow
let value = Decode.Auto.fromString<MyType>(json) |> Result.get
```

**Exception:** In rare cases where deserialization failure is truly exceptional (e.g., deserializing known-good JSON from the database), you may use `failwith` or `Result.get`, but document why:

```fsharp
// JUSTIFICATION: JSON is from database, already validated on insert
let deserialize<'T> (json: string) : 'T =
    match Decode.Auto.fromString<'T>(json, caseStrategy = CaseStrategy.CamelCase) with
    | Ok value -> value
    | Error error -> failwith $"Failed to deserialize event from database: {error}"
```

---

## Testing

Always test custom encoders/decoders with round-trip tests:

```fsharp
[<Fact>]
let ``DateOnly roundtrip serialization`` () =
    // Arrange
    let original = DateOnly(2026, 1, 5)

    // Act
    let json = DateOnlyCodec.encode original
    let decoded = Decode.fromValue "$" DateOnlyCodec.decode json

    // Assert
    match decoded with
    | Ok value -> value |> should equal original
    | Error error -> failwith $"Deserialization failed: {error}"
```

---

## Migration Guide

### From System.Text.Json to Thoth.Json.Net

If you encounter code using `System.Text.Json`, migrate it as follows:

**Before:**
```fsharp
open System.Text.Json
open System.Text.Json.Serialization

let options = JsonSerializerOptions()
options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
options.Converters.Add(JsonFSharpConverter())

let serialize (value: 'T) : string =
    JsonSerializer.Serialize(value, options)

let deserialize (json: string) : 'T =
    JsonSerializer.Deserialize<'T>(json, options)
```

**After:**
```fsharp
open Thoth.Json.Net

let serialize (value: 'T) : string =
    Encode.Auto.toString(0, value, caseStrategy = CaseStrategy.CamelCase)

let deserialize (json: string) : Result<'T, string> =
    Decode.Auto.fromString<'T>(json, caseStrategy = CaseStrategy.CamelCase)
```

---

## Summary

✅ **DO:**
- Use Thoth.Json.Net for all JSON serialization
- Use `CaseStrategy.CamelCase` as default
- Handle deserialization errors with `Result` type
- Create custom codecs for domain types that need special handling
- Test serialization round-trips

❌ **DON'T:**
- Use System.Text.Json or Newtonsoft.Json
- Ignore deserialization errors
- Mix different JSON libraries in the same module
- Use different casing strategies without documentation

---

## References

- **Thoth.Json.Net Documentation:** https://thoth-org.github.io/Thoth.Json/
- **EventStore Serialization Implementation:** `Web/EventStore/EventStore.Serialization.fs`
- **Custom Codec Example:** `Web/Organizations/Database/DateOnlyCoder.fs`
