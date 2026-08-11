# Organization Event Sourcing Migration - Implementation Tasks

**Document Version:** 3.0
**Created:** 2026-01-04
**Last Updated:** 2026-01-05
**Status:** Phase 2 Complete - Phase 3 Ready
**Related PRD:** [prd-organization-event-sourcing.md](prd-organization-event-sourcing.md)
**Architecture Guide:** [architecture/event-sourcing.md](architecture/event-sourcing.md)

**Note:** Completed phases (1, 1.5, 2) are archived in [archive-tasks.md](archive-tasks.md)

---

## 📁 Archive

Completed phases (Phase 1: EventStore Infrastructure, Phase 1.5: EventSourcing Patterns, Phase 2: Domain Modeling) have been moved to [archive-tasks.md](archive-tasks.md) for reference.

**Current Focus:** Phase 3 - Projections & Dual-Write

---

## Overview

This document provides a comprehensive, phase-by-phase task breakdown for migrating the Organization vertical slice to event sourcing. The migration follows a **7-phase approach** emphasizing:

- **Test-Driven Development (TDD):** All implementation follows test-first approach
- **100% Test Coverage:** Mandatory coverage gates at end of each phase
- **Shared Infrastructure:** Reusable EventStore (Phase 1) and EventSourcing patterns (Phase 1.5) for all vertical slices
- **Generic Patterns First:** Extract reusable patterns before domain-specific implementation
- **Gradual Rollout:** Feature-flagged deployment with 4-week rollout plan
- **Zero Downtime:** Legacy and ES handlers coexist during migration

---

## Success Criteria Summary

| Phase | Success Criteria |
|-------|------------------|
| **1: EventStore Infrastructure** | ✅ EventStore package created in `Web/EventStore/`<br>✅ Events table migration added to Migrations project<br>✅ TDD: All tests written before implementation<br>✅ **100% test coverage for EventStore package** |
| **1.5: EventSourcing Patterns** | ✅ EventSourcing package created in `Web/EventSourcing/`<br>✅ Generic CommandHandler for load→decide→append flow<br>✅ Feature flag for percentage-based rollout<br>✅ Projection composition helpers<br>✅ **100% test coverage for all generic patterns** |
| **2: Domain Modeling** | ✅ Event types, commands, aggregates defined<br>✅ Pure functions (decide, evolve, replay) implemented<br>✅ AggregateIdMapping for TeczkaId → Guid conversion<br>✅ SyntheticEvents for migration<br>✅ **100% test coverage for all domain logic** |
| **3: Projections & Dual-Write** | ✅ Projections update organizacje table correctly<br>✅ Synthetic OrganizationCreated events work<br>✅ ES handlers use shared CommandHandler<br>✅ **100% test coverage for projections and handlers** |
| **4: Feature Flag & Integration** | ✅ Feature flag switches between ES/legacy correctly (using shared module)<br>✅ Audit trail adapter maintains compatibility<br>✅ CompositionRoot conditionally wires handlers<br>✅ **100% test coverage for integration logic** |
| **5: Observability & Rollout** | ✅ Logging and metrics instrumentation added<br>✅ Gradual rollout: 0% → 10% → 50% → 100%<br>✅ No performance degradation >20%<br>✅ **100% monitoring coverage (all operations logged)** |
| **6: Cleanup & Documentation** | ✅ Legacy code removed (Handlers.fs, FindDiffForAudit.fs)<br>✅ Feature flag removed (ES is default)<br>✅ Documentation complete (architecture, event catalog, time-traveling)<br>✅ **100% test coverage maintained after cleanup** |

---

## Phase Status Overview

| Phase | Status | Completion Date | Notes |
|-------|--------|----------------|-------|
| **Phase 1-2** | ✅ **ARCHIVED** | See archive-tasks.md | All phases complete, moved to archive |
| **Phase 3: Projections & Dual-Write** | 🚀 **READY** | - | Ready to begin. Phase 2 dependencies satisfied. |
| **Phase 4: Feature Flag & Integration** | ⏸️ **BLOCKED** | - | Blocked by Phase 3 completion. |
| **Phase 5: Observability & Rollout** | ⏸️ **BLOCKED** | - | Blocked by Phase 4 completion. |
| **Phase 6: Cleanup & Documentation** | ⏸️ **BLOCKED** | - | Blocked by Phase 5 completion. |

---

## PHASE 3: Projections & Dual-Write (**UPDATED**)

**Goal:** Implement projections and ES handlers with 100% test coverage

**Duration Estimate:** 1-2 weeks
**Tasks:** 9

### Phase Overview

Create projection functions that update the `organizacje` table from events. Implement event-sourced handlers using the **shared CommandHandler** from Phase 1.5. Keep legacy handlers intact (dual-write phase).

**Changes from Original Plan:**
- ✅ Projections.fs - unchanged (SQL updates only)
- ✅ EventSourcedHandlers.fs - **SIMPLIFIED** using EventSourcing.CommandHandler
- ❌ Remove custom handleCommand implementation (use shared module)

**Key Simplification:**
Each handler becomes ~10 lines of configuration + 1 line calling shared CommandHandler, instead of 50+ lines of orchestration logic.

**File Structure:**
```
Web/Organizations/EventSourcing/
├── Projections.fs              # Project events to organizacje table (SQL updates only)
└── EventSourcedHandlers.fs     # ES handlers using shared CommandHandler pattern
```

### Tasks

#### 3.1: TDD - Write integration tests for projection functions
**Type:** Test
**Approach:** Test-First (TDD)

**Create test file:** `/OperatorPortal/Tests/Organizations/EventSourcing/ProjectionsTests.fs`

**Test Cases:**
1. `OrganizationCreated event inserts row into organizacje table`
2. `KontaktyChanged event updates Kontakty columns`
3. `Projections are idempotent (replaying same event = same DB state)`
4. All 6 change events update correct columns
5. `Projection executes within transaction`

**Example Test:**
```fsharp
[<Fact>]
let ``KontaktyChanged event updates organizacje table`` () =
    async {
        // Arrange
        use! db = setupTestDb()
        let teczkaId = 123L
        // Insert initial organization
        do! insertOrganization db teczkaId

        let event = KontaktyChanged {
            TeczkaId = teczkaId
            Email = "updated@example.com"
            Telefon = "999888777"
            // ... all fields
            Audit = { Who = "user"; OccurredAt = DateTime.UtcNow }
        }

        // Act
        do! Projections.project event db

        // Assert
        let! org = db.Single<OrganizationRow>(
            "SELECT * FROM organizacje WHERE teczka = @id",
            {| id = teczkaId |}
        )
        org.Email |> should equal "updated@example.com"
        org.Telefon |> should equal "999888777"
    } |> Async.RunSynchronously
```

**Acceptance Criteria:**
- Tests compile but fail (projections not implemented)
- All 6 event types tested
- Idempotency tested (replay same event twice)

---

#### 3.2: Implement Projections.fs with 6 projection functions
**Type:** Implementation
**Approach:** Test-Driven (make tests from 3.1 pass)

