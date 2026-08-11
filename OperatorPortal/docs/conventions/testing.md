# Testing Guidelines

## Philosophy

This project follows an **integration-first testing approach**. Tests verify complete user workflows from HTTP request through database persistence and back, using real infrastructure (PostgreSQL, Azure Storage) via Testcontainers rather than heavy mocking.

### Core Principles

1. **Test outcomes, not implementation details**
2. **Use real dependencies where practical** (databases, storage, external services)
3. **Make tests readable as specifications** through descriptive naming
4. **Assert specific values** that match what the test name claims
5. **One concept per test** - tests should verify a single behavioral outcome

---

## Test Naming Conventions

### Pattern: Describe Outcomes

Tests should describe **what happens** when **specific conditions** are met. Use F#'s backtick syntax to write test names as natural language specifications.

**Format:** `` `[HTTP Method] [Endpoint] [outcome] when [condition]` ``

#### Good Examples

```fsharp
[<Fact>]
let ``GET /organizations/{id} returns 404 when organization does not exist`` () = task { ... }

[<Fact>]
let ``PUT /organizations/{id}/kontakty updates email and returns updated view`` () = task { ... }

[<Fact>]
let ``POST /organizations/import/upload returns validation errors when NIP is invalid`` () = task { ... }

[<Fact>]
let ``GET /organizations/summaries orders by last visit date descending when sorted by OstatnieOdwiedziny`` () = task { ... }
```

#### Bad Examples

```fsharp
// Too vague - doesn't describe the outcome
let ``test organization update`` () = task { ... }

// Describes the action, not the result
let ``calls save function`` () = task { ... }

// Missing context about conditions
let ``returns error`` () = task { ... }
```

### Test Names as Documentation

**If someone reads ONLY the test names, they should understand the complete behavior of the system.**

When reviewing a test module, the list of test names should read like a specification document that describes all the ways the feature behaves under different conditions.

---

## Test Organization

### Structure: Arrange-Act-Assert (AAA)

Every test should follow this clear three-phase structure with comments:

```fsharp
[<Fact>]
let ``PUT /organizations/{id}/dane-adresowe modifies and returns updated data`` () =
    task {
        // Arrange - Set up test data and dependencies
        let organization = Arranger.AnOrganization()
        do! organization |> (save Tools.DbConnection.connectDb)
        let randomStuff = Guid.NewGuid().ToString()
        let api = runTestApi() |> authenticate "Editor"
        let teczka = organization.Teczka |> TeczkaId.unwrap

        // Act - Execute the operation being tested
        let! response = putFormWithToken
            api
            ($"/organizations/{teczka}/dane-adresowe/edit")
            ($"/organizations/{teczka}/dane-adresowe")
            [
                ("NazwaOrganizacjiPodpisujacejUmowe", randomStuff)
                ("AdresRejestrowy", randomStuff)
                ("NazwaPlacowkiTrafiaZywnosc", randomStuff)
            ]

        // Assert - Verify the outcome
        let! doc = response.HtmlContent()
        let inputs = doc.CssSelect "small" |> List.map _.InnerText()
        response.StatusCode |> should equal HttpStatusCode.OK
        inputs |> should equal [randomStuff; randomStuff; randomStuff]
    }
```

### File Organization

Tests are organized by feature module, mirroring the application structure:

```
Tests/
├── Tools/              # Test infrastructure, helpers, builders
├── E2E/               # End-to-end Playwright tests
├── Organizations/     # Organization domain tests
│   ├── Viewing.fs     # GET operations
│   ├── Editing.fs     # PUT/POST operations
│   ├── Arranger.fs    # Test data builder
│   └── OrganizationsDataApi.fs  # API tests
├── Login/             # Authentication tests
├── Team/              # Team management tests
└── Learning/          # Exploratory/spike tests (not run in CI)
```

**Naming Convention:**
- Test files: Verb-based names describing the behavior tested (e.g., `Editing.fs`, `Viewing.fs`, `Filtering.fs`)
- Module names: Match file names with namespace prefix (e.g., `module Organizations.Editing`)

### One Concept Per Test

Each test should verify a single behavioral outcome. If you need to assert multiple aspects, consider whether they represent different concepts:

```fsharp
// GOOD - Single concept: Authorization check
[<Fact>]
let ``PUT /organizations/{id}/kontakty requires EditOrganization permission`` () = task {
    let api = runTestApi() |> authenticate "Reader"
    let! response = api.PutAsync("/organizations/1/kontakty", null)
    response.StatusCode |> should equal HttpStatusCode.Forbidden
}

// GOOD - Single concept: Data persistence
[<Fact>]
let ``PUT /organizations/{id}/kontakty persists email to database`` () = task {
    let org = Arranger.AnOrganization()
    do! org |> (save connectDb)
    let newEmail = "new@example.com"

    let! response = putFormWithToken api (editUrl) (submitUrl) [("Email", newEmail)]

    let! updated = readDetailsBy connectDb org.Teczka
    updated.Kontakty.Email |> should equal newEmail
}

// BAD - Multiple unrelated concepts
[<Fact>]
let ``organization update works correctly`` () = task {
    // Tests authorization, validation, persistence, audit trail, and HTTP response
    // Split into separate tests!
}
```

---

## Assertion Best Practices

### Assert Specific Values

Always verify **exact, specific values** that match what your test name claims. Avoid loose assertions that could pass even when the behavior is wrong.

#### Good Assertions

```fsharp
// Specific value equality
response.StatusCode |> should equal HttpStatusCode.OK
inputs |> should equal [expectedValue1; expectedValue2]
updated.Kontakty.Email |> should equal "test@example.com"

// Specific collection membership
rows |> should contain "admin@example.com"
headers |> should supersetOf ["Mail"; "Rola"; "Akcje"]

// Specific ordering
firstDate |> should lessThan secondDate

// Specific count with meaning
auditEntries |> should haveLength 1  // Exactly one change recorded
```

#### Bad Assertions

```fsharp
// Too loose - doesn't verify the actual value
response.StatusCode |> should not' (equal HttpStatusCode.InternalServerError)
email |> should not' (be null)

// Doesn't match test name claim
// Test name: "returns organizations sorted by name"
// Assertion: just checks that some organizations were returned
organizations |> should not' (be Empty)

// Tests implementation, not behavior
// mockFunction |> should haveBeenCalled  // Don't do this in integration tests
```

### Match Test Name to Assertions

If your test name claims to verify something specific, your assertions must verify that exact thing:

```fsharp
// Test name claims: "returns organizations in alphabetical order"
[<Fact>]
let ``GET /organizations/summaries returns organizations in alphabetical order when sorted by name`` () =
    task {
        // Arrange
        let org1 = Arranger.AnOrganization() |> Arranger.setNazwaPlacowki "Zebra Charity"
        let org2 = Arranger.AnOrganization() |> Arranger.setNazwaPlacowki "Alpha Foundation"
        do! saveMany connectDb [org1; org2]

        // Act
        let! response = api.GetAsync "/organizations/summaries?sortBy=NazwaPlacowki&direction=asc"
        let! doc = response.HtmlContent()
        let names = doc.CssSelect ".organization-name" |> List.map _.InnerText()

        // Assert - Verify the specific ordering claimed in test name
        names |> should equal ["Alpha Foundation"; "Zebra Charity"]
        names.Head |> should equal "Alpha Foundation"  // Specific first item
    }
```

### FsUnit Cheat Sheet

Common assertion patterns used in this codebase:

```fsharp
// Equality
value |> should equal expected
value |> should not' (equal unexpected)

// Numeric comparisons
number |> should be (greaterThan 10)
number |> should be (lessThan 100)
number |> should be (greaterThanOrEqualTo 0)

// Collection assertions
list |> should haveLength 3
list |> should contain "item"
list |> should not' (be Empty)
list |> should supersetOf ["item1"; "item2"]  // Contains at least these items

// String assertions
text |> should haveSubstring "expected text"
text |> should startWith "prefix"
text |> should endWith "suffix"

// Boolean
condition |> should equal true
condition |> should be True  // Alternative syntax

// Option/Result types
option |> should equal (Some expected)
result |> should equal (Ok expected)
result |> should equal (Error "error message")

// HTTP Response
response.StatusCode |> should equal HttpStatusCode.OK
response.IsSuccessStatusCode |> should equal true
```

---

## Test Data Management

### The Arranger Pattern

We use a dedicated **Arranger** module to create test data. This provides:
- Type-safe domain objects
- Realistic fake data via Bogus
- Builder-style modifiers for customization
- Consistent test data across the suite

#### Basic Usage

```fsharp
// Create a fully populated organization with random data
let org = Arranger.AnOrganization()
do! org |> (save connectDb)

// Customize specific fields using modifier functions
let org = Arranger.AnOrganization()
          |> Arranger.setEmail "test@example.com"
          |> Arranger.setNazwaPlacowki "Test Charity"
          |> Arranger.setOstatnieOdwiedziny (DateOnly(2024, 1, 15))
do! org |> (save connectDb)
```

#### Creating New Arrangers

When testing a new domain, create an `Arranger.fs` file in the test module:

```fsharp
module YourFeature.Arranger

open Bogus
open Domain

let private f = Faker()

// Main builder - creates a valid domain object with random data
let AnEntity () : Entity =
    { Id = f.Random.Guid()
      Name = f.Company.CompanyName()
      Email = f.Internet.Email()
      CreatedAt = f.Date.Recent() }

// Modifier functions - customize specific fields
let setName (name: string) (entity: Entity) =
    { entity with Name = name }

let setEmail (email: string) (entity: Entity) =
    { entity with Email = email }

// Specialized builders for common scenarios
let AnActiveEntity () =
    AnEntity()
    |> fun e -> { e with IsActive = true }
```

#### Guidelines for Arrangers

1. **Use domain types, not DTOs**: Arrangers create domain entities, not read models or commands
2. **Always generate valid data**: Default objects should pass all domain validations
3. **Provide modifiers**: One function per field for customization
4. **Use Bogus for realism**: Leverage Bogus generators for realistic data (names, addresses, emails)
5. **Keep it focused**: One Arranger per aggregate root

---

## Testing with Real Infrastructure

### Testcontainers Setup

We use Testcontainers to run real PostgreSQL and Azurite (Azure Storage emulator) during tests. This provides:
- High confidence in database interactions
- Real SQL query testing
- Validation of migrations
- No mocking overhead

#### Container Configuration

Containers are configured once per test assembly in `Tools/Setup.fs`:

```fsharp
type Setup() =
    do
        let postgres =
            PostgreSqlBuilder()
                .WithImage("postgres:17")
                .WithReuse(true)  // Fast test execution
                .WithName("foodbank_db")
                .Build()

        let azurite =
            ContainerBuilder()
                .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
                .WithReuse(true)
                .Build()

        azurite.StartAsync() |> Async.AwaitTask |> Async.RunSynchronously
        postgres.StartAsync() |> Async.AwaitTask |> Async.RunSynchronously

        Migrations.main [||] |> ignore  // Run migrations automatically

[<assembly: AssemblyFixture(typeof<Setup>)>]
do ()
```

#### Container Reuse Strategy

`.WithReuse(true)` means:
- First test run starts containers
- Subsequent runs reuse existing containers
- Tests execute in seconds, not minutes
- Data is NOT automatically cleaned between runs (see Data Isolation below)

### Database Testing Patterns

#### Direct Database Access for Arrange

Set up test data directly against the database using domain functions:

```fsharp
[<Fact>]
let ``GET /organizations/{id} returns complete organization details`` () =
    task {
        // Arrange - Use domain function to save to real database
        let org = Arranger.AnOrganization()
                  |> Arranger.setEmail "test@charity.org"
        do! org |> (save Tools.DbConnection.connectDb)
        let teczkaId = org.Teczka |> TeczkaId.unwrap

        // Act - HTTP call hits real database
        let! response = api.GetAsync $"/organizations/{teczkaId}"

        // Assert
        let! doc = response.HtmlContent()
        doc.CssSelect ".email"
        |> List.head
        |> _.InnerText()
        |> should equal "test@charity.org"
    }
```

#### Verify Persistence in Assert

For write operations, verify data was actually saved:

```fsharp
[<Fact>]
let ``PUT /organizations/{id}/kontakty persists changes to database`` () =
    task {
        // Arrange
        let org = Arranger.AnOrganization()
        do! org |> (save connectDb)
        let newEmail = "updated@example.com"

        // Act
        let! response = putFormWithToken api editUrl submitUrl [("Email", newEmail)]

        // Assert - Verify HTTP response
        response.StatusCode |> should equal HttpStatusCode.OK

        // Assert - Verify database persistence
        let! updated = readDetailsBy connectDb org.Teczka
        updated.Kontakty.Email |> should equal newEmail
    }
```

### Data Isolation

Since containers are reused, **tests may see data from previous runs**. Handle this in two ways:

#### 1. Use Unique Identifiers

Generate unique IDs or names for test data:

```fsharp
let uniqueName = $"TestOrg_{Guid.NewGuid()}"
let org = Arranger.AnOrganization() |> Arranger.setNazwaPlacowki uniqueName
```

#### 2. Filter in Queries

When reading data, filter to only your test data:

```fsharp
let! summaries = readSummaries connectDb {
    SearchTerm = uniqueName  // Only get our test org
    SortBy = None
    Pagination = { Page = 1 }
    Filters = []
}
summaries.TotalCount |> should equal 1
```

---

## HTTP Testing Patterns

### WebApplicationFactory Setup

Tests use `Microsoft.AspNetCore.Mvc.Testing` to run the full application in-memory:

```fsharp
// Singleton to avoid creating too many file watchers
let private factory = lazy (new TestWebApplicationFactory())

let runTestApi () =
    let client = factory.Value.Server.CreateClient()
    client.DefaultRequestHeaders.Add("HX-Request", "true")  // Simulate HTMX requests
    client
```

### Authentication in Tests

Simulate authenticated users by setting role headers:

```fsharp
let authenticate (role: string) (client: HttpClient) =
    client.DefaultRequestHeaders.Add("role", role)
    client

// Usage
let api = runTestApi() |> authenticate "Editor"
```

### CSRF Token Handling

For POST/PUT operations, tests must include antiforgery tokens. Use the provided helpers:

```fsharp
// Helper automatically fetches token and includes it in form submission
let! response = postFormWithToken
    api
    "/organizations/1/kontakty/edit"  // URL to get form (for token)
    "/organizations/1/kontakty"        // URL to submit form
    [
        ("Email", "test@example.com")
        ("Telefon", "123456789")
    ]

// For PUT requests
let! response = putFormWithToken api editUrl submitUrl fields
```

### HTML Parsing and Assertions

Parse HTML responses using FSharp.Data:

```fsharp
// Get HTML document
let! doc = response.HtmlContent()

// Extract data with CSS selectors
let emails = doc.CssSelect ".contact-email" |> List.map _.InnerText()
let headers = doc.CssSelect "th" |> List.map _.InnerText()

// Assert on structure
headers |> should supersetOf ["Email"; "Telefon"; "Nazwa"]
emails |> should contain "test@example.com"

// Check for specific elements
let errorMessage = doc.CssSelect ".validation-error" |> List.tryHead
errorMessage |> should not' (equal None)
```

### Status Code Assertions

Always verify expected status codes:

```fsharp
// Success scenarios
response.StatusCode |> should equal HttpStatusCode.OK
response.StatusCode |> should equal HttpStatusCode.Created
response.IsSuccessStatusCode |> should equal true

// Error scenarios
response.StatusCode |> should equal HttpStatusCode.NotFound
response.StatusCode |> should equal HttpStatusCode.BadRequest
response.StatusCode |> should equal HttpStatusCode.Forbidden
```

---

## Testing HTMX Interactions

This application uses HTMX extensively for partial page updates. Test HTMX responses as integration tests to verify both the HTTP response and the HTML fragment returned.

### HTMX Request Simulation

Tests should simulate HTMX requests by adding the `HX-Request` header:

```fsharp
let runTestApi () =
    let client = factory.Value.Server.CreateClient()
    client.DefaultRequestHeaders.Add("HX-Request", "true")  // Simulate HTMX
    client
```

### Testing Section Edit/Update Flow

HTMX sections follow a three-endpoint pattern: view → edit form → update. Test all three:

```fsharp
[<Fact>]
let ``GET /organizations/{id}/kontakty returns section view`` () =
    task {
        // Arrange
        let org = Arranger.AnOrganization() |> Arranger.setEmail "test@example.com"
        do! org |> (save connectDb)
        let api = runTestApi() |> authenticate "Editor"
        let id = org.Teczka |> TeczkaId.unwrap

        // Act
        let! response = api.GetAsync $"/organizations/{id}/kontakty"

        // Assert - Verify HTML fragment contains expected data
        response.StatusCode |> should equal HttpStatusCode.OK
        let! doc = response.HtmlContent()
        let email = doc.CssSelect ".email" |> List.head |> _.InnerText()
        email |> should equal "test@example.com"
    }

[<Fact>]
let ``GET /organizations/{id}/kontakty/edit returns prefilled form`` () =
    task {
        // Arrange
        let org = Arranger.AnOrganization() |> Arranger.setEmail "existing@example.com"
        do! org |> (save connectDb)
        let api = runTestApi() |> authenticate "Editor"
        let id = org.Teczka |> TeczkaId.unwrap

        // Act
        let! response = api.GetAsync $"/organizations/{id}/kontakty/edit"

        // Assert - Verify form is prefilled with current values
        response.StatusCode |> should equal HttpStatusCode.OK
        let! doc = response.HtmlContent()
        let emailInput = doc.CssSelect "input[name='Email']" |> List.head
        emailInput.AttributeValue("value") |> should equal "existing@example.com"
    }

[<Fact>]
let ``PUT /organizations/{id}/kontakty updates and returns view fragment`` () =
    task {
        // Arrange
        let org = Arranger.AnOrganization()
        do! org |> (save connectDb)
        let api = runTestApi() |> authenticate "Editor"
        let id = org.Teczka |> TeczkaId.unwrap

        // Act - Submit update
        let! response = putFormWithToken api
            $"/organizations/{id}/kontakty/edit"
            $"/organizations/{id}/kontakty"
            [("Email", "updated@example.com"); ("Telefon", "123456789")]

        // Assert - Verify returns updated view fragment (not edit form)
        response.StatusCode |> should equal HttpStatusCode.OK
        let! doc = response.HtmlContent()

        // Should return view mode, not form inputs
        let forms = doc.CssSelect "form"
        forms |> should be Empty

        // Should display updated values
        let email = doc.CssSelect ".email" |> List.head |> _.InnerText()
        email |> should equal "updated@example.com"
    }
```

### Testing HTMX Response Headers

For advanced HTMX features, verify response headers:

```fsharp
[<Fact>]
let ``PUT /organizations/{id}/section returns HX-Trigger header when needed`` () =
    task {
        // Arrange
        let org = Arranger.AnOrganization()
        do! org |> (save connectDb)
        let api = runTestApi() |> authenticate "Editor"

        // Act
        let! response = putFormWithToken api editUrl submitUrl fields

        // Assert - Verify HTMX-specific headers
        response.Headers.TryGetValues("HX-Trigger") |> fst |> should equal true
        let trigger = response.Headers.GetValues("HX-Trigger") |> Seq.head
        trigger |> should equal "organizationUpdated"
    }
```

### Testing Out-of-Band Updates

If using HTMX out-of-band swaps (hx-swap-oob), verify multiple elements in response:

```fsharp
[<Fact>]
let ``PUT /organizations/{id}/section includes out-of-band updates`` () =
    task {
        // Arrange & Act
        let! response = putFormWithToken api editUrl submitUrl fields

        // Assert - Verify main content
        let! doc = response.HtmlContent()
        let mainSection = doc.CssSelect "#kontakty-section" |> List.head
        mainSection |> should not' (equal null)

        // Assert - Verify out-of-band update element
        let oobElement = doc.CssSelect "[hx-swap-oob]" |> List.tryHead
        oobElement |> should not' (equal None)
    }
```

### Testing Validation Errors with HTMX

Validation errors should return the form with error messages:

```fsharp
[<Fact>]
let ``PUT /organizations/{id}/kontakty returns form with errors when email invalid`` () =
    task {
        // Arrange
        let org = Arranger.AnOrganization()
        do! org |> (save connectDb)
        let api = runTestApi() |> authenticate "Editor"
        let id = org.Teczka |> TeczkaId.unwrap

        // Act - Submit invalid data
        let! response = putFormWithToken api
            $"/organizations/{id}/kontakty/edit"
            $"/organizations/{id}/kontakty"
            [("Email", "not-an-email"); ("Telefon", "123")]

        // Assert - Should return 400 with form containing errors
        response.StatusCode |> should equal HttpStatusCode.BadRequest
        let! doc = response.HtmlContent()

        // Should still be a form (not view mode)
        let form = doc.CssSelect "form" |> List.head
        form |> should not' (equal null)

        // Should have validation error messages
        let errors = doc.CssSelect ".validation-error"
        errors |> should not' (be Empty)
        errors |> List.exists (fun e -> e.InnerText().Contains("email")) |> should equal true
    }
```

---

## Testing Audit Trail

**Audit trail is observable behavior and must be tested.** Every write operation that modifies an organization should verify that the audit trail was correctly recorded.

### Why Test Audit Trail

Audit trail is not an implementation detail—it's a critical business requirement for:
- Compliance and accountability
- Tracking who changed what and when
- Debugging data issues
- Legal requirements

### Testing Pattern

For every PUT/POST operation, add an assertion that verifies the audit record:

```fsharp
[<Fact>]
let ``PUT /organizations/{id}/kontakty records change in audit trail`` () =
    task {
        // Arrange
        let org = Arranger.AnOrganization() |> Arranger.setEmail "old@example.com"
        do! org |> (save connectDb)
        let api = runTestApi() |> authenticate "Editor"
        let id = org.Teczka |> TeczkaId.unwrap

        // Act
        let! response = putFormWithToken api
            $"/organizations/{id}/kontakty/edit"
            $"/organizations/{id}/kontakty"
            [("Email", "new@example.com"); ("Telefon", "123456789")]

        // Assert - HTTP response
        response.StatusCode |> should equal HttpStatusCode.OK

        // Assert - Database persistence
        let! updated = readDetailsBy connectDb org.Teczka
        updated.Kontakty.Email |> should equal "new@example.com"

        // Assert - Audit trail
        let! auditEntries = readAuditTrail connectDb id (Some "Kontakty")
        auditEntries |> should haveLength 1

        let audit = auditEntries |> List.head
        audit.EntityId |> should equal id
        audit.Kind |> should equal "Kontakty"
        audit.ChangedBy |> should equal "Editor"

        // Verify field-level diff
        let emailDiff = audit.Diff |> Map.tryFind "Email"
        emailDiff |> should not' (equal None)
        emailDiff.Value.OldValue |> should equal "old@example.com"
        emailDiff.Value.NewValue |> should equal "new@example.com"
    }
```

### Testing Multiple Field Changes

Verify that all changed fields are captured in the audit diff:

```fsharp
[<Fact>]
let ``PUT /organizations/{id}/kontakty records all changed fields in diff`` () =
    task {
        // Arrange
        let org = Arranger.AnOrganization()
                  |> Arranger.setEmail "old@example.com"
        do! org |> (save connectDb)
        let api = runTestApi() |> authenticate "Admin"
        let id = org.Teczka |> TeczkaId.unwrap

        // Act - Change multiple fields
        let! response = putFormWithToken api editUrl submitUrl [
            ("Email", "new@example.com")
            ("Telefon", "999888777")
            ("NazwaOsobyKontaktowej", "Jan Kowalski")
        ]

        // Assert - Audit captures all changes
        let! auditEntries = readAuditTrail connectDb id (Some "Kontakty")
        let audit = auditEntries |> List.head

        audit.Diff |> Map.count |> should be (greaterThanOrEqualTo 3)
        audit.Diff |> Map.containsKey "Email" |> should equal true
        audit.Diff |> Map.containsKey "Telefon" |> should equal true
        audit.Diff |> Map.containsKey "NazwaOsobyKontaktowej" |> should equal true
    }
```

### Testing User Identity in Audit

Verify that the correct user is recorded as making the change:

```fsharp
[<Fact>]
let ``audit trail records Admin role when Admin makes change`` () =
    task {
        let org = Arranger.AnOrganization()
        do! org |> (save connectDb)
        let api = runTestApi() |> authenticate "Admin"
        let id = org.Teczka |> TeczkaId.unwrap

        let! response = putFormWithToken api editUrl submitUrl [("Email", "test@example.com")]

        let! auditEntries = readAuditTrail connectDb id None
        auditEntries.Head.ChangedBy |> should equal "Admin"
    }

[<Fact>]
let ``audit trail records Editor role when Editor makes change`` () =
    task {
        let org = Arranger.AnOrganization()
        do! org |> (save connectDb)
        let api = runTestApi() |> authenticate "Editor"
        let id = org.Teczka |> TeczkaId.unwrap

        let! response = putFormWithToken api editUrl submitUrl [("Email", "test@example.com")]

        let! auditEntries = readAuditTrail connectDb id None
        auditEntries.Head.ChangedBy |> should equal "Editor"
    }
```

---

## Testing Document Upload/Download

Document operations involve both Azure Blob Storage and database metadata. Test both aspects to ensure the complete workflow functions correctly.

### Recommended Testing Strategy

1. **Test metadata persistence** (database) for all operations
2. **Spot-check blob operations** (actual file upload/download) for critical paths
3. **Test SAS URI generation** to ensure secure access

### Testing Document Upload

Verify that document metadata is saved to the database and file is accessible:

```fsharp
[<Fact>]
let ``PUT /organizations/{id}/dokumenty uploads file and saves metadata`` () =
    task {
        // Arrange
        let org = Arranger.AnOrganization()
        do! org |> (save connectDb)
        let api = runTestApi() |> authenticate "Editor"
        let id = org.Teczka |> TeczkaId.unwrap

        let fileContent = "Test document content"B
        use fileStream = new MemoryStream(fileContent)
        use multipartContent = new MultipartFormDataContent()
        use fileStreamContent = new StreamContent(fileStream)
        fileStreamContent.Headers.ContentType <- MediaTypeHeaderValue("application/pdf")
        multipartContent.Add(fileStreamContent, "Umowa", "umowa.pdf")
        multipartContent.Add(new StringContent("2024-01-15"), "UmowaDate")

        // Act
        let! token = getAntiforgeryToken api $"/organizations/{id}/dokumenty/edit"
        multipartContent.Add(new StringContent(token), "__RequestVerificationToken")
        let! response = api.PutAsync($"/organizations/{id}/dokumenty", multipartContent)

        // Assert - HTTP response
        response.StatusCode |> should equal HttpStatusCode.OK

        // Assert - Database metadata
        let! updated = readDetailsBy connectDb org.Teczka
        updated.Dokumenty.Umowa.FileName |> should equal (Some "umowa.pdf")
        updated.Dokumenty.Umowa.Date |> should equal (Some (DateOnly(2024, 1, 15)))

        // Assert - File is accessible via generated URI
        let! doc = response.HtmlContent()
        let downloadLink = doc.CssSelect "a[href*='umowa']" |> List.tryHead
        downloadLink |> should not' (equal None)
    }
```

### Testing Document Download (SAS URI)

Verify that download links use SAS URIs with appropriate expiration:

```fsharp
[<Fact>]
let ``GET /organizations/{id}/dokumenty generates valid SAS URI for download`` () =
    task {
        // Arrange - Organization with existing document
        let org = Arranger.AnOrganization()
        let orgWithDoc = { org with
                            Dokumenty = { org.Dokumenty with
                                Umowa = { Date = Some (DateOnly(2024, 1, 1))
                                         FileName = Some "umowa.pdf" }}}
        do! orgWithDoc |> (save connectDb)

        // Upload actual file to blob storage
        let blobClient = getBlobClient "umowa.pdf" id
        do! blobClient.UploadAsync(new MemoryStream("content"B), overwrite = true)

        let api = runTestApi() |> authenticate "Editor"
        let id = org.Teczka |> TeczkaId.unwrap

        // Act
        let! response = api.GetAsync $"/organizations/{id}/dokumenty"

        // Assert
        let! doc = response.HtmlContent()
        let downloadLink = doc.CssSelect "a[href*='sig=']" |> List.head
        let href = downloadLink.AttributeValue("href")

        // Verify SAS URI format
        href |> should haveSubstring "sig="  // SAS token present
        href |> should haveSubstring "se="   // Expiration time present

        // Verify URI is actually accessible
        use httpClient = new HttpClient()
        let! downloadResponse = httpClient.GetAsync(href)
        downloadResponse.IsSuccessStatusCode |> should equal true
    }
```

### Testing Document Deletion

Verify that deleting a document removes both metadata and blob:

```fsharp
[<Fact>]
let ``PUT /organizations/{id}/dokumenty deletes document when checkbox marked`` () =
    task {
        // Arrange - Organization with document
        let org = Arranger.AnOrganization()
        let orgWithDoc = { org with
                            Dokumenty = { org.Dokumenty with
                                Wniosek = { Date = Some (DateOnly(2024, 1, 1))
                                           FileName = Some "wniosek.pdf" }}}
        do! orgWithDoc |> (save connectDb)

        let blobClient = getBlobClient "wniosek.pdf" id
        do! blobClient.UploadAsync(new MemoryStream("content"B), overwrite = true)

        let api = runTestApi() |> authenticate "Editor"
        let id = org.Teczka |> TeczkaId.unwrap

        // Act - Submit form with delete checkbox
        let! response = putFormWithToken api
            $"/organizations/{id}/dokumenty/edit"
            $"/organizations/{id}/dokumenty"
            [("DeleteWniosek", "true")]

        // Assert - Metadata removed from database
        let! updated = readDetailsBy connectDb org.Teczka
        updated.Dokumenty.Wniosek.FileName |> should equal None
        updated.Dokumenty.Wniosek.Date |> should equal None

        // Assert - Blob deleted from storage
        let! exists = blobClient.ExistsAsync()
        exists.Value |> should equal false
    }
```

