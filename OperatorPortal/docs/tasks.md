# Organization Event Sourcing Migration - Implementation Tasks

**Document Version:** 2.0
**Created:** 2026-01-04
**Last Updated:** 2026-01-05
**Status:** Phase 1 Complete - Phase 1.5 Ready
**Related PRD:** [prd-organization-event-sourcing.md](prd-organization-event-sourcing.md)
**Architecture Guide:** [architecture/event-sourcing.md](architecture/event-sourcing.md)

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
| **Phase 1: EventStore Infrastructure** | ✅ **COMPLETE** | 2026-01-05 | All 12 tasks complete. 14 integration tests passing. EventStore package production-ready. |
| **Phase 1.5: EventSourcing Patterns** | ⏸️ **NOT STARTED** | - | Ready to begin. Generic patterns for command handling, feature flags, and projections. |
| **Phase 2: Domain Modeling** | ⏸️ **NOT STARTED** | - | Blocked by Phase 1.5 completion. Organizations/EventSourcing directory to be created. |
| **Phase 3: Projections & Dual-Write** | ⏸️ **NOT STARTED** | - | Blocked by Phase 2 completion. |
| **Phase 4: Feature Flag & Integration** | ⏸️ **NOT STARTED** | - | Blocked by Phase 3 completion. |
| **Phase 5: Observability & Rollout** | ⏸️ **NOT STARTED** | - | Blocked by Phase 4 completion. |
| **Phase 6: Cleanup & Documentation** | ⏸️ **NOT STARTED** | - | Blocked by Phase 5 completion. |

---

## PHASE 1: EventStore Infrastructure ✅ **COMPLETE**

**Goal:** Create shared EventStore package with 100% test coverage

**Status:** ✅ Complete (2026-01-05)
**Duration:** Completed
**Tasks:** 12/12 Complete

### Phase Overview

Create a reusable EventStore package following the PostgresPersistence pattern. This package will be used by Organizations and future vertical slices.

**File Structure:**
```
Web/EventStore/
├── EventStore.Types.fs        # Core types (Event envelope, EventMetadata)
├── EventStore.Serialization.fs # JSON serialization helpers
└── EventStore.Core.fs         # loadEvents, appendEvents, getCurrentVersion
```

### Tasks

#### 1.1: Create EventStore package structure ✅
**Type:** Setup
**Approach:** Manual
**Status:** ✅ Complete

- Create folder: `/OperatorPortal/Web/EventStore/`
- Create empty files:
  - `EventStore.Types.fs`
  - `EventStore.Serialization.fs`
  - `EventStore.Core.fs`
- Define module namespaces following PostgresPersistence pattern
- Add XML documentation comments for public API

**Acceptance Criteria:**
- Folder structure matches PostgresPersistence layout
- Files compile (empty modules)
- Module namespaces are `EventStore.Types`, `EventStore.Serialization`, `EventStore.Core`

---

#### 1.2: Create events table DbUp migration ✅
**Type:** Database Schema
**Approach:** Migration Script
**Status:** ✅ Complete

- Create new migration file in `/OperatorPortal/Migrations/Scripts/`
- Filename: `Script0XXX_CreateEventsTable.sql` (increment XXX based on existing migrations)

**Schema:**
```sql
CREATE TABLE IF NOT EXISTS events (
    event_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    aggregate_type VARCHAR(100) NOT NULL,
    aggregate_id UUID NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL,
    event_metadata JSONB,
    version INTEGER NOT NULL,
    occurred_at TIMESTAMP NOT NULL DEFAULT NOW(),

    -- Optimistic concurrency control
    CONSTRAINT unique_aggregate_version
        UNIQUE (aggregate_type, aggregate_id, version)
);

CREATE INDEX idx_events_aggregate
    ON events(aggregate_type, aggregate_id, version);
```

**Acceptance Criteria:**
- Migration runs successfully via DbUp
- Table created with correct schema
- Unique constraint enforces optimistic concurrency
- Index created for efficient event loading

---

#### 1.3: TDD - Write integration tests for EventStore.loadEvents ✅
**Type:** Test
**Approach:** Test-First (TDD)
**Status:** ✅ Complete

- Create test file: `/OperatorPortal/Tests/EventStore/EventStoreTests.fs`
- Use Testcontainers for PostgreSQL integration tests

**Test Cases:**
1. `loadEvents returns empty list when no events exist`
2. `loadEvents returns events in version order`
3. `loadEvents filters by aggregate_type and aggregate_id`
4. `loadEvents deserializes event_data correctly`
5. `loadEvents handles multiple aggregates independently`

**Example Test:**
```fsharp
[<Fact>]
let ``loadEvents returns events in version order`` () =
    async {
        // Arrange
        use! db = setupTestDb()
        let aggregateId = Guid.NewGuid()
        do! insertEvent db "Order" aggregateId 3 { Data = "Event3" }
        do! insertEvent db "Order" aggregateId 1 { Data = "Event1" }
        do! insertEvent db "Order" aggregateId 2 { Data = "Event2" }

        // Act
        let! events = EventStore.loadEvents connectDb "Order" aggregateId

        // Assert
        events |> should haveLength 3
        events.[0].Version |> should equal 1
        events.[1].Version |> should equal 2
        events.[2].Version |> should equal 3
    } |> Async.RunSynchronously
```