**Create `Projections.fs`:**
```fsharp
module Organizations.EventSourcing.Projections

open PostgresPersistence.DapperFsharp
open Organizations.EventSourcing.Events

let private executeUpdate (sql: string) (parameters: obj) (db: IDbConnection) : Async<unit> =
    async {
        do! db.Execute sql parameters
    }

let private projectOrganizationCreated (event: OrganizationCreatedV1) (db: IDbConnection) : Async<unit> =
    async {
        do! executeUpdate
            """INSERT INTO organizacje (
                Teczka, IdentyfikatorEnova, NIP, Regon, KrsNr, FormaPrawna, OPP,
                -- DaneAdresowe columns
                NazwaOrganizacjiPodpisujacejUmowe, AdresRejestrowy,
                NazwaPlacowkiTrafiaZywnosc, AdresPlacowkiTrafiaZywnosc,
                GminaDzielnica, Powiat,
                -- Kontakty columns
                Email, Telefon, Przedstawiciel, Kontakt, Dostepnosc,
                OsobaDoKontaktu, TelefonOsobyKontaktowej, MailOsobyKontaktowej,
                OsobaOdbierajacaZywnosc, TelefonOsobyOdbierajacej, WwwFacebook,
                -- ZrodlaZywnosci columns
                Sieci, Bazarki, Machfit, FEPZ2024, OdbiorKrotkiTermin, TylkoNaszMagazyn,
                -- AdresyKsiegowosci columns
                NazwaOrganizacjiKsiegowanieDarowizn, KsiegowanieAdres, TelOrganProwadzacegoKsiegowosc,
                -- Beneficjenci columns
                LiczbaBeneficjentow, Beneficjenci,
                -- WarunkiPomocy columns
                Kategoria, RodzajPomocy, SposobUdzielaniaPomocy, WarunkiMagazynowe,
                HACCP, Sanepid, TransportOpis, TransportKategoria
               ) VALUES (
                @Teczka, @IdentyfikatorEnova, @NIP, @Regon, @KrsNr, @FormaPrawna, @OPP,
                -- Map all fields from event to parameters
                @NazwaOrganizacjiPodpisujacejUmowe, @AdresRejestrowy,
                (* ... all parameters ... *)
               )"""
            {| Teczka = event.TeczkaId
               IdentyfikatorEnova = event.IdentyfikatorEnova
               NIP = event.NIP
               Regon = event.Regon
               KrsNr = event.KrsNr
               FormaPrawna = event.FormaPrawna
               OPP = event.OPP
               // DaneAdresowe
               NazwaOrganizacjiPodpisujacejUmowe = event.DaneAdresowe.NazwaOrganizacjiPodpisujacejUmowe
               // ... map all fields ...
            |}
            db
    }

let private projectKontaktyChanged (event: KontaktyChangedV1) (db: IDbConnection) : Async<unit> =
    async {
        do! executeUpdate
            """UPDATE organizacje SET
                Email = @Email,
                Telefon = @Telefon,
                Przedstawiciel = @Przedstawiciel,
                Kontakt = @Kontakt,
                Dostepnosc = @Dostepnosc,
                OsobaDoKontaktu = @OsobaDoKontaktu,
                TelefonOsobyKontaktowej = @TelefonOsobyKontaktowej,
                MailOsobyKontaktowej = @MailOsobyKontaktowej,
                OsobaOdbierajacaZywnosc = @OsobaOdbierajacaZywnosc,
                TelefonOsobyOdbierajacej = @TelefonOsobyOdbierajacej,
                WwwFacebook = @WwwFacebook
               WHERE Teczka = @TeczkaId"""
            {| TeczkaId = event.TeczkaId
               Email = event.Email
               Telefon = event.Telefon
               Przedstawiciel = event.Przedstawiciel
               Kontakt = event.Kontakt
               Dostepnosc = event.Dostepnosc
               OsobaDoKontaktu = event.OsobaDoKontaktu
               TelefonOsobyKontaktowej = event.TelefonOsobyKontaktowej
               MailOsobyKontaktowej = event.MailOsobyKontaktowej
               OsobaOdbierajacaZywnosc = event.OsobaOdbierajacaZywnosc
               TelefonOsobyOdbierajacej = event.TelefonOsobyOdbierajacej
               WwwFacebook = event.WwwFacebook
            |}
            db
    }

// Similar functions for other 5 event types...

let private projectZrodlaZywnosciChanged (event: ZrodlaZywnosciChangedV1) (db: IDbConnection) : Async<unit> =
    // UPDATE organizacje SET Sieci = @Sieci, Bazarki = @Bazarki, ...
    failwith "Not implemented"

let private projectAdresyKsiegowosciChanged (event: AdresyKsiegowosciChangedV1) (db: IDbConnection) : Async<unit> =
    failwith "Not implemented"

let private projectDaneAdresoweChanged (event: DaneAdresoweChangedV1) (db: IDbConnection) : Async<unit> =
    failwith "Not implemented"

let private projectBeneficjenciChanged (event: BeneficjenciChangedV1) (db: IDbConnection) : Async<unit> =
    failwith "Not implemented"

let private projectWarunkiPomocyChanged (event: WarunkiPomocyChangedV1) (db: IDbConnection) : Async<unit> =
    failwith "Not implemented"
```

**Acceptance Criteria:**
- All tests from 3.1 pass
- Each projection function updates only relevant columns
- Projections use existing `organizacje` table schema
- No business logic in projections (just data mapping)

---

#### 3.3: Implement main project function with pattern matching
**Type:** Implementation
**Approach:** Code

**Add to `Projections.fs`:**
```fsharp
// Main projection dispatcher
let project (event: OrganizationEvent) (db: IDbConnection) : Async<unit> =
    match event with
    | OrganizationCreated e -> projectOrganizationCreated e db
    | KontaktyChanged e -> projectKontaktyChanged e db
    | ZrodlaZywnosciChanged e -> projectZrodlaZywnosciChanged e db
    | AdresyKsiegowosciChanged e -> projectAdresyKsiegowosciChanged e db
    | DaneAdresoweChanged e -> projectDaneAdresoweChanged e db
    | BeneficjenciChanged e -> projectBeneficjenciChanged e db
    | WarunkiPomocyChanged e -> projectWarunkiPomocyChanged e db
```

**Acceptance Criteria:**
- Pattern matching routes events to correct projection
- Compiler enforces exhaustive handling
- Projection passed IDbConnection for transaction participation

---

#### 3.4: TDD - Write integration tests for synthetic OrganizationCreatedV1 event
**Type:** Test
**Approach:** Test-First (TDD)

**Add to `ProjectionsTests.fs` or new file `SyntheticEventTests.fs`:**

**Test Cases:**
1. `Creating synthetic event from existing organization in DB`
2. `Synthetic event captures all organization data`
3. `Synthetic event version is 1`
4. `First edit after migration creates synthetic event + actual event`

**Example Test:**
```fsharp
[<Fact>]
let ``First edit creates synthetic OrganizationCreated event`` () =
    async {
        // Arrange
        use! db = setupTestDb()
        let teczkaId = 123L
        // Insert organization directly to DB (legacy way)
        do! insertOrganizationDirectly db teczkaId

        // Act - First ES edit (no events exist yet)
        let! currentVersion = EventStore.getCurrentVersion connectDb "Organization" (Guid.Parse(...))
        // currentVersion should be 0 (no events)

        // Generate synthetic event from DB state
        let! org = OrganizationsDao.readBy connectDb (TeczkaId.create teczkaId)
        let syntheticEvent = createSyntheticEvent org audit

        // Assert
        match syntheticEvent with
        | OrganizationCreated e ->
            e.TeczkaId |> should equal teczkaId
            e.Kontakty.Email |> should equal org.Kontakty.Email
            // ... verify all fields captured
        | _ -> Assert.Fail("Expected OrganizationCreated event")
    } |> Async.RunSynchronously
```

**Acceptance Criteria:**
- Tests compile but fail (synthetic event logic not implemented)
- Tests verify complete data capture from DB

---

#### 3.5: Implement synthetic event creation logic
**Type:** Implementation
**Approach:** Test-Driven (make tests from 3.4 pass)