### Testing Document Type Validation

Test that only valid document types are accepted:

```fsharp
[<Fact>]
let ``PUT /organizations/{id}/dokumenty accepts PDF files`` () = task { ... }

[<Fact>]
let ``PUT /organizations/{id}/dokumenty accepts Word documents`` () = task { ... }

[<Fact>]
let ``PUT /organizations/{id}/dokumenty rejects executable files`` () = task {
    // Security test - reject potentially harmful file types
    ...
}

[<Fact>]
let ``PUT /organizations/{id}/dokumenty rejects files exceeding size limit`` () = task { ... }
```

---

## Testing Authorization

**Every protected endpoint must be tested with each role (Admin, Editor, Reader)** to ensure authorization is correctly enforced.

### Authorization Testing Pattern

For each protected endpoint, create three tests:

```fsharp
// Admin role - full access
[<Fact>]
let ``PUT /organizations/{id}/kontakty allows Admin to update`` () =
    task {
        let org = Arranger.AnOrganization()
        do! org |> (save connectDb)
        let api = runTestApi() |> authenticate "Admin"
        let id = org.Teczka |> TeczkaId.unwrap

        let! response = putFormWithToken api
            $"/organizations/{id}/kontakty/edit"
            $"/organizations/{id}/kontakty"
            [("Email", "test@example.com")]

        response.StatusCode |> should equal HttpStatusCode.OK
    }

// Editor role - standard write access
[<Fact>]
let ``PUT /organizations/{id}/kontakty allows Editor to update`` () =
    task {
        let org = Arranger.AnOrganization()
        do! org |> (save connectDb)
        let api = runTestApi() |> authenticate "Editor"
        let id = org.Teczka |> TeczkaId.unwrap

        let! response = putFormWithToken api
            $"/organizations/{id}/kontakty/edit"
            $"/organizations/{id}/kontakty"
            [("Email", "test@example.com")]

        response.StatusCode |> should equal HttpStatusCode.OK
    }

// Reader role - read-only access
[<Fact>]
let ``PUT /organizations/{id}/kontakty forbids Reader from updating`` () =
    task {
        let org = Arranger.AnOrganization()
        do! org |> (save connectDb)
        let api = runTestApi() |> authenticate "Reader"
        let id = org.Teczka |> TeczkaId.unwrap

        let! response = putFormWithToken api
            $"/organizations/{id}/kontakty/edit"
            $"/organizations/{id}/kontakty"
            [("Email", "test@example.com")]

        response.StatusCode |> should equal HttpStatusCode.Forbidden
    }
```

### Testing Read Access

Verify that all roles can read data (unless specifically restricted):

```fsharp
[<Theory>]
[<InlineData("Admin")>]
[<InlineData("Editor")>]
[<InlineData("Reader")>]
let ``GET /organizations/{id} allows all authenticated roles`` (role: string) =
    task {
        let org = Arranger.AnOrganization()
        do! org |> (save connectDb)
        let api = runTestApi() |> authenticate role
        let id = org.Teczka |> TeczkaId.unwrap

        let! response = api.GetAsync $"/organizations/{id}"

        response.StatusCode |> should equal HttpStatusCode.OK
    }
```

### Testing Unauthenticated Access

Verify that unauthenticated requests are rejected:

```fsharp
[<Fact>]
let ``GET /organizations requires authentication`` () =
    task {
        let api = runTestApi()  // No authentication
        let! response = api.GetAsync "/organizations"

        response.StatusCode |> should equal HttpStatusCode.Unauthorized
    }

[<Fact>]
let ``PUT /organizations/{id}/kontakty requires authentication`` () =
    task {
        let api = runTestApi()  // No authentication
        let! response = api.PutAsync("/organizations/1/kontakty", null)

        response.StatusCode |> should equal HttpStatusCode.Unauthorized
    }
```

---

## Testing Excel Import (Partial Failures)

The Excel import feature must handle partial failures gracefully—some rows succeed while others fail validation. This is the most critical scenario to test.

### Testing Partial Import Success

```fsharp
[<Fact>]
let ``POST /organizations/import/upload imports valid rows and reports errors for invalid rows`` () =
    task {
        // Arrange - Create Excel with mix of valid and invalid data
        use workbook = new XLWorkbook()
        let worksheet = workbook.Worksheets.Add("Organizations")

        // Add headers (50 expected columns)
        worksheet.Cell(1, 1).Value <- "Teczka"
        worksheet.Cell(1, 2).Value <- "NIP"
        // ... all 50 headers

        // Row 2 - Valid organization
        worksheet.Cell(2, 1).Value <- "12345"
        worksheet.Cell(2, 2).Value <- "1234567890"  // Valid NIP
        // ... all valid data

        // Row 3 - Invalid NIP
        worksheet.Cell(3, 1).Value <- "12346"
        worksheet.Cell(3, 2).Value <- "INVALID"  // Invalid NIP format
        // ... rest of data

        // Row 4 - Valid organization
        worksheet.Cell(4, 1).Value <- "12347"
        worksheet.Cell(4, 2).Value <- "9876543210"  // Valid NIP
        // ... all valid data

        use stream = new MemoryStream()
        workbook.SaveAs(stream)
        stream.Position <- 0L

        let api = runTestApi() |> authenticate "Admin"
        use content = new MultipartFormDataContent()
        use fileContent = new StreamContent(stream)
        content.Add(fileContent, "file", "organizations.xlsx")

        // Act
        let! response = api.PostAsync("/organizations/import/upload", content)

        // Assert - HTTP response shows partial success
        response.StatusCode |> should equal HttpStatusCode.OK
        let! doc = response.HtmlContent()

        // Should report: 2 imported, 1 failed
        doc.InnerText() |> should haveSubstring "2 organizations imported"
        doc.InnerText() |> should haveSubstring "1 row failed"

        // Assert - Valid rows were saved to database
        let! org1 = readDetailsBy connectDb (TeczkaId.create 12345L |> getOrThrow)
        org1.NIP |> Nip.unwrap |> should equal "1234567890"

        let! org2 = readDetailsBy connectDb (TeczkaId.create 12347L |> getOrThrow)
        org2.NIP |> Nip.unwrap |> should equal "9876543210"

        // Assert - Invalid row was NOT saved
        let! invalidExists =
            try
                readDetailsBy connectDb (TeczkaId.create 12346L |> getOrThrow)
                |> Task.map (fun _ -> true)
            with
            | :? NotFoundException -> Task.FromResult false
        invalidExists |> should equal false

        // Assert - Error details are shown
        let errors = doc.CssSelect ".import-error"
        errors |> should haveLength 1
        errors.Head.InnerText() |> should haveSubstring "Row 3"
        errors.Head.InnerText() |> should haveSubstring "NIP"
    }
```

### Testing All Rows Invalid