**Acceptance Criteria:**
- Tests written and compile (but fail - implementation doesn't exist yet)
- All edge cases covered
- Tests use Testcontainers for real PostgreSQL

---

#### 1.4: Implement EventStore.Types module ✅
**Type:** Implementation
**Approach:** Code
**Status:** ✅ Complete

**Create types in `EventStore.Types.fs`:**
```fsharp
module EventStore.Types

type EventEnvelope<'EventData> = {
    EventId: Guid
    AggregateType: string
    AggregateId: Guid
    EventType: string
    EventData: 'EventData
    Metadata: EventMetadata option
    Version: int
    OccurredAt: DateTime
}

and EventMetadata = {
    CausationId: Guid option
    CorrelationId: Guid option
    UserId: string option
}

type ExpectedVersion =
    | Any
    | NoStream
    | ExactVersion of int

type AppendResult =
    | Ok
    | ConcurrencyConflict of expectedVersion: int * actualVersion: int
```

**Acceptance Criteria:**
- Types defined with immutable F# records
- Generic `EventEnvelope<'EventData>` supports any event type
- Metadata supports causation/correlation tracking
- ExpectedVersion type enables optimistic concurrency patterns

---

#### 1.5: Implement EventStore.Serialization module ✅
**Type:** Implementation
**Approach:** Code
**Status:** ✅ Complete

**Create serialization helpers in `EventStore.Serialization.fs`:**
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

**Acceptance Criteria:**
- Uses Thoth.Json.Net (per project serialization conventions)
- Serializes F# discriminated unions correctly
- Option types handled correctly (None → null, Some x → x)
- Serialization roundtrip preserves data
- Uses camelCase naming strategy

---

#### 1.6: Implement EventStore.loadEvents function ✅
**Type:** Implementation
**Approach:** Test-Driven (make tests from 1.3 pass)
**Status:** ✅ Complete

**Implementation in `EventStore.Core.fs`:**
```fsharp
module EventStore.Core

open PostgresPersistence.DapperFsharp
open EventStore.Types
open EventStore.Serialization

type private EventRow = {
    event_data: string
    version: int
}

let loadEvents<'EventData>
    (connectDb: unit -> Async<IDbConnection>)
    (aggregateType: string)
    (aggregateId: Guid)
    : Async<'EventData list> =
    async {
        use! db = connectDb()
        let! rows = db.Query<EventRow>(
            "SELECT event_data, version
             FROM events
             WHERE aggregate_type = @aggregateType
               AND aggregate_id = @aggregateId
             ORDER BY version ASC",
            {| aggregateType = aggregateType; aggregateId = aggregateId |}
        )
        return
            rows
            |> List.map (fun row -> deserialize<'EventData> row.event_data)
    }
```

**Acceptance Criteria:**
- All tests from task 1.3 pass
- Follows PostgresPersistence pattern (connectDb factory function)
- Uses extension methods from DapperFsharp
- Events returned in version order
- Generic function works with any event type

---

#### 1.7: TDD - Write integration tests for EventStore.getCurrentVersion ✅
**Type:** Test
**Approach:** Test-First (TDD)
**Status:** ✅ Complete

**Test Cases:**
1. `getCurrentVersion returns 0 when no events exist`
2. `getCurrentVersion returns max version when events exist`
3. `getCurrentVersion filters by aggregate_type and aggregate_id`

**Example Test:**
```fsharp
[<Fact>]
let ``getCurrentVersion returns max version`` () =
    async {
        // Arrange
        use! db = setupTestDb()
        let aggregateId = Guid.NewGuid()
        do! insertEvent db "Order" aggregateId 1 { Data = "Event1" }
        do! insertEvent db "Order" aggregateId 2 { Data = "Event2" }
        do! insertEvent db "Order" aggregateId 3 { Data = "Event3" }

        // Act
        let! version = EventStore.getCurrentVersion connectDb "Order" aggregateId

        // Assert
        version |> should equal 3
    } |> Async.RunSynchronously
```

**Acceptance Criteria:**
- Tests written and compile (but fail)
- Edge cases covered (no events, multiple aggregates)

---

#### 1.8: Implement EventStore.getCurrentVersion function ✅
**Type:** Implementation
**Approach:** Test-Driven (make tests from 1.7 pass)
**Status:** ✅ Complete

**Implementation:**
```fsharp
let getCurrentVersion
    (connectDb: unit -> Async<IDbConnection>)
    (aggregateType: string)
    (aggregateId: Guid)
    : Async<int> =
    async {
        use! db = connectDb()
        let! result = db.trySingle<int>(
            "SELECT COALESCE(MAX(version), 0)
             FROM events
             WHERE aggregate_type = @aggregateType
               AND aggregate_id = @aggregateId",
            {| aggregateType = aggregateType; aggregateId = aggregateId |}
        )
        return result |> Option.defaultValue 0
    }
```

**Acceptance Criteria:**
- All tests from task 1.7 pass
- Returns 0 for new aggregates (no events)
- Uses `trySingle` for safe null handling

---

#### 1.9: TDD - Write integration tests for EventStore.appendEvents with optimistic concurrency ✅
**Type:** Test
**Approach:** Test-First (TDD)
**Status:** ✅ Complete

**Test Cases:**
1. `appendEvents succeeds when expected version matches`
2. `appendEvents fails with ConcurrencyConflict when version mismatch`
3. `appendEvents executes projection inline within same transaction`
4. `appendEvents rolls back on projection failure`
5. `appendEvents handles PostgreSQL unique constraint violation`
6. `appendEvents increments version for multiple events`

**Example Test:**
```fsharp
[<Fact>]
let ``appendEvents fails with concurrency conflict`` () =
    async {
        // Arrange
        use! db = setupTestDb()
        let aggregateId = Guid.NewGuid()
        do! insertEvent db "Order" aggregateId 1 { Data = "Event1" }

        let projection = fun _ _ -> async { () }

        // Act - Try to append at version 0 (but current is 1)
        let! result = EventStore.appendEvents
            connectDb "Order" aggregateId 0
            [{ Data = "Event2" }] projection

        // Assert
        match result with
        | Error msg -> msg |> should contain "Concurrency conflict"
        | Ok _ -> Assert.Fail("Should have failed with concurrency conflict")
    } |> Async.RunSynchronously
```

**Acceptance Criteria:**
- Tests written and compile (but fail)
- Optimistic concurrency tests cover race conditions
- Projection integration tests verify transactional behavior

---

#### 1.10: Implement EventStore.appendEvents with TransactionScope integration ✅
**Type:** Implementation
**Approach:** Test-Driven (make tests from 1.9 pass)
**Status:** ✅ Complete

**Implementation:**
```fsharp
open System.Transactions
open Npgsql

let appendEvents<'EventData>
    (connectDb: unit -> Async<IDbConnection>)
    (aggregateType: string)
    (aggregateId: Guid)
    (expectedVersion: int)
    (events: 'EventData list)
    (project: 'EventData -> IDbConnection -> Async<unit>)
    : Async<Result<unit, string>> =
    async {
        use transaction = new TransactionScope(
            TransactionScopeAsyncFlowOption.Enabled)

        try
            use! db = connectDb()

            // Append each event
            for i, event in List.indexed events do
                let version = expectedVersion + i + 1
                let eventId = Guid.NewGuid()
                let eventData = serialize event

                do! db.Execute(
                    "INSERT INTO events (
                        event_id, aggregate_type, aggregate_id,
                        event_type, event_data, version, occurred_at
                     ) VALUES (
                        @eventId, @aggregateType, @aggregateId,
                        @eventType, @eventData::jsonb, @version, @occurredAt
                     )",
                    {| eventId = eventId
                       aggregateType = aggregateType
                       aggregateId = aggregateId
                       eventType = typeof<'EventData>.Name
                       eventData = eventData
                       version = version
                       occurredAt = DateTime.UtcNow |}
                )

                // Execute projection inline (same transaction)
                do! project event db

            transaction.Complete()
            return Ok ()

        with
        | :? PostgresException as ex when ex.SqlState = "23505" ->
            return Error "Concurrency conflict: another process modified this aggregate"
        | ex ->
            return Error $"Failed to append events: {ex.Message}"
    }
```

**Acceptance Criteria:**
- All tests from task 1.9 pass
- Uses TransactionScope (matches existing handler pattern)
- Handles unique constraint violation → concurrency conflict
- Projections execute inline within same transaction
- Transaction rolls back on any failure

---

#### 1.11: Add EventStore compile items to Web.fsproj ✅
**Type:** Configuration
**Approach:** Manual
**Status:** ✅ Complete

**Edit `Web.fsproj`:**
```xml
<!-- After PostgresPersistence section -->
<Compile Include="EventStore\EventStore.Types.fs" />
<Compile Include="EventStore\EventStore.Serialization.fs" />
<Compile Include="EventStore\EventStore.Core.fs" />
```

**Order matters:**
1. Types (no dependencies)
2. Serialization (depends on Types)
3. Core (depends on Types and Serialization)

**Acceptance Criteria:**
- Project compiles successfully
- EventStore modules accessible from other parts of Web project
- Compilation order enforced (dependencies before dependents)

---

#### 1.12: Verify 100% test coverage for EventStore package ✅
**Type:** Quality Gate
**Approach:** Coverage Analysis
**Status:** ✅ Complete (14 integration tests passing)

**Actions:**
1. Run test coverage tool (dotnet-coverage or similar)
2. Generate coverage report for EventStore namespace
3. Verify all lines covered by tests
4. Document any intentional exclusions (e.g., throw statements)

**Required Coverage:**
- EventStore.Types: 100% (all type definitions used in tests)
- EventStore.Serialization: 100% (serialize/deserialize tested)
- EventStore.Core: 100% (all functions have integration tests)

**Acceptance Criteria:**
- Coverage report shows 100% for all EventStore modules
- All edge cases tested (empty events, concurrency conflicts, errors)
- Integration tests run against real PostgreSQL (Testcontainers)

---

### Phase 1 Completion Summary

**✅ Status:** COMPLETE (2026-01-05)

**Implementation Details:**
- **Files Created:**
  - `Web/EventStore/EventStore.Types.fs` - Error types (AppendError)
  - `Web/EventStore/EventStore.Serialization.fs` - JSON serialization with Thoth.Json.Net
  - `Web/EventStore/EventStore.Core.fs` - Core functions (loadEvents, getCurrentVersion, appendEvents)
  - `Web/EventStore/Database/migrations.sql` - Events table schema with optimistic concurrency
  - `Tests/EventStore/EventStoreTests.fs` - 14 comprehensive integration tests

**Test Coverage:**
- ✅ 14/14 integration tests passing
- ✅ All core functions tested (loadEvents, getCurrentVersion, appendEvents)
- ✅ Optimistic concurrency control verified
- ✅ TransactionScope integration tested
- ✅ Projection inline execution tested
- ✅ Error handling (concurrency conflicts, projection failures) tested

**Key Features Delivered:**
- Generic event storage for any aggregate type
- Optimistic concurrency control with version checking
- Transactional consistency (event append + projection in single transaction)
- PostgreSQL JSONB-based event storage
- Comprehensive error handling and recovery

**Production Readiness:** ✅ Package is production-ready and available for use by Organizations vertical slice and future domains.

---

## PHASE 1.5: EventSourcing Patterns Module

**Goal:** Create shared EventSourcing package with generic patterns for command handling, feature flags, and projection composition

**Duration Estimate:** 3-5 days
**Tasks:** 8

### Phase Overview

Extract reusable event sourcing patterns into a shared module that any vertical slice can use. This prevents duplication when future slices (Applications, Donors, Distributions) adopt event sourcing.

**Architecture:**
```
Web/EventSourcing/          # Generic patterns (reusable)
├── CommandHandler.fs       # Generic load→decide→append orchestration
├── FeatureFlag.fs         # Percentage-based rollout with deterministic hashing
└── Projection.fs          # Composition helpers and utilities

Web/Organizations/EventSourcing/   # Domain-specific (Organizations only)
├── Events.fs, Commands.fs         # Organization events and commands
├── Decision.fs, Evolution.fs      # Domain logic (decide, evolve, replay)
└── Projections.fs                 # SQL updates for organizacje table
```

**Key Principle:** Generic infrastructure (EventStore + EventSourcing) should be agnostic to domain concepts. Domain-specific logic (Organizations) should use generic infrastructure but remain independent.

### Tasks

#### 1.5.1: Create EventSourcing module structure
**Type:** Setup
**Approach:** Manual

- Create folder: `/OperatorPortal/Web/EventSourcing/`
- Create empty files:
  - `CommandHandler.fs`
  - `FeatureFlag.fs`
  - `Projection.fs`
- Define module namespaces: `EventSourcing.CommandHandler`, `EventSourcing.FeatureFlag`, `EventSourcing.Projection`

**Acceptance Criteria:**
- Folder structure created
- Files compile (empty modules)
- Module namespaces follow convention

---

#### 1.5.2: TDD - Write tests for CommandHandler.handleCommand
**Type:** Test
**Approach:** Test-First (TDD)

**Create test file:** `/OperatorPortal/Tests/EventSourcing/CommandHandlerTests.fs`

**Test Cases:**
1. `handleCommand succeeds when decide returns events and append succeeds`
2. `handleCommand returns error when decide function rejects command`
3. `handleCommand handles optimistic concurrency conflict from EventStore`
4. `handleCommand handles projection failure error`
5. `handleCommand works with empty event stream (new aggregate)`
6. `handleCommand replays events correctly before calling decide`
7. `handleCommand extracts aggregate ID and converts to Guid correctly`

**Example Test:**
```fsharp
[<Fact>]
let ``handleCommand succeeds with valid command`` () =
    async {
        // Arrange
        let aggregateId = Guid.NewGuid()
        let events = []  // Empty stream
        let decide cmd state = Ok [TestEvent { Data = "test" }]
        let replay evts = None
        let project evt db = async { () }

        let config = {
            LoadEvents = fun _ -> async { return events }
            Replay = replay
            Decide = decide
            Project = project
            AppendEvents = fun _ _ _ -> async { return Ok () }
            ToGuid = fun id -> aggregateId
            GetAggregateId = fun cmd -> "test-id"
            AggregateType = "Test"
        }

        // Act
        let! result = CommandHandler.handleCommand config { TestCommand = "test" }

        // Assert
        match result with
        | Ok () -> ()  // Success
        | Error msg -> Assert.Fail($"Expected success, got error: {msg}")
    } |> Async.RunSynchronously
```

**Acceptance Criteria:**
- Tests written and compile (but fail - implementation doesn't exist yet)
- All edge cases covered (new aggregate, existing aggregate, errors)
- Tests verify full orchestration flow

---

#### 1.5.3: Implement CommandHandler.fs
**Type:** Implementation
**Approach:** Test-Driven (make tests from 1.5.2 pass)

**Implementation:**
```fsharp
module EventSourcing.CommandHandler

open System
open System.Data
open EventStore.Core
open EventStore.Types

type CommandHandlerConfig<'State, 'Command, 'Event, 'AggregateId> = {
    LoadEvents: Guid -> Async<'Event list>
    Replay: 'Event list -> 'State option
    Decide: 'Command -> 'State option -> Result<'Event list, string>
    Project: 'Event -> IDbConnection -> Async<unit>
    AppendEvents: Guid -> int -> 'Event list -> Async<Result<unit, AppendError>>
    ToGuid: 'AggregateId -> Guid
    GetAggregateId: 'Command -> 'AggregateId
    AggregateType: string
}

let handleCommand<'State, 'Command, 'Event, 'AggregateId>
    (config: CommandHandlerConfig<'State, 'Command, 'Event, 'AggregateId>)
    (command: 'Command)
    : Async<Result<unit, string>> =
    async {
        // 1. Extract aggregate ID and convert to Guid
        let aggregateId = config.GetAggregateId command
        let guid = config.ToGuid aggregateId

        // 2. Load events
        let! events = config.LoadEvents guid

        // 3. Replay to current state
        let currentState = config.Replay events

        // 4. Get current version
        let currentVersion = events |> List.length

        // 5. Decide what events to produce
        match config.Decide command currentState with
        | Error err ->
            return Error err

        | Ok newEvents ->
            // 6. Append events with inline projections
            let! appendResult = config.AppendEvents guid currentVersion newEvents

            return
                match appendResult with
                | Ok () -> Ok ()
                | Error (ConcurrencyConflict (expected, actual)) ->
                    Error $"Concurrency conflict: expected version {expected}, actual {actual}"
                | Error (ProjectionFailed (eventType, ex)) ->
                    Error $"Projection failed for {eventType}: {ex.Message}"
                | Error (DatabaseError ex) ->
                    Error $"Database error: {ex.Message}"
    }
```

**Acceptance Criteria:**
- All tests from task 1.5.2 pass
- Generic function works with any state/command/event types
- Error mapping from AppendError to string is clear
- Full orchestration flow implemented

---

#### 1.5.4: TDD - Write tests for FeatureFlag
**Type:** Test
**Approach:** Test-First (TDD)

**Create test file:** `/OperatorPortal/Tests/EventSourcing/FeatureFlagTests.fs`

**Test Cases:**
1. `isEnabled returns false for all IDs when percentage is 0`
2. `isEnabled returns true for all IDs when percentage is 100`
3. `isEnabled returns same result for same ID (deterministic)`
4. `isEnabled distributes roughly evenly at 50%`
5. `Environment variable overrides config percentage`
6. `executeWithFeatureFlag routes to ES handler when enabled`
7. `executeWithFeatureFlag routes to legacy handler when disabled`

**Example Test:**
```fsharp
[<Fact>]
let ``isEnabled is deterministic for same ID`` () =
    // Arrange
    let config = { Percentage = 50; EnvVarName = "TEST_FLAG" }
    let aggregateId = "test-123"

    // Act
    let result1 = FeatureFlag.isEnabled config aggregateId
    let result2 = FeatureFlag.isEnabled config aggregateId
    let result3 = FeatureFlag.isEnabled config aggregateId

    // Assert
    result1 |> should equal result2
    result2 |> should equal result3
```

**Acceptance Criteria:**
- Tests written and compile (but fail)
- Determinism and distribution tested
- Environment variable override tested

---

#### 1.5.5: Implement FeatureFlag.fs
**Type:** Implementation
**Approach:** Test-Driven (make tests from 1.5.4 pass)

**Implementation:**
```fsharp
module EventSourcing.FeatureFlag

open System
open System.Security.Cryptography
open System.Text

type FeatureFlagConfig = {
    Percentage: int  // 0-100
    EnvVarName: string
}

let isEnabled<'AggregateId>
    (config: FeatureFlagConfig)
    (aggregateId: 'AggregateId)
    : bool =

    // 1. Get percentage from environment or config
    let percentage =
        match Environment.GetEnvironmentVariable(config.EnvVarName) with
        | null | "" -> config.Percentage
        | value ->
            match Int32.TryParse(value) with
            | true, pct -> pct
            | false, _ -> config.Percentage

    // Edge cases
    if percentage <= 0 then false
    elif percentage >= 100 then true
    else
        // 2. Deterministic hash of aggregate ID
        let idString = aggregateId.ToString()
        use sha256 = SHA256.Create()
        let hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(idString))
        let hashInt = BitConverter.ToInt32(hashBytes, 0) |> abs

        // 3. Modulo 100 to get bucket (0-99)
        let bucket = hashInt % 100

        // 4. Enable if bucket < percentage
        bucket < percentage

let executeWithFeatureFlag<'T, 'AggregateId>
    (config: FeatureFlagConfig)
    (aggregateId: 'AggregateId)
    (esHandler: unit -> Async<Result<'T, string>>)
    (legacyHandler: unit -> Async<Result<'T, string>>)
    : Async<Result<'T, string>> =

    if isEnabled config aggregateId then
        esHandler()
    else
        legacyHandler()
```

**Acceptance Criteria:**
- All tests from task 1.5.4 pass
- SHA-256 ensures deterministic hashing
- Environment variable override works
- Generic function works with any aggregate ID type

---

#### 1.5.6: TDD - Write tests for Projection helpers
**Type:** Test
**Approach:** Test-First (TDD)

**Create test file:** `/OperatorPortal/Tests/EventSourcing/ProjectionTests.fs`

**Test Cases:**
1. `combine executes all projections in sequence`
2. `combine stops on first projection failure`
3. `forEvent only executes handler when matcher returns true`
4. `forEvent skips handler when matcher returns false`
5. `withLogging logs start and success`
6. `withLogging logs failure and rethrows`
7. `noop projection always succeeds`

**Example Test:**
```fsharp
[<Fact>]
let ``combine executes all projections`` () =
    async {
        // Arrange
        let mutable projection1Called = false
        let mutable projection2Called = false

        let projection1 evt db = async { projection1Called <- true }
        let projection2 evt db = async { projection2Called <- true }

        let combined = Projection.combine [projection1; projection2]

        // Act
        use db = createMockDb()
        do! combined TestEvent db

        // Assert
        projection1Called |> should be True
        projection2Called |> should be True
    } |> Async.RunSynchronously
```

**Acceptance Criteria:**
- Tests written and compile (but fail)
- All composition patterns tested
- Logging decorator tested

---

#### 1.5.7: Implement Projection.fs
**Type:** Implementation
**Approach:** Test-Driven (make tests from 1.5.6 pass)

**Implementation:**
```fsharp
module EventSourcing.Projection

open System.Data

type ProjectionFn<'Event> = 'Event -> IDbConnection -> Async<unit>

let combine<'Event>
    (projections: ProjectionFn<'Event> list)
    : ProjectionFn<'Event> =
    fun event db ->
        async {
            for projection in projections do
                do! projection event db
        }

let forEvent<'Event>
    (matcher: 'Event -> bool)
    (handler: 'Event -> IDbConnection -> Async<unit>)
    : ProjectionFn<'Event> =
    fun event db ->
        async {
            if matcher event then
                do! handler event db
        }

let noop<'Event> : ProjectionFn<'Event> =
    fun _ _ -> async { () }

let withLogging<'Event>
    (logger: string -> unit)
    (projection: ProjectionFn<'Event>)
    : ProjectionFn<'Event> =
    fun event db ->
        async {
            let eventType = event.GetType().Name
            logger $"Projecting {eventType}"
            try
                do! projection event db
                logger $"Projected {eventType} successfully"
            with ex ->
                logger $"Projection failed for {eventType}: {ex.Message}"
                reraise()
        }
```

**Acceptance Criteria:**
- All tests from task 1.5.6 pass
- Composition functions work correctly
- Logging decorator preserves errors
- Generic functions work with any event type

---

#### 1.5.8: Add EventSourcing to Web.fsproj & verify 100% coverage
**Type:** Configuration & Quality Gate
**Approach:** Manual + Coverage Analysis

**Edit `Web.fsproj`:**
```xml
<!-- After EventStore section, before Organizations section -->
<Compile Include="EventSourcing\CommandHandler.fs" />
<Compile Include="EventSourcing\FeatureFlag.fs" />
<Compile Include="EventSourcing\Projection.fs" />
```

**Order matters:**
1. CommandHandler (depends on EventStore)
2. FeatureFlag (no EventStore dependency)
3. Projection (no EventStore dependency)

**Coverage verification:**
1. Run test coverage tool (dotnet-coverage or similar)
2. Generate coverage report for EventSourcing namespace
3. Verify 100% coverage for all three files
4. Document any intentional exclusions

**Required Coverage:**
- EventSourcing.CommandHandler: 100% (all orchestration steps tested)
- EventSourcing.FeatureFlag: 100% (all branches tested)
- EventSourcing.Projection: 100% (all helpers tested)

**Acceptance Criteria:**
- Project compiles successfully
- EventSourcing modules accessible from Organizations
- Compilation order correct
- Coverage report shows 100% for all EventSourcing modules

---

### Phase 1.5 Completion Summary

**Deliverables:**
- **Files Created:**
  - `Web/EventSourcing/CommandHandler.fs` - Generic command handler orchestration
  - `Web/EventSourcing/FeatureFlag.fs` - Percentage-based feature flag
  - `Web/EventSourcing/Projection.fs` - Projection composition helpers
  - `Tests/EventSourcing/CommandHandlerTests.fs` - Command handler tests
  - `Tests/EventSourcing/FeatureFlagTests.fs` - Feature flag tests
  - `Tests/EventSourcing/ProjectionTests.fs` - Projection helper tests

**Test Coverage:**
- ✅ 100% coverage for all EventSourcing modules
- ✅ All orchestration patterns tested
- ✅ Determinism and distribution verified
- ✅ Composition patterns tested

**Key Features Delivered:**
- Generic command handler pattern (load→replay→decide→append)
- Deterministic feature flag for gradual rollout
- Projection composition and filtering utilities
- Reusable by any vertical slice

**Production Readiness:** ✅ Package is production-ready and available for use by Organizations and future domains.

---

## PHASE 2: Domain Modeling (**UPDATED**)

**Goal:** Create Organization events, commands, aggregates, and pure functions with 100% test coverage

**Duration Estimate:** 1-2 weeks
**Tasks:** 15 (updated from 13)

### Phase Overview

Define event-sourced domain model for Organizations. Create versioned events, commands, aggregate state, pure functions (decide, evolve, replay), and infrastructure for TeczkaId → Guid mapping and synthetic event creation.

**Changes from Original Plan:**
- ➕ Added AggregateIdMapping.fs for deterministic TeczkaId → Guid conversion
- ➕ Added SyntheticEvents.fs for migration helpers (create synthetic OrganizationCreated events)
- ✅ Events.fs, Commands.fs, Aggregate.fs, Decision.fs, Evolution.fs remain unchanged

**File Structure:**
```
Web/Organizations/EventSourcing/
├── Events.fs               # 6 versioned event types (V1)
├── Commands.fs             # OrganizationCommand types
├── Aggregate.fs            # OrganizationState record
├── Decision.fs             # decide function (command → events)
├── Evolution.fs            # evolve/replay functions (events → state)
├── AggregateIdMapping.fs   # TeczkaId → Guid conversion (NEW)
└── SyntheticEvents.fs      # Migration helpers (NEW)
```

### Tasks

#### 2.1: Create Organizations/EventSourcing folder structure
**Type:** Setup
**Approach:** Manual

- Create folder: `/OperatorPortal/Web/Organizations/EventSourcing/`
- Create empty files:
  - `Events.fs`
  - `Commands.fs`
  - `Aggregate.fs`
  - `Decision.fs`
  - `Evolution.fs`
  - `AggregateIdMapping.fs` (NEW)
  - `SyntheticEvents.fs` (NEW)
- Define module namespaces: `Organizations.EventSourcing.*`

**Acceptance Criteria:**
- Folder structure created
- All files compile (empty modules)
- Namespaces follow convention

---

#### 2.2: TDD - Write unit tests for versioned event types
**Type:** Test
**Approach:** Test-First (TDD)

**Create test file:** `/OperatorPortal/Tests/Organizations/EventSourcing/EventsTests.fs`

**Test Cases:**
1. `KontaktyChangedV1 serializes and deserializes correctly`
2. `ZrodlaZywnosciChangedV1 contains all expected fields`
3. `OrganizationCreatedV1 captures complete organization state`
4. All 6 event types serialize to/from JSON

**Example Test:**
```fsharp
[<Fact>]
let ``KontaktyChangedV1 serializes correctly`` () =
    // Arrange
    let event = {
        TeczkaId = 123L
        Email = "test@example.com"
        Telefon = "123456789"
        // ... all Kontakty fields
        Who = "user@example.com"
        OccurredAt = DateTime(2026, 1, 4)
    }

    // Act
    let json = EventStore.Serialization.serialize event
    let deserialized = EventStore.Serialization.deserialize<KontaktyChangedV1> json

    // Assert
    deserialized |> should equal event
```

**Acceptance Criteria:**
- Tests compile but fail (events not defined yet)
- All 6 event types tested
- Serialization roundtrip tests included

---

#### 2.3: Create Events.fs with 6 versioned event data types (V1 schema)
**Type:** Implementation
**Approach:** Test-Driven

**Implement in `Events.fs`:**
```fsharp
module Organizations.EventSourcing.Events

// Base audit metadata for all events
type EventAudit = {
    Who: string
    OccurredAt: DateTime
}

// 1. Kontakty Changed Event
type KontaktyChangedV1 = {
    TeczkaId: int64
    Email: string
    Telefon: string
    OsobaDoKontaktu: string
    TelefonOsobyKontaktowej: string
    MailOsobyKontaktowej: string
    OsobaOdbierajacaZywnosc: string
    TelefonOsobyOdbierajacej: string
    Kontakt: string
    Przedstawiciel: string
    Dostepnosc: string
    WwwFacebook: string
    Audit: EventAudit
}

// 2. ZrodlaZywnosci Changed Event
type ZrodlaZywnosciChangedV1 = {
    TeczkaId: int64
    Bazarki: bool
    FEPZ2024: bool
    OdbiorKrotkiTermin: bool
    Machfit: bool
    Sieci: bool
    TylkoNaszMagazyn: bool
    Audit: EventAudit
}

// 3. AdresyKsiegowosci Changed Event
type AdresyKsiegowosciChangedV1 = {
    TeczkaId: int64
    KsiegowanieAdres: string
    NazwaOrganizacjiKsiegowanieDarowizn: string
    TelOrganProwadzacegoKsiegowosc: string
    Audit: EventAudit
}

// 4. DaneAdresowe Changed Event
type DaneAdresoweChangedV1 = {
    TeczkaId: int64
    AdresPlacowkiTrafiaZywnosc: string
    AdresRejestrowy: string
    NazwaOrganizacjiPodpisujacejUmowe: string
    Powiat: string
    GminaDzielnica: string
    NazwaPlacowkiTrafiaZywnosc: string
    Audit: EventAudit
}

// 5. Beneficjenci Changed Event
type BeneficjenciChangedV1 = {
    TeczkaId: int64
    Beneficjenci: string
    LiczbaBeneficjentow: int
    Audit: EventAudit
}

// 6. WarunkiPomocy Changed Event
type WarunkiPomocyChangedV1 = {
    TeczkaId: int64
    HACCP: bool
    Kategoria: string
    RodzajPomocy: string
    Sanepid: bool
    SposobUdzielaniaPomocy: string
    TransportKategoria: string
    TransportOpis: string
    WarunkiMagazynowe: string
    Audit: EventAudit
}

// Synthetic event for existing organizations
type OrganizationCreatedV1 = {
    TeczkaId: int64
    IdentyfikatorEnova: string
    NIP: string
    Regon: string
    KrsNr: string
    FormaPrawna: string
    OPP: bool
    DaneAdresowe: DaneAdresoweChangedV1
    Kontakty: KontaktyChangedV1
    ZrodlaZywnosci: ZrodlaZywnosciChangedV1
    AdresyKsiegowosci: AdresyKsiegowosciChangedV1
    Beneficjenci: BeneficjenciChangedV1
    WarunkiPomocy: WarunkiPomocyChangedV1
    Audit: EventAudit
}
```

**Acceptance Criteria:**
- All tests from 2.2 pass
- Events follow versioned naming (V1)
- Each event contains all fields for its section
- Audit metadata included in all events

---

#### 2.4: Create OrganizationEvent discriminated union
**Type:** Implementation
**Approach:** Code

**Add to `Events.fs`:**
```fsharp
// Discriminated union wrapping all event versions
type OrganizationEvent =
    | OrganizationCreated of OrganizationCreatedV1
    | KontaktyChanged of KontaktyChangedV1
    | ZrodlaZywnosciChanged of ZrodlaZywnosciChangedV1
    | AdresyKsiegowosciChanged of AdresyKsiegowosciChangedV1
    | DaneAdresoweChanged of DaneAdresoweChangedV1
    | BeneficjenciChanged of BeneficjenciChangedV1
    | WarunkiPomocyChanged of WarunkiPomocyChangedV1

// Helper to extract event type name for persistence
module OrganizationEvent =
    let getEventType = function
        | OrganizationCreated _ -> "OrganizationCreatedV1"
        | KontaktyChanged _ -> "KontaktyChangedV1"
        | ZrodlaZywnosciChanged _ -> "ZrodlaZywnosciChangedV1"
        | AdresyKsiegowosciChanged _ -> "AdresyKsiegowosciChangedV1"
        | DaneAdresoweChanged _ -> "DaneAdresoweChangedV1"
        | BeneficjenciChanged _ -> "BeneficjenciChangedV1"
        | WarunkiPomocyChanged _ -> "WarunkiPomocyChangedV1"

    let getAudit = function
        | OrganizationCreated e -> e.Audit
        | KontaktyChanged e -> e.Audit
        | ZrodlaZywnosciChanged e -> e.Audit
        | AdresyKsiegowosciChanged e -> e.Audit
        | DaneAdresoweChanged e -> e.Audit
        | BeneficjenciChanged e -> e.Audit
        | WarunkiPomocyChanged e -> e.Audit
```

**Acceptance Criteria:**
- Discriminated union wraps all V1 events
- Helper functions extract metadata
- Pattern matches force exhaustive handling

---

#### 2.5: Create Commands.fs with OrganizationCommand types
**Type:** Implementation
**Approach:** Code

**Create `Commands.fs`:**
```fsharp
module Organizations.EventSourcing.Commands

open Organizations.Domain

// Commands mirror existing Application/Commands.fs DTOs
// but use domain types instead of primitives

type CreateOrganization = {
    Teczka: TeczkaId
    IdentyfikatorEnova: string
    NIP: Nip
    Regon: Regon
    FormaPrawna: FormaPrawna
    OPP: bool
    DaneAdresowe: DaneAdresowe
    Kontakty: Kontakty
    ZrodlaZywnosci: ZrodlaZywnosci
    AdresyKsiegowosci: AdresyKsiegowosci
    Beneficjenci: Beneficjenci
    WarunkiPomocy: WarunkiPomocy
}

type ChangeKontakty = {
    TeczkaId: TeczkaId
    Kontakty: Kontakty
}

type ChangeZrodlaZywnosci = {
    TeczkaId: TeczkaId
    ZrodlaZywnosci: ZrodlaZywnosci
}

type ChangeAdresyKsiegowosci = {
    TeczkaId: TeczkaId
    AdresyKsiegowosci: AdresyKsiegowosci
}

type ChangeDaneAdresowe = {
    TeczkaId: TeczkaId
    DaneAdresowe: DaneAdresowe
}

type ChangeBeneficjenci = {
    TeczkaId: TeczkaId
    Beneficjenci: Beneficjenci
}

type ChangeWarunkiPomocy = {
    TeczkaId: TeczkaId
    WarunkiPomocy: WarunkiPomocy
}

// Discriminated union of all commands
type OrganizationCommand =
    | CreateOrganization of CreateOrganization
    | ChangeKontakty of ChangeKontakty
    | ChangeZrodlaZywnosci of ChangeZrodlaZywnosci
    | ChangeAdresyKsiegowosci of ChangeAdresyKsiegowosci
    | ChangeDaneAdresowe of ChangeDaneAdresowe
    | ChangeBeneficjenci of ChangeBeneficjenci
    | ChangeWarunkiPomocy of ChangeWarunkiPomocy
```

**Acceptance Criteria:**
- Commands use domain types (TeczkaId, Nip, etc.)
- Commands mirror existing handler signatures
- Discriminated union enables pattern matching

---

#### 2.6: Create Aggregate.fs with OrganizationState record
**Type:** Implementation
**Approach:** Code

**Create `Aggregate.fs`:**
```fsharp
module Organizations.EventSourcing.Aggregate

open Organizations.Domain

type OrganizationState = {
    Teczka: TeczkaId
    IdentyfikatorEnova: string
    NIP: Nip
    Regon: Regon
    FormaPrawna: FormaPrawna
    OPP: bool
    DaneAdresowe: DaneAdresowe
    Kontakty: Kontakty
    ZrodlaZywnosci: ZrodlaZywnosci
    AdresyKsiegowosci: AdresyKsiegowosci
    Beneficjenci: Beneficjenci
    WarunkiPomocy: WarunkiPomocy
    Version: int  // For optimistic concurrency
}

module OrganizationState =
    let empty teczkaId = {
        Teczka = teczkaId
        IdentyfikatorEnova = ""
        NIP = Nip.create "" |> Result.defaultValue (Nip.create "0000000000" |> Result.get)
        Regon = Regon.create "" |> Result.defaultValue (Regon.create "000000000" |> Result.get)
        FormaPrawna = FormaPrawna.create "" (Krs.create "" |> Result.toOption)
        OPP = false
        DaneAdresowe = {
            NazwaOrganizacjiPodpisujacejUmowe = ""
            AdresRejestrowy = ""
            NazwaPlacowkiTrafiaZywnosc = ""
            AdresPlacowkiTrafiaZywnosc = ""
            GminaDzielnica = ""
            Powiat = ""
        }
        Kontakty = {
            WwwFacebook = ""
            Telefon = ""
            Przedstawiciel = ""
            Kontakt = ""
            Email = ""
            Dostepnosc = ""
            OsobaDoKontaktu = ""
            TelefonOsobyKontaktowej = ""
            MailOsobyKontaktowej = ""
            OsobaOdbierajacaZywnosc = ""
            TelefonOsobyOdbierajacej = ""
        }
        ZrodlaZywnosci = {
            Sieci = false
            Bazarki = false
            Machfit = false
            FEPZ2024 = false
            OdbiorKrotkiTermin = false
            TylkoNaszMagazyn = false
        }
        AdresyKsiegowosci = {
            NazwaOrganizacjiKsiegowanieDarowizn = ""
            KsiegowanieAdres = ""
            TelOrganProwadzacegoKsiegowosc = ""
        }
        Beneficjenci = {
            LiczbaBeneficjentow = 0
            Beneficjenci = ""
        }
        WarunkiPomocy = {
            Kategoria = ""
            RodzajPomocy = ""
            SposobUdzielaniaPomocy = ""
            WarunkiMagazynowe = ""
            HACCP = false
            Sanepid = false
            TransportOpis = ""
            TransportKategoria = ""
        }
        Version = 0
    }

    let exists state = state.Version > 0
```

**Acceptance Criteria:**
- State mirrors existing Organization domain type
- Version field added for optimistic concurrency
- Empty state helper for initialization

---

#### 2.7: TDD - Write unit tests for decide function
**Type:** Test
**Approach:** Test-First (TDD)

**Create test file:** `/OperatorPortal/Tests/Organizations/EventSourcing/DecisionTests.fs`

**Test Cases:**
1. `CreateOrganization command produces OrganizationCreated event`
2. `ChangeKontakty command produces KontaktyChanged event`
3. `ChangeKontakty on non-existent organization returns error`
4. All 6 change commands produce correct events
5. Commands with invalid data return validation errors
6. Commands on existing organizations succeed

**Example Test:**
```fsharp
[<Fact>]
let ``ChangeKontakty produces KontaktyChanged event`` () =
    // Arrange
    let existingState = Some {
        OrganizationState.empty (TeczkaId.create 123L)
        with Version = 1
    }
    let command = ChangeKontakty {
        TeczkaId = TeczkaId.create 123L
        Kontakty = {
            Email = "new@example.com"
            Telefon = "987654321"
            // ... all fields
        }
    }
    let audit = { Who = "user@test.com"; OccurredAt = DateTime.UtcNow }

    // Act
    let result = decide command existingState audit

    // Assert
    match result with
    | Ok [ KontaktyChanged event ] ->
        event.Email |> should equal "new@example.com"
        event.Audit.Who |> should equal "user@test.com"
    | _ -> Assert.Fail("Expected KontaktyChanged event")
```

**Acceptance Criteria:**
- Tests compile but fail (decide not implemented)
- All 6 command types tested
- Validation error cases tested
- Audit metadata included in events

---

#### 2.8: Implement Decision.fs with decide function
**Type:** Implementation
**Approach:** Test-Driven (make tests from 2.7 pass)

**Create `Decision.fs`:**
```fsharp
module Organizations.EventSourcing.Decision

open Organizations.EventSourcing.Commands
open Organizations.EventSourcing.Events
open Organizations.EventSourcing.Aggregate

let decide
    (command: OrganizationCommand)
    (state: OrganizationState option)
    (audit: EventAudit)
    : Result<OrganizationEvent list, string> =

    match command, state with

    // Create new organization
    | CreateOrganization cmd, None ->
        let event = OrganizationCreated {
            TeczkaId = TeczkaId.unwrap cmd.Teczka
            IdentyfikatorEnova = cmd.IdentyfikatorEnova
            NIP = Nip.unwrap cmd.NIP
            Regon = Regon.unwrap cmd.Regon
            KrsNr = "" // Extract from FormaPrawna
            FormaPrawna = "" // Convert to string
            OPP = cmd.OPP
            DaneAdresowe = mapDaneAdresowe cmd.DaneAdresowe audit
            Kontakty = mapKontakty cmd.Kontakty audit
            ZrodlaZywnosci = mapZrodlaZywnosci cmd.ZrodlaZywnosci audit
            AdresyKsiegowosci = mapAdresyKsiegowosci cmd.AdresyKsiegowosci audit
            Beneficjenci = mapBeneficjenci cmd.Beneficjenci audit
            WarunkiPomocy = mapWarunkiPomocy cmd.WarunkiPomocy audit
            Audit = audit
        }
        Ok [ event ]

    | CreateOrganization _, Some _ ->
        Error "Organization already exists"

    // Change Kontakty
    | ChangeKontakty cmd, Some org ->
        let event = KontaktyChanged {
            TeczkaId = TeczkaId.unwrap cmd.TeczkaId
            Email = cmd.Kontakty.Email
            Telefon = cmd.Kontakty.Telefon
            OsobaDoKontaktu = cmd.Kontakty.OsobaDoKontaktu
            TelefonOsobyKontaktowej = cmd.Kontakty.TelefonOsobyKontaktowej
            MailOsobyKontaktowej = cmd.Kontakty.MailOsobyKontaktowej
            OsobaOdbierajacaZywnosc = cmd.Kontakty.OsobaOdbierajacaZywnosc
            TelefonOsobyOdbierajacej = cmd.Kontakty.TelefonOsobyOdbierajacej
            Kontakt = cmd.Kontakty.Kontakt
            Przedstawiciel = cmd.Kontakty.Przedstawiciel
            Dostepnosc = cmd.Kontakty.Dostepnosc
            WwwFacebook = cmd.Kontakty.WwwFacebook
            Audit = audit
        }
        Ok [ KontaktyChanged event ]

    | ChangeKontakty _, None ->
        Error "Organization does not exist"

    // Similar patterns for other 5 change commands...

    | ChangeZrodlaZywnosci cmd, Some org ->
        // ... implementation
        Ok [ ZrodlaZywnosciChanged {...} ]

    | ChangeAdresyKsiegowosci cmd, Some org ->
        // ... implementation
        Ok [ AdresyKsiegowosciChanged {...} ]

    | ChangeDaneAdresowe cmd, Some org ->
        // ... implementation
        Ok [ DaneAdresoweChanged {...} ]

    | ChangeBeneficjenci cmd, Some org ->
        // ... implementation
        Ok [ BeneficjenciChanged {...} ]

    | ChangeWarunkiPomocy cmd, Some org ->
        // ... implementation
        Ok [ WarunkiPomocyChanged {...} ]

    | _ ->
        Error "Invalid command for current state"
```

**Acceptance Criteria:**
- All tests from 2.7 pass
- Pattern matching ensures all cases handled
- Result type for explicit error handling
- Each command produces exactly 1 event
- Validation failures return descriptive errors

---

#### 2.9: TDD - Write unit tests for evolve function
**Type:** Test
**Approach:** Test-First (TDD)

**Create test file:** `/OperatorPortal/Tests/Organizations/EventSourcing/EvolutionTests.fs`

**Test Cases:**
1. `OrganizationCreated event creates new state with version 1`
2. `KontaktyChanged event updates Kontakty section and increments version`
3. `evolve is deterministic (same event = same state)`
4. `evolve increments version correctly`
5. All 6 event types update correct sections

**Example Test:**
```fsharp
[<Fact>]
let ``KontaktyChanged updates state correctly`` () =
    // Arrange
    let initialState = {
        OrganizationState.empty (TeczkaId.create 123L)
        with
            Version = 1
            Kontakty = { Email = "old@example.com"; (* ... *) }
    }
    let event = KontaktyChanged {
        TeczkaId = 123L
        Email = "new@example.com"
        Telefon = "123456789"
        // ... all fields
        Audit = { Who = "user"; OccurredAt = DateTime.UtcNow }
    }

    // Act
    let newState = evolve (Some initialState) event

    // Assert
    newState.Kontakty.Email |> should equal "new@example.com"
    newState.Version |> should equal 2
    // Other sections unchanged
    newState.ZrodlaZywnosci |> should equal initialState.ZrodlaZywnosci
```

**Acceptance Criteria:**
- Tests compile but fail (evolve not implemented)
- All 6 event types tested
- Version increment verified
- Immutability verified (old state unchanged)

---

#### 2.10: Implement Evolution.fs with evolve and replay functions
**Type:** Implementation
**Approach:** Test-Driven (make tests from 2.9 pass)

**Create `Evolution.fs`:**
```fsharp
module Organizations.EventSourcing.Evolution

open Organizations.EventSourcing.Events
open Organizations.EventSourcing.Aggregate
open Organizations.Domain

let evolve
    (state: OrganizationState option)
    (event: OrganizationEvent)
    : OrganizationState =

    match event, state with

    // Create organization from OrganizationCreated event
    | OrganizationCreated e, None ->
        {
            Teczka = TeczkaId.create e.TeczkaId |> Result.get
            IdentyfikatorEnova = e.IdentyfikatorEnova
            NIP = Nip.create e.NIP |> Result.get
            Regon = Regon.create e.Regon |> Result.get
            FormaPrawna = FormaPrawna.create e.FormaPrawna None
            OPP = e.OPP
            DaneAdresowe = {
                NazwaOrganizacjiPodpisujacejUmowe = e.DaneAdresowe.NazwaOrganizacjiPodpisujacejUmowe
                AdresRejestrowy = e.DaneAdresowe.AdresRejestrowy
                NazwaPlacowkiTrafiaZywnosc = e.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc
                AdresPlacowkiTrafiaZywnosc = e.DaneAdresowe.AdresPlacowkiTrafiaZywnosc
                GminaDzielnica = e.DaneAdresowe.GminaDzielnica
                Powiat = e.DaneAdresowe.Powiat
            }
            Kontakty = {
                Email = e.Kontakty.Email
                Telefon = e.Kontakty.Telefon
                // ... map all fields
            }
            ZrodlaZywnosci = {
                Sieci = e.ZrodlaZywnosci.Sieci
                Bazarki = e.ZrodlaZywnosci.Bazarki
                // ... map all fields
            }
            AdresyKsiegowosci = {
                KsiegowanieAdres = e.AdresyKsiegowosci.KsiegowanieAdres
                // ... map all fields
            }
            Beneficjenci = {
                LiczbaBeneficjentow = e.Beneficjenci.LiczbaBeneficjentow
                Beneficjenci = e.Beneficjenci.Beneficjenci
            }
            WarunkiPomocy = {
                Kategoria = e.WarunkiPomocy.Kategoria
                // ... map all fields
            }
            Version = 1
        }

    // Update Kontakty section
    | KontaktyChanged e, Some org ->
        { org with
            Kontakty = {
                Email = e.Email
                Telefon = e.Telefon
                OsobaDoKontaktu = e.OsobaDoKontaktu
                TelefonOsobyKontaktowej = e.TelefonOsobyKontaktowej
                MailOsobyKontaktowej = e.MailOsobyKontaktowej
                OsobaOdbierajacaZywnosc = e.OsobaOdbierajacaZywnosc
                TelefonOsobyOdbierajacej = e.TelefonOsobyOdbierajacej
                Kontakt = e.Kontakt
                Przedstawiciel = e.Przedstawiciel
                Dostepnosc = e.Dostepnosc
                WwwFacebook = e.WwwFacebook
            }
            Version = org.Version + 1
        }

    // Similar for other 5 event types...

    | ZrodlaZywnosciChanged e, Some org ->
        { org with
            ZrodlaZywnosci = { (* ... *) }
            Version = org.Version + 1
        }

    | AdresyKsiegowosciChanged e, Some org ->
        { org with
            AdresyKsiegowosci = { (* ... *) }
            Version = org.Version + 1
        }

    | DaneAdresoweChanged e, Some org ->
        { org with
            DaneAdresowe = { (* ... *) }
            Version = org.Version + 1
        }

    | BeneficjenciChanged e, Some org ->
        { org with
            Beneficjenci = { (* ... *) }
            Version = org.Version + 1
        }

    | WarunkiPomocyChanged e, Some org ->
        { org with
            WarunkiPomocy = { (* ... *) }
            Version = org.Version + 1
        }

    // Invalid state transitions
    | OrganizationCreated _, Some _ ->
        failwith "Cannot create organization that already exists"

    | _, None ->
        failwith "Cannot apply event to non-existent organization"

// Replay events to reconstitute state
let replay (events: OrganizationEvent list) : OrganizationState option =
    match events with
    | [] -> None
    | _ ->
        events
        |> List.fold (fun state event -> Some (evolve state event)) None
```

**Acceptance Criteria:**
- All tests from 2.9 pass
- Pure function (no side effects)
- Deterministic (same events = same state)
- Version increments correctly
- Immutable updates (record with expression)

---

#### 2.11: TDD - Write property-based tests for replay determinism
**Type:** Test
**Approach:** Property Testing (FsCheck)

**Add to `EvolutionTests.fs`:**
```fsharp
open FsCheck
open FsCheck.Xunit

[<Property>]
let ``Replaying events produces same state as sequential application``
    (events: OrganizationEvent list) =
    not (List.isEmpty events) ==> lazy
        // Apply events sequentially
        let stateSequential =
            events
            |> List.fold (fun state event -> Some (evolve state event)) None

        // Replay all at once
        let stateReplayed = replay events

        stateSequential = stateReplayed

[<Property>]
let ``Replay is idempotent - replaying twice gives same result``
    (events: OrganizationEvent list) =
    not (List.isEmpty events) ==> lazy
        let replay1 = replay events
        let replay2 = replay events

        replay1 = replay2
```

**Acceptance Criteria:**
- Property tests verify replay correctness
- FsCheck generates random event sequences
- Tests pass for all generated inputs

---

#### 2.12: Implement AggregateIdMapping.fs (NEW)
**Type:** Implementation
**Approach:** Code

**Purpose:** Deterministic TeczkaId (int64) → Guid conversion for EventStore

**Implementation in `AggregateIdMapping.fs`:**
```fsharp
module Organizations.EventSourcing.AggregateIdMapping

open System
open System.Security.Cryptography
open System.Text
open Organizations.Domain.Identifiers

/// Namespace UUID for Organizations (generated once, hardcoded)
/// This ensures all TeczkaId mappings are deterministic and consistent
let private organizationNamespace =
    Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890")

/// Convert TeczkaId to deterministic Guid using UUID v5 (SHA-1 based)
/// Same TeczkaId always produces same Guid
let teczkaIdToGuid (teczkaId: TeczkaId) : Guid =
    let id = TeczkaId.unwrap teczkaId
    let name = $"Organization-{id}"
    let nameBytes = Encoding.UTF8.GetBytes(name)

    // UUID v5 = SHA-1 hash of namespace + name
    use sha1 = SHA1.Create()
    let namespaceBytes = organizationNamespace.ToByteArray()
    let combined = Array.concat [namespaceBytes; nameBytes]
    let hash = sha1.ComputeHash(combined)

    // Take first 16 bytes, set version and variant bits
    let guidBytes = hash.[0..15]
    guidBytes.[6] <- (guidBytes.[6] &&& 0x0Fuy) ||| 0x50uy // Version 5
    guidBytes.[8] <- (guidBytes.[8] &&& 0x3Fuy) ||| 0x80uy // Variant 10

    Guid(guidBytes)
```

**Unit Tests:**
```fsharp
[<Fact>]
let ``teczkaIdToGuid is deterministic`` () =
    // Arrange
    let teczkaId = TeczkaId.create 123L |> Result.get

    // Act
    let guid1 = AggregateIdMapping.teczkaIdToGuid teczkaId
    let guid2 = AggregateIdMapping.teczkaIdToGuid teczkaId
    let guid3 = AggregateIdMapping.teczkaIdToGuid teczkaId

    // Assert
    guid1 |> should equal guid2
    guid2 |> should equal guid3

[<Fact>]
let ``different TeczkaIds produce different Guids`` () =
    // Arrange
    let teczka1 = TeczkaId.create 123L |> Result.get
    let teczka2 = TeczkaId.create 456L |> Result.get

    // Act
    let guid1 = AggregateIdMapping.teczkaIdToGuid teczka1
    let guid2 = AggregateIdMapping.teczkaIdToGuid teczka2

    // Assert
    guid1 |> should not' (equal guid2)
```

**Acceptance Criteria:**
- Deterministic mapping (same TeczkaId → same Guid always)
- Different TeczkaIds produce different Guids
- Uses UUID v5 standard for consistency
- Unit tests verify determinism

---

#### 2.13: Implement SyntheticEvents.fs (NEW)
**Type:** Implementation
**Approach:** Code

**Purpose:** Create synthetic OrganizationCreated events for existing organizations during migration

**Implementation in `SyntheticEvents.fs`:**
```fsharp
module Organizations.EventSourcing.SyntheticEvents

open System
open Organizations.Domain
open Organizations.Domain.Organization
open Organizations.Domain.Identifiers
open Organizations.EventSourcing.Events
open Organizations.EventSourcing.AggregateIdMapping
open EventStore.Core

/// Create synthetic OrganizationCreated event from existing Organization
let createOrganizationCreatedEvent
    (org: Organization)
    (who: string)
    : OrganizationCreatedV1 =
    {
        TeczkaId = TeczkaId.unwrap org.Teczka
        IdentyfikatorEnova = org.IdentyfikatorEnova
        NIP = Nip.unwrap org.NIP
        Regon = Regon.unwrap org.Regon
        KrsNr =
            match org.FormaPrawna with
            | FormaPrawna.WRejestrzeKRS krs -> Krs.unwrap krs
            | FormaPrawna.PozaRejestrem _ -> ""
        FormaPrawna = FormaPrawna.toString org.FormaPrawna
        OPP = org.OPP
        DaneAdresowe = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            NazwaOrganizacjiPodpisujacejUmowe = org.DaneAdresowe.NazwaOrganizacjiPodpisujacejUmowe
            AdresRejestrowy = org.DaneAdresowe.AdresRejestrowy
            NazwaPlacowkiTrafiaZywnosc = org.DaneAdresowe.NazwaPlacowkiTrafiaZywnosc
            AdresPlacowkiTrafiaZywnosc = org.DaneAdresowe.AdresPlacowkiTrafiaZywnosc
            GminaDzielnica = org.DaneAdresowe.GminaDzielnica
            Powiat = org.DaneAdresowe.Powiat
            Audit = { Who = who; OccurredAt = DateTime.UtcNow }
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
            Audit = { Who = who; OccurredAt = DateTime.UtcNow }
        }
        ZrodlaZywnosci = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            Sieci = org.ZrodlaZywnosci.Sieci
            Bazarki = org.ZrodlaZywnosci.Bazarki
            Machfit = org.ZrodlaZywnosci.Machfit
            FEPZ2024 = org.ZrodlaZywnosci.FEPZ2024
            OdbiorKrotkiTermin = org.ZrodlaZywnosci.OdbiorKrotkiTermin
            TylkoNaszMagazyn = org.ZrodlaZywnosci.TylkoNaszMagazyn
            Audit = { Who = who; OccurredAt = DateTime.UtcNow }
        }
        AdresyKsiegowosci = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            KsiegowanieAdres = org.AdresyKsiegowosci.KsiegowanieAdres
            NazwaOrganizacjiKsiegowanieDarowizn = org.AdresyKsiegowosci.NazwaOrganizacjiKsiegowanieDarowizn
            TelOrganProwadzacegoKsiegowosc = org.AdresyKsiegowosci.TelOrganProwadzacegoKsiegowosc
            Audit = { Who = who; OccurredAt = DateTime.UtcNow }
        }
        Beneficjenci = {
            TeczkaId = TeczkaId.unwrap org.Teczka
            LiczbaBeneficjentow = org.Beneficjenci.LiczbaBeneficjentow
            Beneficjenci = org.Beneficjenci.Beneficjenci
            Audit = { Who = who; OccurredAt = DateTime.UtcNow }
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
            Audit = { Who = who; OccurredAt = DateTime.UtcNow }
        }
        Audit = { Who = who; OccurredAt = DateTime.UtcNow }
    }

/// Backfill existing organization with synthetic event (for migration)
let backfillOrganization
    (connectDb: unit -> Async<IDbConnection>)
    (org: Organization)
    (projection: OrganizationEvent -> IDbConnection -> Async<unit>)
    : Async<Result<unit, string>> =
    async {
        let guid = teczkaIdToGuid org.Teczka
        let syntheticEvent =
            OrganizationCreated (createOrganizationCreatedEvent org "migration-script")

        let! result =
            EventStore.Core.appendEvents
                connectDb
                "Organization"
                guid
                0 // New stream, version = 0
                [syntheticEvent]
                projection

        return
            match result with
            | Ok () -> Ok ()
            | Error err -> Error $"Failed to backfill: {err}"
    }
```

**Unit Tests:**
```fsharp
[<Fact>]
let ``createOrganizationCreatedEvent captures all organization data`` () =
    // Arrange
    let org = createTestOrganization()

    // Act
    let event = SyntheticEvents.createOrganizationCreatedEvent org "test-user"

    // Assert
    event.TeczkaId |> should equal (TeczkaId.unwrap org.Teczka)
    event.NIP |> should equal (Nip.unwrap org.NIP)
    event.Kontakty.Email |> should equal org.Kontakty.Email
    // ... verify all fields
```

**Acceptance Criteria:**
- Creates complete OrganizationCreated event from Organization
- Maps all domain types to event primitives correctly
- Backfill function appends synthetic event successfully
- Unit tests verify complete data capture

---

#### 2.14: Add EventSourcing compile items to Web.fsproj (renumbered from 2.12)
**Type:** Configuration
**Approach:** Manual

**Edit `Web.fsproj`:**
```xml
<!-- Organizations/EventSourcing section -->
<Compile Include="Organizations\EventSourcing\Events.fs" />
<Compile Include="Organizations\EventSourcing\Commands.fs" />
<Compile Include="Organizations\EventSourcing\Aggregate.fs" />
<Compile Include="Organizations\EventSourcing\Decision.fs" />
<Compile Include="Organizations\EventSourcing\Evolution.fs" />
<Compile Include="Organizations\EventSourcing\AggregateIdMapping.fs" />
<Compile Include="Organizations\EventSourcing\SyntheticEvents.fs" />
```

**Order:**
1. Events (no dependencies)
2. Commands (depends on Domain)
3. Aggregate (depends on Domain)
4. Decision (depends on Events, Commands, Aggregate)
5. Evolution (depends on Events, Aggregate)
6. AggregateIdMapping (depends on Domain.Identifiers)
7. SyntheticEvents (depends on Events, Domain, AggregateIdMapping, EventStore)

**Acceptance Criteria:**
- Project compiles successfully
- All EventSourcing modules accessible from Organizations
- Dependency order correct

---

#### 2.15: Verify 100% test coverage for all domain logic (renumbered from 2.13)
**Type:** Quality Gate
**Approach:** Coverage Analysis

**Actions:**
1. Run coverage report for Organizations.EventSourcing namespace
2. Verify decide function: 100% (all command types, validation paths)
3. Verify evolve function: 100% (all event types)
4. Verify replay function: 100% (empty list, single event, multiple events)

**Required Coverage:**
- Events.fs: 100% (all type constructors used)
- Commands.fs: 100% (all type constructors used)
- Aggregate.fs: 100% (empty state tested)
- Decision.fs: 100% (all pattern matches tested)
- Evolution.fs: 100% (all pattern matches tested)
- AggregateIdMapping.fs: 100% (determinism and uniqueness tested)
- SyntheticEvents.fs: 100% (event creation and backfill tested)

**Acceptance Criteria:**
- Coverage report shows 100% for all modules
- All edge cases tested
- Property tests validate replay correctness
- Deterministic mapping verified for AggregateIdMapping
- Synthetic event creation verified for SyntheticEvents

**Phase 2 Complete:** Domain model defined with pure, testable functions, plus infrastructure for TeczkaId mapping and migration.

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