**Add to `EventSourcedHandlers.fs` or new `SyntheticEvents.fs`:**
```fsharp
module Organizations.EventSourcing.SyntheticEvents

open Organizations.Domain
open Organizations.EventSourcing.Events

let createSyntheticEvent (org: Organization) (audit: EventAudit) : OrganizationEvent =
    OrganizationCreated {
        TeczkaId = TeczkaId.unwrap org.Teczka
        IdentyfikatorEnova = org.IdentyfikatorEnova
        NIP = Nip.unwrap org.NIP
        Regon = Regon.unwrap org.Regon
        KrsNr = "" // Extract from FormaPrawna
        FormaPrawna = "" // Convert FormaPrawna to string
        OPP = org.OPP
        DaneAdresowe = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            NazwaOrganizacjiPodpisujacejUmowe = org.DaneAdresowe.NazwaOrganizacjiPodpisujacejUmowe
            AdresRejestrowy = org.DaneAdresowe.AdresRejestrowy
            NazwaPlacowkiTrafiaZywnosc = org.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc
            AdresPlacowkiTrafiaZywnosc = org.DaneAdresowe.AdresPlacowkiTrafiaZywnosc
            GminaDzielnica = org.DaneAdresowe.GminaDzielnica
            Powiat = org.DaneAdresowe.Powiat
            Audit = audit
        }
        Kontakty = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            Email = org.Kontakty.Email
            Telefon = org.Kontakty.Telefon
            OsobaDoKontaktu = org.Kontakty.OsobaDoKontaktu
            TelefonOsobyKontaktowej = org.Kontakty.TelefonOsobyKontaktowej
            MailOsobyKontaktowej = org.Kontakty.MailOsobyKontaktowej
            OsobaOdbierajacaZywnosc = org.Kontakty.OsobaOdbierajacaZywnosc
            TelefonOsobyOdbierajacej = org.Kontakty.TelefonOsobyOdbierajacej
            Kontakt = org.Kontakty.Kontakt
            Przedstawiciel = org.Kontakty.Przedstawiciel
            Dostepnosc = org.Kontakty.Dostepnosc
            WwwFacebook = org.Kontakty.WwwFacebook
            Audit = audit
        }
        ZrodlaZywnosci = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            Sieci = org.ZrodlaZywnosci.Sieci
            Bazarki = org.ZrodlaZywnosci.Bazarki
            Machfit = org.ZrodlaZywnosci.Machfit
            FEPZ2024 = org.ZrodlaZywnosci.FEPZ2024
            OdbiorKrotkiTermin = org.ZrodlaZywnosci.OdbiorKrotkiTermin
            TylkoNaszMagazyn = org.ZrodlaZywnosci.TylkoNaszMagazyn
            Audit = audit
        }
        AdresyKsiegowosci = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            KsiegowanieAdres = org.AdresyKsiegowosci.KsiegowanieAdres
            NazwaOrganizacjiKsiegowanieDarowizn = org.AdresyKsiegowosci.NazwaOrganizacjiKsiegowanieDarowizn
            TelOrganProwadzacegoKsiegowosc = org.AdresyKsiegowosci.TelOrganProwadzacegoKsiegowosc
            Audit = audit
        }
        Beneficjenci = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            LiczbaBeneficjentow = org.Beneficjenci.LiczbaBeneficjentow
            Beneficjenci = org.Beneficjenci.Beneficjenci
            Audit = audit
        }
        WarunkiPomocy = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            Kategoria = org.WarunkiPomocy.Kategoria
            RodzajPomocy = org.WarunkiPomocy.RodzajPomocy
            SposobUdzielaniaPomocy = org.WarunkiPomocy.SposobUdzielaniaPomocy
            WarunkiMagazynowe = org.WarunkiPomocy.WarunkiMagazynowe
            HACCP = org.WarunkiPomocy.HACCP
            Sanepid = org.WarunkiPomocy.Sanepid
            TransportOpis = org.WarunkiPomocy.TransportOpis
            TransportKategoria = org.WarunkiPomocy.TransportKategoria
            Audit = audit
        }
        Audit = audit
    }
```

**Acceptance Criteria:**
- All tests from 3.4 pass
- Synthetic event captures complete organization state
- Maps domain types to event primitive types

---

#### 3.6: TDD - Write integration tests for EventSourcedHandlers
**Type:** Test
**Approach:** Test-First (TDD)

**Create test file:** `/OperatorPortal/Tests/Organizations/EventSourcing/EventSourcedHandlersTests.fs`

**Test Cases:**
1. `changeKontakty on existing organization appends KontaktyChanged event`
2. `changeKontakty on organization without events creates synthetic event first`
3. `changeKontakty updates organizacje table via projection`
4. `changeKontakty handles concurrency conflict`
5. All 6 handlers end-to-end (command → events → projection → DB)

**Example Test:**
```fsharp
[<Fact>]
let ``changeKontakty appends event and updates DB`` () =
    async {
        // Arrange
        use! db = setupTestDb()
        let teczkaId = TeczkaId.create 123L
        // Create organization with initial event
        do! insertOrganizationWithEvents db teczkaId

        let command = {
            TeczkaId = teczkaId
            Kontakty = {
                Email = "new@example.com"
                Telefon = "111222333"
                // ... all fields
            }
        }
        let audit = { Who = "user@test.com"; OccuredAt = DateTime.UtcNow }

        // Act
        let! result = EventSourcedHandlers.changeKontakty
            connectDb (teczkaId, audit, command)

        // Assert
        result |> should be (ofCase <@ Ok @>)

        // Verify event appended
        let! events = EventStore.loadEvents connectDb "Organization" (teczkaIdToGuid teczkaId)
        events |> should haveLength 2

        // Verify DB updated
        let! org = db.Single<OrganizationRow>(
            "SELECT * FROM organizacje WHERE teczka = @id",
            {| id = TeczkaId.unwrap teczkaId |}
        )
        org.Email |> should equal "new@example.com"
    } |> Async.RunSynchronously
```

**Acceptance Criteria:**
- Tests compile but fail (handlers not implemented)
- End-to-end flow tested (command → event → DB)
- Concurrency conflict scenarios tested

---

#### 3.7: Implement EventSourcedHandlers.fs
**Type:** Implementation
**Approach:** Test-Driven (make tests from 3.6 pass)

**Create `EventSourcedHandlers.fs`:**
```fsharp
module Organizations.EventSourcing.EventSourcedHandlers

open Organizations.Domain
open Organizations.EventSourcing.Commands
open Organizations.EventSourcing.Events
open Organizations.EventSourcing.Decision
open Organizations.EventSourcing.Evolution
open Organizations.EventSourcing.Projections
open Organizations.Application
open EventStore.Core

// Helper to convert TeczkaId to Guid for event store
let private teczkaIdToGuid (teczkaId: TeczkaId) : Guid =
    // Use consistent hashing or mapping
    // For simplicity, could use TeczkaId as part of UUID
    failwith "Implement TeczkaId → Guid mapping"

// Generic command handler
let private handleCommand
    (connectDb: unit -> Async<IDbConnection>)
    (teczkaId: TeczkaId)
    (audit: Audit)
    (buildCommand: OrganizationCommand)
    : Async<Result<unit, TeczkaIdError>> =
    asyncResult {
        let aggregateId = teczkaIdToGuid teczkaId
        let eventAudit = { Who = audit.Who; OccurredAt = audit.OccuredAt }

        // 1. Load events
        let! events = EventStore.loadEvents connectDb "Organization" aggregateId

        // 2. Check if organization has events (or need synthetic event)
        let! currentState =
            if List.isEmpty events then
                // No events - check if organization exists in DB
                async {
                    try
                        let! org = OrganizationsDao.readBy connectDb teczkaId
                        // Create synthetic event
                        let syntheticEvent = SyntheticEvents.createSyntheticEvent org eventAudit
                        return Some (evolve None syntheticEvent)
                    with
                    | _ -> return None
                }
            else
                // Replay events
                async { return replay events }

        // 3. Get current version
        let currentVersion =
            currentState
            |> Option.map (fun s -> s.Version)
            |> Option.defaultValue 0

        // 4. Decide what events to produce
        let! newEvents =
            match decide buildCommand currentState eventAudit with
            | Ok events -> Ok events
            | Error msg -> Error (TeczkaIdError.InvalidOperation msg)

        // 5. Append events with inline projections
        let! appendResult =
            EventStore.appendEvents
                connectDb
                "Organization"
                aggregateId
                currentVersion
                newEvents
                (fun event db -> Projections.project event db)

        match appendResult with
        | Ok () -> return ()
        | Error msg -> return! Error (TeczkaIdError.InvalidOperation msg)
    }

// Public API - 6 handlers matching existing signatures
type ChangeKontakty =
    Command<Commands.TeczkaId, Commands.Kontakty> -> Async<Result<unit, TeczkaIdError>>

let changeKontakty
    (connectDb: unit -> Async<IDbConnection>)
    : ChangeKontakty =
    fun (teczkaIdInt, audit, kontaktyDto) ->
        asyncResult {
            let! teczkaId = TeczkaId.create teczkaIdInt
            let command = ChangeKontakty {
                TeczkaId = teczkaId
                Kontakty = {
                    Email = kontaktyDto.Email
                    Telefon = kontaktyDto.Telefon
                    // ... map all fields from DTO
                }
            }
            return! handleCommand connectDb teczkaId audit command
        }

// Similar for other 5 handlers...

type ChangeZrodlaZywnosci =
    Command<Commands.TeczkaId, Commands.ZrodlaZywnosci> -> Async<Result<unit, TeczkaIdError>>

let changeZrodlaZywnosci
    (connectDb: unit -> Async<IDbConnection>)
    : ChangeZrodlaZywnosci =
    fun (teczkaIdInt, audit, dto) ->
        asyncResult {
            let! teczkaId = TeczkaId.create teczkaIdInt
            let command = ChangeZrodlaZywnosci {
                TeczkaId = teczkaId
                ZrodlaZywnosci = { (* map fields *) }
            }
            return! handleCommand connectDb teczkaId audit command
        }

// ... changeAdresyKsiegowosci, changeDaneAdresowe,
//     changeBeneficjenci, changeWarunkiPomocy
```