```fsharp
[<Fact>]
let ``POST /organizations/import/upload imports nothing when all rows invalid`` () =
    task {
        // Arrange - Excel with all invalid rows
        use workbook = createExcelWithInvalidData()
        use stream = new MemoryStream()
        workbook.SaveAs(stream)
        stream.Position <- 0L

        let api = runTestApi() |> authenticate "Admin"
        use content = createMultipartContent stream "invalid.xlsx"

        // Count organizations before import
        let! beforeCount = countOrganizations connectDb

        // Act
        let! response = api.PostAsync("/organizations/import/upload", content)

        // Assert - No imports succeeded
        response.StatusCode |> should equal HttpStatusCode.OK
        let! doc = response.HtmlContent()
        doc.InnerText() |> should haveSubstring "0 organizations imported"
        doc.InnerText() |> should haveSubstring "5 rows failed"

        // Assert - Database unchanged
        let! afterCount = countOrganizations connectDb
        afterCount |> should equal beforeCount

        // Assert - All error details shown
        let errors = doc.CssSelect ".import-error"
        errors |> should haveLength 5
    }
```

### Testing Column Validation Errors

Test specific validation failures for different column types:

```fsharp
[<Fact>]
let ``POST /organizations/import/upload reports specific error for invalid NIP format`` () =
    task {
        use workbook = createExcelWithRow [
            ("Teczka", "12345")
            ("NIP", "123")  // Too short
            // ... other fields
        ]

        let! response = postImport api workbook

        let! doc = response.HtmlContent()
        let errors = doc.CssSelect ".import-error"
        errors.Head.InnerText() |> should haveSubstring "NIP must be exactly 10 digits"
    }

[<Fact>]
let ``POST /organizations/import/upload reports error for invalid email format`` () =
    task {
        use workbook = createExcelWithRow [
            ("Teczka", "12345")
            ("Email", "not-an-email")  // Invalid format
            // ... other fields
        ]

        let! response = postImport api workbook

        let! doc = response.HtmlContent()
        doc.InnerText() |> should haveSubstring "Invalid email format"
    }

[<Fact>]
let ``POST /organizations/import/upload reports error for duplicate Teczka ID`` () =
    task {
        // Arrange - Existing organization
        let existing = Arranger.AnOrganization()
        do! existing |> (save connectDb)
        let existingId = existing.Teczka |> TeczkaId.unwrap

        // Create Excel with duplicate ID
        use workbook = createExcelWithRow [
            ("Teczka", existingId.ToString())  // Duplicate!
            // ... other fields
        ]

        let! response = postImport api workbook

        let! doc = response.HtmlContent()
        doc.InnerText() |> should haveSubstring "Teczka ID already exists"
    }
```

### Testing Transaction Rollback

Verify that if the import operation fails catastrophically, no partial data is saved:

```fsharp
[<Fact>]
let ``POST /organizations/import/upload rolls back transaction on database error`` () =
    task {
        // This test would require injecting a failure in the middle of a batch save
        // Left as an exercise - requires more sophisticated test infrastructure
        ()
    }
```

---

## Testing Error Scenarios

**Test everything that is important.** This includes infrastructure failures, validation errors, business rule violations, and edge cases that could cause system instability.

### Categories of Error Scenarios to Test

#### 1. Validation Errors

```fsharp
[<Fact>]
let ``PUT /organizations/{id}/kontakty returns 400 when email format invalid`` () = task { ... }

[<Fact>]
let ``PUT /organizations/{id}/kontakty returns 400 when required field missing`` () = task { ... }

[<Fact>]
let ``POST /organizations/import/upload returns error when Excel headers missing`` () = task { ... }
```

#### 2. Business Rule Violations

```fsharp
[<Fact>]
let ``POST /organizations returns error when NIP already registered`` () = task { ... }

[<Fact>]
let ``PUT /organizations/{id}/dokumenty returns error when date is in future`` () = task { ... }

[<Fact>]
let ``PUT /organizations/{id}/warunki-pomocy returns error when HACCP date before audit date`` () = task { ... }
```

#### 3. Not Found Scenarios

```fsharp
[<Fact>]
let ``GET /organizations/{id} returns 404 when organization does not exist`` () =
    task {
        let api = runTestApi() |> authenticate "Editor"
        let nonExistentId = 999999999L

        let! response = api.GetAsync $"/organizations/{nonExistentId}"

        response.StatusCode |> should equal HttpStatusCode.NotFound
    }

[<Fact>]
let ``PUT /organizations/{id}/kontakty returns 404 when organization does not exist`` () = task { ... }
```

#### 4. Storage Failures

```fsharp
[<Fact>]
let ``PUT /organizations/{id}/dokumenty handles blob storage failure gracefully`` () =
    task {
        // This test requires mocking blob storage or causing a real failure
        // Could use a test double for BlobServiceClient that throws
        // Verify error message is user-friendly and transaction rolls back
        ()
    }

[<Fact>]
let ``GET /organizations/{id}/dokumenty generates download link even if blob temporarily unavailable`` () =
    task {
        // SAS URI generation should work even if blob service has issues
        // URI will fail at download time, not at link generation time
        ()
    }
```

#### 5. Database Constraint Violations

```fsharp
[<Fact>]
let ``PUT /organizations/{id} handles unique constraint violation`` () =
    task {
        // Try to update NIP to a value already used by another organization
        let org1 = Arranger.AnOrganization() |> Arranger.setNip "1234567890"
        let org2 = Arranger.AnOrganization() |> Arranger.setNip "9876543210"
        do! saveMany connectDb [org1; org2]

        let api = runTestApi() |> authenticate "Editor"
        let id = org2.Teczka |> TeczkaId.unwrap

        // Try to change org2's NIP to org1's NIP
        let! response = putFormWithToken api
            $"/organizations/{id}/dane-adresowe/edit"
            $"/organizations/{id}/dane-adresowe"
            [("NIP", "1234567890")]

        response.StatusCode |> should equal HttpStatusCode.BadRequest
        let! doc = response.HtmlContent()
        doc.InnerText() |> should haveSubstring "NIP already in use"
    }
```

#### 6. Transaction Rollback Scenarios

```fsharp
[<Fact>]
let ``PUT /organizations/{id}/kontakty rolls back database change when audit save fails`` () =
    task {
        // Requires dependency injection of failing audit DAO
        // Verify that neither organization update NOR audit entry are saved
        // Both operations should be in same transaction
        ()
    }
```

#### 7. Large Data Handling

```fsharp
[<Fact>]
let ``GET /organizations/summaries handles 1000+ organizations efficiently`` () =
    task {
        // Arrange - Create many organizations
        let organizations = [1..1000] |> List.map (fun _ -> Arranger.AnOrganization())
        do! saveMany connectDb organizations

        // Act
        let stopwatch = Stopwatch.StartNew()
        let! response = api.GetAsync "/organizations/summaries?page=1"
        stopwatch.Stop()

        // Assert - Pagination works and query is reasonably fast
        response.StatusCode |> should equal HttpStatusCode.OK
        stopwatch.ElapsedMilliseconds |> should be (lessThan 2000L)  // 2 seconds max

        let! doc = response.HtmlContent()
        let rows = doc.CssSelect ".organization-row"
        rows |> should haveLength 50  // Paginated to 50 per page
    }

[<Fact>]
let ``POST /organizations/import/upload handles large Excel file`` () =
    task {
        // Create Excel with 500 rows
        use workbook = createExcelWithRows 500

        let! response = postImport api workbook

        response.StatusCode |> should equal HttpStatusCode.OK
        // Should complete in reasonable time
    }
```

#### 8. Concurrent Modification (Optimistic Concurrency)