**Acceptance Criteria:**
- All tests from 3.6 pass
- Handlers mirror existing signatures (`Command<TeczkaId, Payload> -> Async<Result<unit, TeczkaIdError>>`)
- Generic handleCommand reduces duplication
- Synthetic event created on first edit for existing organizations
- Events + projections execute in single transaction

---

#### 3.8: TDD - Write integration tests for projection idempotency
**Type:** Test
**Approach:** Test-First (TDD)

**Add to `ProjectionsTests.fs`:**

**Test Cases:**
1. `Replaying same event twice produces same DB state`
2. `Replaying all events rebuilds correct state`

**Example Test:**
```fsharp
[<Fact>]
let ``Projections are idempotent`` () =
    async {
        // Arrange
        use! db = setupTestDb()
        let teczkaId = 123L
        do! insertOrganization db teczkaId

        let event = KontaktyChanged {
            TeczkaId = teczkaId
            Email = "test@example.com"
            // ... all fields
            Audit = { Who = "user"; OccurredAt = DateTime.UtcNow }
        }

        // Act - Project twice
        do! Projections.project event db
        let! state1 = db.Single<OrganizationRow>("SELECT * FROM organizacje WHERE teczka = @id", {| id = teczkaId |})

        do! Projections.project event db
        let! state2 = db.Single<OrganizationRow>("SELECT * FROM organizacje WHERE teczka = @id", {| id = teczkaId |})

        // Assert - Same state after replaying
        state1 |> should equal state2
    } |> Async.RunSynchronously
```

**Acceptance Criteria:**
- Tests verify idempotency (same event replayed = same state)
- Tests verify full event replay rebuilds correct state

---

#### 3.9: Verify 100% test coverage for projections and ES handlers
**Type:** Quality Gate
**Approach:** Coverage Analysis

**Actions:**
1. Run coverage for Projections.fs and EventSourcedHandlers.fs
2. Verify all projection functions tested
3. Verify all 6 handlers tested end-to-end

**Required Coverage:**
- Projections.fs: 100% (all event types, all columns)
- EventSourcedHandlers.fs: 100% (all 6 handlers, synthetic event path, concurrency conflicts)
- SyntheticEvents.fs: 100% (synthetic event creation)

**Acceptance Criteria:**
- Coverage report shows 100%
- Integration tests run against real database
- End-to-end flow verified (command → event → projection → DB)

**Phase 3 Complete:** Event-sourced handlers implemented alongside legacy handlers.

---

## PHASE 4: Feature Flag & Integration

**Goal:** Add feature flag and audit trail adapter with 100% test coverage

**Duration Estimate:** 1 week
**Tasks:** 9

### Phase Overview

Implement feature flag to conditionally use ES handlers vs legacy handlers. Create adapter to convert events to legacy AuditTrail format. Update CompositionRoot to wire correct handlers based on feature flag.

**File Structure:**
```
Web/Organizations/EventSourcing/
├── FeatureFlag.fs          # Feature flag evaluation logic
└── AuditTrailAdapter.fs    # Convert events to AuditTrail format
```

### Tasks

#### 4.1: TDD - Write unit tests for feature flag evaluation
**Type:** Test
**Approach:** Test-First (TDD)

**Create test file:** `/OperatorPortal/Tests/Organizations/EventSourcing/FeatureFlagTests.fs`

**Test Cases:**
1. `Feature flag at 0% returns false for all TeczkaIds`
2. `Feature flag at 100% returns true for all TeczkaIds`
3. `Feature flag at 50% returns consistent result per TeczkaId`
4. `Feature flag hashing is deterministic`

**Example Test:**
```fsharp
[<Fact>]
let ``Feature flag at 50% is deterministic per TeczkaId`` () =
    // Arrange
    let teczkaId = TeczkaId.create 123L

    // Act
    let result1 = FeatureFlag.isEnabled 50 teczkaId
    let result2 = FeatureFlag.isEnabled 50 teczkaId

    // Assert
    result1 |> should equal result2

[<Fact>]
let ``Feature flag at 50% splits approximately 50-50`` () =
    // Arrange
    let teczkaIds = [1L..1000L] |> List.map TeczkaId.create

    // Act
    let enabled =
        teczkaIds
        |> List.filter (FeatureFlag.isEnabled 50)
        |> List.length

    // Assert - Within 5% of 50% (450-550)
    enabled |> should be (greaterThan 450)
    enabled |> should be (lessThan 550)
```

**Acceptance Criteria:**
- Tests compile but fail (feature flag not implemented)
- Edge cases tested (0%, 100%, determinism)

---

#### 4.2: Implement FeatureFlag.fs
**Type:** Implementation
**Approach:** Test-Driven (make tests from 4.1 pass)

**Create `FeatureFlag.fs`:**
```fsharp
module Organizations.EventSourcing.FeatureFlag

open System
open Organizations.Domain

// Read from environment variable
let private getThresholdFromEnv () : int =
    let envVar = Environment.GetEnvironmentVariable("USE_EVENT_SOURCING_FOR_ORGANIZATION")
    match Int32.TryParse(envVar) with
    | true, value when value >= 0 && value <= 100 -> value
    | _ -> 0  // Default: disabled

// Hash TeczkaId to deterministic percentage bucket
let private hash (teczkaId: TeczkaId) : int =
    let id = TeczkaId.unwrap teczkaId
    // Simple modulo hashing
    int (id % 100L)

// Check if ES enabled for this TeczkaId
let isEnabled (threshold: int) (teczkaId: TeczkaId) : bool =
    let bucket = hash teczkaId
    bucket < threshold

// Convenience function using environment variable
let isEnabledForOrganization (teczkaId: TeczkaId) : bool =
    let threshold = getThresholdFromEnv()
    isEnabled threshold teczkaId
```

**Acceptance Criteria:**
- All tests from 4.1 pass
- Environment variable `USE_EVENT_SOURCING_FOR_ORGANIZATION` controls threshold
- Hashing is deterministic (same TeczkaId = same result)
- Values: 0-100 (percentage)

---

#### 4.3: TDD - Write unit tests for AuditTrailAdapter
**Type:** Test
**Approach:** Test-First (TDD)

**Create test file:** `/OperatorPortal/Tests/Organizations/EventSourcing/AuditTrailAdapterTests.fs`