```fsharp
[<Fact>]
let ``PUT /organizations/{id}/kontakty handles concurrent updates with last-write-wins`` () =
    task {
        // Note: Current implementation uses last-write-wins
        // This test documents that behavior

        let org = Arranger.AnOrganization() |> Arranger.setEmail "original@example.com"
        do! org |> (save connectDb)
        let id = org.Teczka |> TeczkaId.unwrap

        // Two clients update simultaneously
        let api1 = runTestApi() |> authenticate "Editor"
        let api2 = runTestApi() |> authenticate "Editor"

        let! response1Task = putFormWithToken api1 editUrl submitUrl [("Email", "user1@example.com")] |> Async.AwaitTask |> Async.StartAsTask
        let! response2Task = putFormWithToken api2 editUrl submitUrl [("Email", "user2@example.com")] |> Async.AwaitTask |> Async.StartAsTask

        Task.WaitAll([| response1Task; response2Task |])

        // Both succeed (last-write-wins)
        response1Task.Result.StatusCode |> should equal HttpStatusCode.OK
        response2Task.Result.StatusCode |> should equal HttpStatusCode.OK

        // Last write wins
        let! final = readDetailsBy connectDb org.Teczka
        // Could be either email - depends on race condition
        final.Kontakty.Email |> should satisfy (fun e -> e = "user1@example.com" || e = "user2@example.com")
    }
```

---

## Testing REST APIs

For JSON REST API endpoints (e.g., `DataApi.fs`), use specialized helpers:

```fsharp
[<Fact>]
let ``GET /api/organizations/{id}/kontakty returns contact information as JSON`` () =
    task {
        // Arrange
        let org = Arranger.AnOrganization() |> Arranger.setEmail "api@test.com"
        do! org |> (save connectDb)
        let api = runTestApi()
        let teczkaId = org.Teczka |> TeczkaId.unwrap

        // Act - Deserialize JSON response to F# type
        let! kontakty = getAndDeserialize<KontaktyDto> api $"/api/organizations/{teczkaId}/kontakty"

        // Assert
        kontakty.Email |> should equal "api@test.com"
    }

[<Fact>]
let ``PUT /api/organizations/{id}/kontakty updates contact via JSON`` () =
    task {
        // Arrange
        let org = Arranger.AnOrganization()
        do! org |> (save connectDb)
        let api = runTestApi()
        let teczkaId = org.Teczka |> TeczkaId.unwrap

        let payload = {| Email = "new@test.com"; Telefon = "999888777" |}

        // Act - Serialize F# object to JSON and PUT
        do! putAndSerialize api $"/api/organizations/{teczkaId}/kontakty" payload "test@user.com"

        // Assert
        let! updated = readDetailsBy connectDb org.Teczka
        updated.Kontakty.Email |> should equal "new@test.com"
    }
```

---

## Testing Edge Cases Systematically

When implementing a feature, consider and test these edge cases by category:

### Numbers and Identifiers

```fsharp
// Zero and boundary values
[<Fact>]
let ``rejects TeczkaId when value is zero`` () = ...

[<Fact>]
let ``accepts TeczkaId at maximum Int64 value`` () = ...

// Invalid formats
[<Fact>]
let ``rejects NIP when not exactly 10 digits`` () = ...

[<Fact>]
let ``rejects Regon when format is invalid`` () = ...
```

### Strings and Text Fields

```fsharp
// Empty and whitespace
[<Fact>]
let ``rejects organization name when empty string`` () = ...

[<Fact>]
let ``rejects organization name when only whitespace`` () = ...

// Special characters
[<Fact>]
let ``accepts organization name with Polish characters`` () = ...

[<Fact>]
let ``accepts email with plus sign`` () = ...  // test+tag@example.com

// Length extremes
[<Fact>]
let ``accepts description up to 5000 characters`` () = ...

[<Fact>]
let ``rejects description exceeding maximum length`` () = ...
```

### Collections and Lists

```fsharp
// Empty collections
[<Fact>]
let ``returns empty array when no organizations match filter`` () = ...

// Duplicates
[<Fact>]
let ``prevents duplicate NIP registration`` () = ...

// Large collections
[<Fact>]
let ``paginates correctly when 100+ organizations exist`` () = ...
```

### Dates and Timestamps

```fsharp
// Boundary dates
[<Fact>]
let ``accepts visit date on leap day (Feb 29)`` () = ...

[<Fact>]
let ``correctly orders organizations by visit date across year boundary`` () = ...

// Invalid dates
[<Fact>]
let ``rejects visit date in the future`` () = ...

[<Fact>]
let ``rejects visit date before organization creation`` () = ...
```

### Domain-Specific Scenarios

```fsharp
// Email validation
[<Fact>]
let ``accepts valid international email addresses`` () = ...

[<Fact>]
let ``rejects email without @ symbol`` () = ...

// KRS number validation
[<Fact>]
let ``accepts 10-digit KRS number`` () = ...

[<Fact>]
let ``rejects KRS number with letters`` () = ...

// Form submissions
[<Fact>]
let ``rejects form submission without antiforgery token`` () = ...
```

### Authorization and Permissions

```fsharp
// Role-based access
[<Fact>]
let ``allows Editor to modify organization data`` () = ...

[<Fact>]
let ``forbids Reader from modifying organization data`` () = ...

[<Fact>]
let ``requires authentication for all organization endpoints`` () = ...
```

### Bug Clustering Pattern

When you find a bug, test related scenarios systematically. Bugs often cluster around the same misunderstanding:

```fsharp
// Bug found: NIP validation fails for valid numbers starting with 0

// Test the fix
[<Fact>]
let ``accepts NIP starting with zero`` () = ...

// Test related scenarios (bug clustering)
[<Fact>]
let ``accepts Regon starting with zero`` () = ...

[<Fact>]
let ``accepts KRS starting with zero`` () = ...

[<Fact>]
let ``preserves leading zeros when saving NIP`` () = ...

[<Fact>]
let ``displays leading zeros in NIP on organization details page`` () = ...
```

---

## Async Testing with Tasks

All tests in this codebase use F# **task** computation expressions, consistent with the application code.

### Basic Pattern

```fsharp
[<Fact>]
let ``test name`` () =
    task {
        // Arrange
        let! something = asyncOperation()

        // Act
        let! result = anotherAsyncOperation()

        // Assert
        result |> should equal expected
    }
```

### Combining Multiple Async Operations

```fsharp
[<Fact>]
let ``test with multiple database operations`` () =
    task {
        // Sequential async operations with do!
        let org1 = Arranger.AnOrganization()
        do! org1 |> (save connectDb)

        let org2 = Arranger.AnOrganization()
        do! org2 |> (save connectDb)

        // Async operation that returns a value
        let! response = api.GetAsync "/organizations/summaries"

        // Parse HTML asynchronously
        let! doc = response.HtmlContent()

        // Assert
        doc.CssSelect ".organization-row" |> should haveLength 2
    }
```

### Common Pitfalls

```fsharp
// WRONG - Forgetting to await async operation
let result = asyncOperation()  // Returns Task<T>, not T
result |> should equal expected  // Type error!

// CORRECT - Use let! to await
let! result = asyncOperation()
result |> should equal expected

// WRONG - Using do instead of do! for async side effects
let org = Arranger.AnOrganization()
do org |> (save connectDb)  // Doesn't actually wait for save!

// CORRECT - Use do! for async side effects
do! org |> (save connectDb)
```

---

## End-to-End Testing with Playwright

For critical user workflows that require JavaScript interaction or full browser rendering, use Playwright tests.

### When to Use E2E Tests