**Test Cases:**
1. `KontaktyChanged event converts to AuditTrail with diff`
2. `AuditTrail captures Who and OccuredAt from event`
3. `AuditTrail Kind matches event type`
4. `Diff contains only changed fields`

**Example Test:**
```fsharp
[<Fact>]
let ``KontaktyChanged converts to AuditTrail`` () =
    // Arrange
    let oldKontakty = {
        Email = "old@example.com"
        Telefon = "111111111"
        // ... all fields
    }
    let event = KontaktyChanged {
        TeczkaId = 123L
        Email = "new@example.com"
        Telefon = "111111111"  // Unchanged
        // ... all fields
        Audit = { Who = "user@test.com"; OccurredAt = DateTime(2026, 1, 4) }
    }

    // Act
    let auditTrail = AuditTrailAdapter.fromEvent event oldKontakty

    // Assert
    auditTrail.Who |> should equal "user@test.com"
    auditTrail.OccuredAt |> should equal (DateTime(2026, 1, 4))
    auditTrail.Kind |> should equal "Kontakty"
    auditTrail.EntityId |> should equal 123L

    // Diff should contain only Email change
    auditTrail.Diff |> Map.containsKey "Email" |> should equal true
    auditTrail.Diff |> Map.containsKey "Telefon" |> should equal false
```

**Acceptance Criteria:**
- Tests compile but fail (adapter not implemented)
- All 6 event types tested
- Diff calculation tested

---

#### 4.4: Implement AuditTrailAdapter.fs
**Type:** Implementation
**Approach:** Test-Driven (make tests from 4.3 pass)

**Create `AuditTrailAdapter.fs`:**
```fsharp
module Organizations.EventSourcing.AuditTrailAdapter

open Organizations.EventSourcing.Events
open Organizations.Application.Audit

// Convert event to AuditTrail format
let fromEvent (event: OrganizationEvent) (oldState: obj) : AuditTrail =
    let audit = OrganizationEvent.getAudit event
    let kind =
        match event with
        | KontaktyChanged _ -> "Kontakty"
        | ZrodlaZywnosciChanged _ -> "ZrodlaZywnosci"
        | AdresyKsiegowosciChanged _ -> "AdresyKsiegowosci"
        | DaneAdresoweChanged _ -> "DaneAdresowe"
        | BeneficjenciChanged _ -> "Beneficjenci"
        | WarunkiPomocyChanged _ -> "WarunkiPomocy"
        | OrganizationCreated _ -> "OrganizationCreated"

    let entityId =
        match event with
        | KontaktyChanged e -> e.TeczkaId
        | ZrodlaZywnosciChanged e -> e.TeczkaId
        | AdresyKsiegowosciChanged e -> e.TeczkaId
        | DaneAdresoweChanged e -> e.TeczkaId
        | BeneficjenciChanged e -> e.TeczkaId
        | WarunkiPomocyChanged e -> e.TeczkaId
        | OrganizationCreated e -> e.TeczkaId

    // Calculate diff (compare event data with old state)
    let diff =
        match event, oldState with
        | KontaktyChanged newData, (:? Kontakty as oldData) ->
            FindDiffForAudit.findDiff oldData (toKontaktyDomain newData)
        | _ -> Map.empty

    {
        Who = audit.Who
        OccuredAt = audit.OccurredAt
        EntityId = entityId
        Kind = kind
        Diff = diff
    }

// Helper to convert event data back to domain type for diff
let private toKontaktyDomain (event: KontaktyChangedV1) : Kontakty =
    {
        Email = event.Email
        Telefon = event.Telefon
        OsobaDoKontaktu = event.OsobaDoKontaktu
        TelefonOsobyKontaktowej = event.TelefonOsobyKontaktowej
        MailOsobyKontaktowej = event.MailOsobyKontaktowej
        OsobaOdbierajacaZywnosc = event.OsobaOdbierajacaZywnosc
        TelefonOsobyOdbierajacej = event.TelefonOsobyOdbierajacej
        Kontakt = event.Kontakt
        Przedstawiciel = event.Przedstawiciel
        Dostepnosc = event.Dostepnosc
        WwwFacebook = event.WwwFacebook
    }

// Similar helpers for other 5 event types...
```

**Acceptance Criteria:**
- All tests from 4.3 pass
- Reuses existing `FindDiffForAudit.findDiff` function
- Produces AuditTrail compatible with existing audit trail queries

---

#### 4.5: TDD - Write integration tests for CompositionRoot conditional wiring
**Type:** Test
**Approach:** Test-First (TDD)

**Create test file:** `/OperatorPortal/Tests/Organizations/CompositionRootTests.fs`

**Test Cases:**
1. `When feature flag OFF, legacy handlers used`
2. `When feature flag ON, ES handlers used`
3. `Handler signature matches expected type`

**Example Test:**
```fsharp
[<Fact>]
let ``CompositionRoot wires ES handler when feature flag enabled`` () =
    // Arrange
    Environment.SetEnvironmentVariable("USE_EVENT_SOURCING_FOR_ORGANIZATION", "100")
    let deps = CompositionRoot.build (connectDb, blobClient)
    let teczkaId = 123L  // Will hash to enabled

    // Act
    let handlerType = deps.ChangeKontakty.GetType()

    // Assert
    handlerType.FullName |> should contain "EventSourcedHandlers"

[<Fact>]
let ``CompositionRoot wires legacy handler when feature flag disabled`` () =
    // Arrange
    Environment.SetEnvironmentVariable("USE_EVENT_SOURCING_FOR_ORGANIZATION", "0")
    let deps = CompositionRoot.build (connectDb, blobClient)

    // Act
    let handlerType = deps.ChangeKontakty.GetType()

    // Assert
    handlerType.FullName |> should contain "Handlers"
    handlerType.FullName |> should not' (contain "EventSourced")
```

**Acceptance Criteria:**
- Tests compile but fail (conditional wiring not implemented)
- Tests verify correct handler wired based on feature flag

---

#### 4.6: Update CompositionRoot.fs to conditionally wire handlers
**Type:** Implementation
**Approach:** Test-Driven (make tests from 4.5 pass)

**Edit `CompositionRoot.fs`:**
```fsharp
module Organizations.CompositionRoot

open Organizations.Application.CommandHandlers
open Organizations.EventSourcing

let build (connectDb, blobServiceClient) : Dependencies =

    // Feature flag check
    let useEventSourcing =
        let threshold =
            match System.Environment.GetEnvironmentVariable("USE_EVENT_SOURCING_FOR_ORGANIZATION") with
            | null -> 0
            | value ->
                match System.Int32.TryParse(value) with
                | true, v -> v
                | _ -> 0
        threshold > 0

    if useEventSourcing then
        // Event-sourced handlers
        {
            ReadOrganizationSummaries = ReadModels.readOrganizationSummaries connectDb
            ReadOrganizationDetailsBy = ReadModels.readOrganizationDetailsBy connectDb
            ReadAuditTrail = ReadModels.readAuditTrail connectDb
            ReadMailingList = MailingList.readMailingList connectDb

            // ES handlers wrapped with feature flag check per organization
            ChangeKontakty = wrapWithFeatureFlag
                (EventSourcedHandlers.changeKontakty connectDb)
                (Handlers.changeKontakty
                    (OrganizationsDao.readBy connectDb)
                    (OrganizationsDao.save connectDb)
                    (AuditTrailDao.AuditTrailDao(connectDb).SaveAuditTrail))

            ChangeZrodlaZywnosci = wrapWithFeatureFlag
                (EventSourcedHandlers.changeZrodlaZywnosci connectDb)
                (Handlers.changeZrodlaZywnosci (* ... *))

            // ... other 4 handlers similarly wrapped
        }
    else
        // Legacy handlers only
        {
            ReadOrganizationSummaries = ReadModels.readOrganizationSummaries connectDb
            ReadOrganizationDetailsBy = ReadModels.readOrganizationDetailsBy connectDb
            ReadAuditTrail = ReadModels.readAuditTrail connectDb
            ReadMailingList = MailingList.readMailingList connectDb

            ChangeKontakty =
                Handlers.changeKontakty
                    (OrganizationsDao.readBy connectDb)
                    (OrganizationsDao.save connectDb)
                    (AuditTrailDao.AuditTrailDao(connectDb).SaveAuditTrail)

            // ... other 5 legacy handlers
        }

// Helper to wrap handler with per-organization feature flag check
let private wrapWithFeatureFlag
    (esHandler: Handlers.ChangeKontakty)
    (legacyHandler: Handlers.ChangeKontakty)
    : Handlers.ChangeKontakty =
    fun (teczkaIdInt, audit, payload) ->
        asyncResult {
            let! teczkaId = TeczkaId.create teczkaIdInt
            if FeatureFlag.isEnabledForOrganization teczkaId then
                return! esHandler (teczkaIdInt, audit, payload)
            else
                return! legacyHandler (teczkaIdInt, audit, payload)
        }
```

**Acceptance Criteria:**
- All tests from 4.5 pass
- Feature flag evaluated per-organization (TeczkaId hashing)
- Both code paths maintained (ES and legacy)
- Wrapper function enables gradual rollout

---

#### 4.7: TDD - Write integration tests verifying audit trail compatibility
**Type:** Test
**Approach:** Test-First (TDD)

**Add to `EventSourcedHandlersTests.fs` or new file:**

**Test Cases:**
1. `ES handler creates AuditTrail entry`
2. `Legacy handler creates AuditTrail entry`
3. `Both formats queryable by ReadAuditTrail`
4. `AuditTrail diff format matches`

**Example Test:**
```fsharp
[<Fact>]
let ``ES handler creates compatible audit trail`` () =
    async {
        // Arrange
        use! db = setupTestDb()
        let teczkaId = TeczkaId.create 123L
        do! insertOrganization db (TeczkaId.unwrap teczkaId)

        let command = { (* Kontakty payload *) }
        let audit = { Who = "user@test.com"; OccuredAt = DateTime.UtcNow }

        // Act - Use ES handler
        let! result = EventSourcedHandlers.changeKontakty connectDb (TeczkaId.unwrap teczkaId, audit, command)

        // Assert - Verify AuditTrail entry exists
        let! auditTrail = ReadModels.readAuditTrail connectDb (TeczkaId.unwrap teczkaId)

        auditTrail |> should not' (be Empty)
        let entry = auditTrail |> List.head
        entry.Who |> should equal "user@test.com"
        entry.Kind |> should equal "Kontakty"
    } |> Async.RunSynchronously
```

**Acceptance Criteria:**
- Tests verify ES handlers save audit trail
- Tests verify audit trail format matches legacy
- Tests verify existing audit queries work with ES entries

---

#### 4.8: TDD - Write integration tests for feature flag percentages
**Type:** Test
**Approach:** Test-First (TDD)

**Create test file:** `/OperatorPortal/Tests/Organizations/FeatureFlagIntegrationTests.fs`

**Test Cases:**
1. `0% rollout uses legacy handlers for all organizations`
2. `100% rollout uses ES handlers for all organizations`
3. `50% rollout uses mix of ES and legacy`

**Example Test:**
```fsharp
[<Fact>]
let ``10% rollout enables ES for approximately 10% of organizations`` () =
    async {
        // Arrange
        Environment.SetEnvironmentVariable("USE_EVENT_SOURCING_FOR_ORGANIZATION", "10")
        use! db = setupTestDb()
        let teczkaIds = [1L..100L]

        let mutable esCount = 0
        let mutable legacyCount = 0

        // Act - Execute handlers for 100 organizations
        for id in teczkaIds do
            let teczkaId = TeczkaId.create id
            do! insertOrganization db id

            let! _ = deps.ChangeKontakty (id, audit, payload)

            // Check if ES was used (event exists)
            let! events = EventStore.loadEvents connectDb "Organization" (teczkaIdToGuid teczkaId)
            if not (List.isEmpty events) then
                esCount <- esCount + 1
            else
                legacyCount <- legacyCount + 1

        // Assert - Approximately 10% used ES (5-15%)
        esCount |> should be (greaterThan 5)
        esCount |> should be (lessThan 15)
    } |> Async.RunSynchronously
```

**Acceptance Criteria:**
- Tests verify percentage rollout works
- Tests verify determinism (same org = same handler)

---

#### 4.9: Verify 100% test coverage for feature flag and integration logic
**Type:** Quality Gate
**Approach:** Coverage Analysis

**Actions:**
1. Run coverage for FeatureFlag.fs and AuditTrailAdapter.fs
2. Verify CompositionRoot conditional wiring tested
3. Verify all feature flag percentages tested

**Required Coverage:**
- FeatureFlag.fs: 100% (all percentages, hashing logic)
- AuditTrailAdapter.fs: 100% (all event types, diff calculation)
- CompositionRoot.fs: 100% (both code paths: ES and legacy)

**Acceptance Criteria:**
- Coverage report shows 100%
- Integration tests verify end-to-end feature flag behavior
- Audit trail compatibility verified

**Phase 4 Complete:** Feature flag and integration logic ready for gradual rollout.

---

## PHASE 5: Observability & Rollout

**Goal:** Add logging, metrics, and execute gradual rollout with 100% monitoring coverage

**Duration Estimate:** 4 weeks (includes deployment weeks)
**Tasks:** 9

### Phase Overview

Add observability (logging, metrics), establish performance baseline, and execute 4-week gradual rollout plan (0% → 10% → 50% → 100%).

### Tasks

#### 5.1: TDD - Write unit tests for logging behavior
**Type:** Test
**Approach:** Test-First (TDD)

**Create test file:** `/OperatorPortal/Tests/EventStore/LoggingTests.fs`

**Test Cases:**
1. `Event append logs duration and event count`
2. `Concurrency conflict logs warning with details`
3. `Errors log with exception details`

**Example Test:**
```fsharp
[<Fact>]
let ``appendEvents logs successful append`` () =
    // Arrange
    let loggerMock = Mock<ILogger>()
    let eventStore = EventStore.Core.withLogging loggerMock.Object

    // Act
    let! result = eventStore.appendEvents (* ... *)

    // Assert
    loggerMock.Verify(
        fun logger ->
            logger.LogInformation(
                It.Is<string>(fun msg -> msg.Contains("Appended")),
                It.IsAny<obj[]>()
            ),
        Times.Once()
    )
```

**Acceptance Criteria:**
- Tests use mocked ILogger to verify logging calls
- Tests verify structured logging (event count, duration, aggregate ID)

---

#### 5.2: Add structured logging to EventStore operations
**Type:** Implementation
**Approach:** Test-Driven (make tests from 5.1 pass)

**Update `EventStore.Core.fs`:**
```fsharp
open Microsoft.Extensions.Logging

let appendEventsWithLogging
    (logger: ILogger)
    (connectDb: unit -> Async<IDbConnection>)
    (aggregateType: string)
    (aggregateId: Guid)
    (expectedVersion: int)
    (events: 'EventData list)
    (project: 'EventData -> IDbConnection -> Async<unit>)
    : Async<Result<unit, string>> =
    async {
        let startTime = DateTime.UtcNow

        logger.LogInformation(
            "Appending {EventCount} events for {AggregateType}:{AggregateId}",
            events.Length, aggregateType, aggregateId
        )

        let! result = appendEvents connectDb aggregateType aggregateId expectedVersion events project

        match result with
        | Ok () ->
            let duration = (DateTime.UtcNow - startTime).TotalMilliseconds
            logger.LogInformation(
                "Appended {EventCount} events for {AggregateType}:{AggregateId} in {DurationMs}ms",
                events.Length, aggregateType, aggregateId, duration
            )
            return Ok ()

        | Error msg when msg.Contains("Concurrency conflict") ->
            logger.LogWarning(
                "Concurrency conflict for {AggregateType}:{AggregateId}, expected version {ExpectedVersion}",
                aggregateType, aggregateId, expectedVersion
            )
            return Error msg

        | Error msg ->
            logger.LogError(
                "Failed to append events for {AggregateType}:{AggregateId}: {ErrorMessage}",
                aggregateType, aggregateId, msg
            )
            return Error msg
    }
```