Use E2E tests for:
- Complex multi-step workflows (e.g., login → navigate → edit → save → verify)
- JavaScript-heavy interactions
- Visual regression testing
- Cross-browser compatibility

**Do NOT use E2E tests for:**
- Simple CRUD operations (use integration tests)
- Business logic validation (use integration tests)
- API testing (use REST API tests)

### Basic Playwright Test Structure

```fsharp
[<Fact>]
let ``user can log in and edit organization`` () =
    task {
        use! playwright = Playwright.CreateAsync()
        let! browser = playwright.Chromium.LaunchAsync()
        let! page = browser.NewPageAsync()

        // Navigate and interact
        do! page.GotoAsync("http://localhost:5000/login")
        do! page.FillAsync("#email", "admin@example.com")
        do! page.ClickAsync("button[type='submit']")

        // Wait for navigation
        do! page.WaitForURLAsync("**/organizations")

        // Assert on final state
        let! title = page.TitleAsync()
        title |> should equal "Organizations - Operator Portal"
    }
```

Keep E2E tests in the `E2E/` directory and run them separately from the main test suite due to their slower execution time.

---

## Test Helpers and Utilities

### Creating Reusable Helpers

When you find yourself repeating test setup code, extract it into `Tools/`:

```fsharp
// Tools/FormHelpers.fs
module Tests.Tools.FormHelpers

let submitContactForm (api: HttpClient) (teczkaId: int64) (email: string) (telefon: string) =
    putFormWithToken
        api
        $"/organizations/{teczkaId}/kontakty/edit"
        $"/organizations/{teczkaId}/kontakty"
        [
            ("Email", email)
            ("Telefon", telefon)
        ]
```

### FormData Builder

Use the FormData computation expression for complex forms:

```fsharp
open Tests.Tools.FormDataBuilder

let formContent = formData {
    "Email", "test@example.com"
    "Telefon", "123456789"
    "NazwaOsobyKontaktowej", "Jan Kowalski"
    if includeOptionalField then
        "Komentarz", "Some comment"
}

let! response = client.PostAsync(url, formContent)
```

---

## Learning Tests

The `Learning/` directory is for **exploratory tests** that:
- Spike external API integrations
- Experiment with new libraries
- Validate assumptions about framework behavior
- Document how third-party services work

These tests:
- Should NOT be included in CI pipelines
- May depend on external services
- May be slower or flaky
- Serve as executable documentation

Example structure:

```fsharp
// Learning/BlobStorage.fs
module Learning.BlobStorage

// These tests explore Azure Blob Storage behavior
// They require a real Azure Storage account or Azurite running

[<Fact>]
let ``can upload blob with SAS token`` () = task {
    // Exploratory test to understand SAS token permissions
    ...
}

[<Fact>]
let ``SAS token expires after configured duration`` () = task {
    // Learning test to verify token expiration behavior
    ...
}
```

---

## Common Patterns Summary

### Integration Test Template

```fsharp
module Feature.Behavior

open Xunit
open FsUnit.Xunit
open System.Net

[<Fact>]
let ``HTTP_METHOD /endpoint outcome when condition`` () =
    task {
        // Arrange - Set up test data
        let entity = Arranger.AnEntity() |> Arranger.customize
        do! entity |> (save Tools.DbConnection.connectDb)
        let api = runTestApi() |> authenticate "Editor"

        // Act - Execute HTTP request
        let! response = api.GetAsync "/endpoint"

        // Assert - Verify specific outcomes
        response.StatusCode |> should equal HttpStatusCode.OK
        let! doc = response.HtmlContent()
        let value = doc.CssSelect ".selector" |> List.head |> _.InnerText()
        value |> should equal "expected value"

        // Assert - Verify database state if needed
        let! persisted = readBy Tools.DbConnection.connectDb entity.Id
        persisted.Field |> should equal expectedValue
    }
```

### API Test Template

```fsharp
[<Fact>]
let ``HTTP_METHOD /api/endpoint outcome when condition`` () =
    task {
        // Arrange
        let entity = Arranger.AnEntity()
        do! entity |> (save connectDb)
        let api = runTestApi()

        // Act
        let! result = getAndDeserialize<DtoType> api $"/api/endpoint/{entity.Id}"

        // Assert
        result.Field |> should equal expectedValue
    }
```

---

## Checklist for New Tests

Before committing a new test, verify:

- [ ] Test name describes the **outcome**, not the action
- [ ] Test name includes the **condition** that triggers the outcome
- [ ] Test uses **Arrange-Act-Assert** structure with comments
- [ ] Assertions verify **specific values**, not just existence/type
- [ ] Assertions **match** what the test name claims
- [ ] Test verifies **one concept** (split if testing multiple unrelated behaviors)
- [ ] Test data uses **Arranger** pattern for consistency
- [ ] Async operations use `let!` or `do!`, not `let` or `do`
- [ ] Database state is verified for write operations
- [ ] HTTP status codes are explicitly asserted
- [ ] Test is in the correct module (Organizations, Login, etc.)
- [ ] Test runs successfully in isolation and with the full suite

---

## Anti-Patterns to Avoid

### Don't Test Implementation Details

```fsharp
// BAD - Tests that a function was called (implementation detail)
let ``save function is called when updating organization`` () = task {
    // Don't verify internal function calls
}

// GOOD - Tests that data was actually saved (observable behavior)
let ``PUT /organizations/{id}/kontakty persists changes to database`` () = task {
    // Verify the outcome by reading from the database
}
```

### Don't Use Vague Assertions

```fsharp
// BAD - Too vague
result |> should not' (be null)
organizations |> should not' (be Empty)

// GOOD - Specific expectations
result |> should equal expectedValue
organizations |> should haveLength 3
organizations.[0].Name |> should equal "Alpha Charity"
```

### Don't Test Multiple Unrelated Concepts

```fsharp
// BAD - Tests too many things
let ``organization CRUD operations work`` () = task {
    // Creates, reads, updates, deletes - split these!
}

// GOOD - One concept per test
let ``POST /organizations creates new organization`` () = task { ... }
let ``GET /organizations/{id} returns existing organization`` () = task { ... }
let ``PUT /organizations/{id} updates existing organization`` () = task { ... }
let ``DELETE /organizations/{id} removes organization`` () = task { ... }
```

### Don't Skip Arrange Phase

```fsharp
// BAD - Relies on data from previous tests or external state
let ``GET /organizations/1 returns organization`` () = task {
    // Assumes organization with ID 1 exists!
    let! response = api.GetAsync "/organizations/1"
    ...
}

// GOOD - Creates its own test data
let ``GET /organizations/{id} returns organization`` () = task {
    let org = Arranger.AnOrganization()
    do! org |> (save connectDb)
    let id = org.Teczka |> TeczkaId.unwrap
    let! response = api.GetAsync $"/organizations/{id}"
    ...
}
```

### Don't Ignore Status Codes

```fsharp
// BAD - Doesn't verify response succeeded
let ``GET /organizations/{id} returns organization details`` () = task {
    let! response = api.GetAsync $"/organizations/{id}"
    let! doc = response.HtmlContent()
    // If response was 404 or 500, this test might still pass!
}

// GOOD - Always assert status code
let ``GET /organizations/{id} returns organization details`` () = task {
    let! response = api.GetAsync $"/organizations/{id}"
    response.StatusCode |> should equal HttpStatusCode.OK
    let! doc = response.HtmlContent()
    ...
}
```

---

## Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [FsUnit Guide](https://fsprojects.github.io/FsUnit/)
- [Bogus Fake Data](https://github.com/bchavez/Bogus)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [F# Task Computation Expression](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/task-expressions)