**Acceptance Criteria:**
- All tests from 5.1 pass
- Structured logging with named parameters
- Logs: successful appends, concurrency conflicts, errors

---

#### 5.3: Add metrics instrumentation
**Type:** Implementation
**Approach:** Code

**Create `EventStore.Metrics.fs`:**
```fsharp
module EventStore.Metrics

open System.Diagnostics.Metrics

type EventStoreMetrics(meter: Meter) =
    let eventAppendCounter =
        meter.CreateCounter<int>("event_store.events_appended")

    let eventAppendDuration =
        meter.CreateHistogram<double>("event_store.append_duration_ms")

    let concurrencyConflictCounter =
        meter.CreateCounter<int>("event_store.concurrency_conflicts")

    let projectionDuration =
        meter.CreateHistogram<double>("event_store.projection_duration_ms")

    member _.RecordEventAppended(count: int) =
        eventAppendCounter.Add(count)

    member _.RecordAppendDuration(durationMs: double) =
        eventAppendDuration.Record(durationMs)

    member _.RecordConcurrencyConflict() =
        concurrencyConflictCounter.Add(1)

    member _.RecordProjectionDuration(durationMs: double) =
        projectionDuration.Record(durationMs)

// Integrate into appendEvents
let appendEventsWithMetrics
    (metrics: EventStoreMetrics)
    (connectDb: unit -> Async<IDbConnection>)
    (* ... other parameters ... *)
    : Async<Result<unit, string>> =
    async {
        let startTime = DateTime.UtcNow

        let! result = appendEvents (* ... *)

        match result with
        | Ok () ->
            let duration = (DateTime.UtcNow - startTime).TotalMilliseconds
            metrics.RecordEventAppended(events.Length)
            metrics.RecordAppendDuration(duration)

        | Error msg when msg.Contains("Concurrency conflict") ->
            metrics.RecordConcurrencyConflict()

        | Error _ -> ()

        return result
    }
```

**Acceptance Criteria:**
- Metrics: event count, append duration, concurrency conflicts
- Histograms for latency (p50, p95, p99)
- Counters for errors

---

#### 5.4: Create performance baseline script
**Type:** Tooling
**Approach:** Script

**Create script:** `/OperatorPortal/Tools/PerformanceBaseline.fsx`

```fsharp
#r "nuget: BenchmarkDotNet"

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

[<MemoryDiagnoser>]
type OrganizationHandlerBenchmarks() =

    [<Benchmark>]
    member this.LegacyChangeKontakty() =
        // Execute legacy handler
        Handlers.changeKontakty (* ... *)
        |> Async.RunSynchronously

    [<Benchmark>]
    member this.ESChangeKontakty() =
        // Execute ES handler
        EventSourcedHandlers.changeKontakty (* ... *)
        |> Async.RunSynchronously

// Run benchmarks
BenchmarkRunner.Run<OrganizationHandlerBenchmarks>()
```

**Acceptance Criteria:**
- Baseline measurements for all 6 handlers
- p50, p95, p99 latencies recorded
- Results documented for comparison during rollout

---

#### 5.5: Week 1 - Deploy with USE_EVENT_SOURCING_FOR_ORGANIZATION=0
**Type:** Deployment
**Approach:** Manual

**Actions:**
1. Deploy application with feature flag set to `0`
2. Verify EventStore infrastructure deployed (events table exists)
3. Smoke test: Verify no errors in logs
4. Verify existing handlers work (no regressions)

**Acceptance Criteria:**
- Deployment successful
- No errors in application logs
- All existing handlers work correctly
- Events table created but empty (no events written)

---

#### 5.6: Week 2 - Enable for 10% and monitor
**Type:** Deployment + Monitoring
**Approach:** Manual

**Actions:**
1. Set `USE_EVENT_SOURCING_FOR_ORGANIZATION=10`
2. Deploy application
3. Monitor metrics:
   - Error rate (should be 0%)
   - Latency (p50, p95, p99 - should be within ±20% of baseline)
   - Event append success rate
   - Concurrency conflict rate
4. Verify audit trail entries created for ES organizations
5. Check logs for errors or warnings

**Acceptance Criteria:**
- ~10% of organizations using ES (deterministic per TeczkaId)
- Error rate: 0%
- Latency degradation: <20%
- No data integrity issues
- Audit trail complete

---

#### 5.7: Week 3 - Enable for 50% if metrics acceptable
**Type:** Deployment + Monitoring
**Approach:** Manual

**Actions:**
1. Review Week 2 metrics (if acceptable, proceed)
2. Set `USE_EVENT_SOURCING_FOR_ORGANIZATION=50`
3. Deploy application
4. Monitor same metrics as Week 2
5. Compare latencies to baseline

**Acceptance Criteria:**
- Week 2 metrics acceptable (no errors, <20% degradation)
- ~50% of organizations using ES
- Error rate: 0%
- Latency degradation: <20%
- No rollback needed

---

#### 5.8: Week 4 - Enable for 100% if metrics acceptable
**Type:** Deployment + Monitoring
**Approach:** Manual

**Actions:**
1. Review Week 3 metrics (if acceptable, proceed)
2. Set `USE_EVENT_SOURCING_FOR_ORGANIZATION=100`
3. Deploy application
4. Monitor same metrics
5. Full ES migration complete

**Acceptance Criteria:**
- Week 3 metrics acceptable
- 100% of organizations using ES
- Error rate: 0%
- Latency degradation: <20%
- Legacy handlers no longer invoked

---

#### 5.9: Monitor production for 4+ weeks
**Type:** Monitoring
**Approach:** Manual

**Actions:**
1. Continue monitoring for at least 4 weeks
2. Verify stability:
   - No errors
   - No performance degradation
   - Audit trail complete
   - Event store growing as expected
3. Document any incidents or issues
4. Validate rollback capability (can revert to 0% if needed)

**Acceptance Criteria:**
- 4+ weeks of stable operation at 100%
- No critical issues
- Rollback capability validated
- Ready for Phase 6 (cleanup)

**Phase 5 Complete:** ES fully rolled out and stable in production.

---

## PHASE 6: Cleanup & Documentation

**Goal:** Remove legacy code and finalize with 100% documentation coverage

**Duration Estimate:** 1 week
**Tasks:** 9

### Phase Overview

Remove legacy handlers, feature flag logic, and finalize documentation. ES becomes the only implementation.

### Tasks

#### 6.1: Remove legacy Handlers.fs
**Type:** Cleanup
**Approach:** Manual

**Actions:**
1. Delete `/OperatorPortal/Web/Organizations/Application/CommandHandlers/Handlers.fs`
2. Remove compile item from `Web.fsproj`
3. Verify project compiles (no references to legacy handlers)

**Acceptance Criteria:**
- Legacy handlers removed
- Project compiles successfully
- No references to old `Handlers.changeKontakty` etc.

---

#### 6.2: Remove FindDiffForAudit.fs
**Type:** Cleanup
**Approach:** Manual

**Actions:**
1. Delete `/OperatorPortal/Web/Organizations/Application/FindDiffForAudit.fs`
2. Remove compile item from `Web.fsproj`
3. Update AuditTrailAdapter if it used FindDiffForAudit (now extracts diff from events)

**Acceptance Criteria:**
- FindDiffForAudit.fs removed
- Audit trail logic uses events directly
- Project compiles

---

#### 6.3: Remove feature flag logic from CompositionRoot
**Type:** Cleanup
**Approach:** Manual

**Actions:**
1. Edit `CompositionRoot.fs`
2. Remove conditional wiring (always use ES handlers)
3. Remove `wrapWithFeatureFlag` function
4. Simplify dependencies initialization

**Before:**
```fsharp
if useEventSourcing then
    { ChangeKontakty = EventSourcedHandlers.changeKontakty connectDb }
else
    { ChangeKontakty = Handlers.changeKontakty (* ... *) }
```

**After:**
```fsharp
{ ChangeKontakty = EventSourcedHandlers.changeKontakty connectDb }
```

**Acceptance Criteria:**
- Feature flag removed
- ES handlers always used
- Code simplified

---

#### 6.4: Remove AuditTrailAdapter.fs
**Type:** Cleanup
**Approach:** Manual

**Actions:**
1. Verify audit trail now derived directly from events
2. If AuditTrailAdapter no longer needed, delete it
3. Update audit trail queries to read from events table directly

**Acceptance Criteria:**
- Audit trail adapter removed (if no longer needed)
- Audit trail queries work correctly

---

#### 6.5: Update architecture documentation
**Type:** Documentation
**Approach:** Manual

**Actions:**
1. Update `/OperatorPortal/docs/architecture/overview.md`
2. Add section on Event Sourcing
3. Create flow diagram: Command → Events → Projections

**Content:**
```markdown
## Event Sourcing

The Organization vertical slice uses event sourcing for persistence.

### Architecture

```
User Request
    ↓
Handler (EventSourcedHandlers)
    ↓
Load Events (EventStore.loadEvents)
    ↓
Replay → Current State
    ↓
Decide (validate command → produce events)
    ↓
Append Events + Project (single transaction)
    ↓
Response
```

### Key Components

- **EventStore**: Shared infrastructure for event persistence
- **Events**: Immutable facts (KontaktyChangedV1, etc.)
- **Commands**: User intent (ChangeKontakty, etc.)
- **Decide**: Pure function (command + state → events)
- **Evolve**: Pure function (state + event → new state)
- **Projections**: Event → DB updates (organizacje table)
```

**Acceptance Criteria:**
- Architecture docs updated with ES flow
- Diagrams added
- Event sourcing patterns explained

---

#### 6.6: Create event catalog documentation
**Type:** Documentation
**Approach:** Manual

**Create file:** `/OperatorPortal/docs/events/organization-events.md`

**Content:**
```markdown
# Organization Events Catalog

## Event Types

### 1. OrganizationCreatedV1

**Description:** Initial snapshot of organization state (synthetic event for existing orgs)

**Schema:**
```json
{
  "TeczkaId": 123,
  "IdentyfikatorEnova": "...",
  "NIP": "...",
  "Regon": "...",
  "KrsNr": "...",
  "FormaPrawna": "...",
  "OPP": false,
  "DaneAdresowe": { /* ... */ },
  "Kontakty": { /* ... */ },
  "ZrodlaZywnosci": { /* ... */ },
  "AdresyKsiegowosci": { /* ... */ },
  "Beneficjenci": { /* ... */ },
  "WarunkiPomocy": { /* ... */ },
  "Audit": {
    "Who": "user@example.com",
    "OccurredAt": "2026-01-04T12:00:00Z"
  }
}
```

### 2. KontaktyChangedV1

**Description:** Contact information updated

**Schema:**
```json
{
  "TeczkaId": 123,
  "Email": "new@example.com",
  "Telefon": "123456789",
  // ... all Kontakty fields
  "Audit": { /* ... */ }
}
```

**Projection:** Updates `organizacje` table columns: Email, Telefon, etc.

### 3-7. Other Event Types

(Document ZrodlaZywnosciChangedV1, AdresyKsiegowosciChangedV1, etc.)
```

**Acceptance Criteria:**
- All 7 event types documented
- Schemas included (JSON examples)
- Projections explained

---

#### 6.7: Document time-traveling capability
**Type:** Documentation
**Approach:** Manual

**Add to event catalog or new file:** `/OperatorPortal/docs/events/time-traveling.md`

**Content:**
```markdown
# Time-Traveling Queries

Event sourcing enables reconstructing organization state at any point in time.

## Query State at Specific Version

```fsharp
// Load events up to version N
let! events =
    EventStore.loadEvents connectDb "Organization" aggregateId
    |> Async.map (List.take versionN)

// Replay to reconstitute state
let historicalState = Evolution.replay events
```

## Query State at Specific Timestamp

```fsharp
// Load events before timestamp
let! events =
    EventStore.loadEventsBefore connectDb "Organization" aggregateId timestamp

let historicalState = Evolution.replay events
```

## Use Cases

- Audit: "What did this organization look like on 2025-12-01?"
- Analytics: "How many organizations changed their Kontakty in Q4 2025?"
- Debugging: "What sequence of changes led to this state?"
```

**Acceptance Criteria:**
- Time-traveling capability documented
- Code examples provided
- Use cases explained

---

#### 6.8: Document event versioning strategy
**Type:** Documentation
**Approach:** Manual

**Add to event catalog:** `/OperatorPortal/docs/events/versioning-strategy.md`

**Content:**
```markdown
# Event Versioning Strategy

## Approach: Versioned Types with Upcasting

We use versioned event types (V1, V2, etc.) with upcasting functions.

## Example: Adding Field to KontaktyChanged

### Step 1: Create V2 Event
```fsharp
type KontaktyChangedV2 = {
    TeczkaId: int64
    Email: string
    Telefon: string
    // ... existing fields
    MobilePhone: string  // NEW FIELD
    Audit: EventAudit
}
```

### Step 2: Update Discriminated Union
```fsharp
type OrganizationEvent =
    | KontaktyChangedV1 of KontaktyChangedV1
    | KontaktyChangedV2 of KontaktyChangedV2  // NEW
    // ... other events
```

### Step 3: Create Upcast Function
```fsharp
module KontaktyChanged =
    let upcast = function
        | V1 v1 ->
            {
                TeczkaId = v1.TeczkaId
                Email = v1.Email
                Telefon = v1.Telefon
                // ... copy all fields
                MobilePhone = ""  // Default for old events
                Audit = v1.Audit
            }
        | V2 v2 -> v2
```

### Step 4: Update decide, evolve, project
- decide: Produce V2 events
- evolve: Handle both V1 and V2 (upcast V1 → V2)
- project: Handle both versions

## Best Practices

- Never modify existing event types
- Always create new version (V2, V3, etc.)
- Upcast old events to latest version in application logic
- Store events in their original version (no migration)
```

**Acceptance Criteria:**
- Versioning strategy documented
- Upcasting pattern explained with examples
- Best practices included

---

#### 6.9: Final verification - Confirm 100% test coverage
**Type:** Quality Gate
**Approach:** Coverage Analysis

**Actions:**
1. Run full test suite
2. Generate coverage report for all EventSourcing code
3. Verify 100% coverage maintained after cleanup

**Required Coverage:**
- EventStore package: 100%
- Organizations.EventSourcing: 100%
- No legacy code remaining (Handlers.fs, FindDiffForAudit.fs removed)

**Acceptance Criteria:**
- Coverage report shows 100%
- All tests pass
- No legacy code references

**Phase 6 Complete:** Migration finalized, legacy code removed, documentation complete.

---

## Summary

**Total Tasks:** 61
**Total Phases:** 6
**Estimated Duration:** 8-10 weeks (including 4-week rollout)

**Key Deliverables:**
1. ✅ Shared EventStore package (reusable for future slices)
2. ✅ Event-sourced Organization aggregate
3. ✅ 100% test coverage (mandatory)
4. ✅ TDD approach throughout
5. ✅ Feature-flagged gradual rollout
6. ✅ Complete documentation (architecture, events, time-traveling)
7. ✅ Zero downtime migration
8. ✅ Legacy code removed

**Success Metrics:**
- 100% test coverage maintained
- No performance degradation >20%
- Zero data loss
- Complete audit trail
- Time-traveling capability demonstrated

---

**Document Status:** Ready for Implementation
**Next Step:** Begin Phase 1, Task 1.1
