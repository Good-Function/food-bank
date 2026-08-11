# EventSourcing Migration - Completed Phases (Archive)

This document archives completed phases of the EventSourcing migration for the OperatorPortal.
Active phases are tracked in `tasks.md`.

**Last Updated:** 2026-01-05

---

## Completion Summary

| Phase | Completed | Duration | Tests | Notes |
|-------|-----------|----------|-------|-------|
| Phase 1: EventStore Infrastructure | 2026-01-05 | 1-2 weeks | 14 integration tests | EventStore package production-ready |
| Phase 1.5: EventSourcing Patterns | 2026-01-05 | N/A | 29 tests | Generic patterns extracted |
| Phase 2: Domain Modeling | 2026-01-05 | 1-2 weeks | 27 tests | Domain model with pure functions |

**Total:** 70 tests passing across all completed phases

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

## PHASE 1.5: EventSourcing Patterns Module ✅ **COMPLETE**

**Goal:** Create shared EventSourcing package with generic patterns for command handling, feature flags, and projection composition

**Status:** ✅ Complete (2026-01-05)
**Duration:** Completed
**Tasks:** 8/8 Complete

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

**✅ Status:** COMPLETE (2026-01-05)

**Implementation Details:**
- **Files Created:**
  - `Web/EventSourcing/CommandHandler.fs` - Generic command orchestration
  - `Web/EventSourcing/FeatureFlag.fs` - SHA-256 deterministic rollout
  - `Web/EventSourcing/Projection.fs` - Composition helpers (combine, forEvent, noop, withLogging)
  - `Tests/EventSourcing/CommandHandlerTests.fs` - 8 tests
  - `Tests/EventSourcing/FeatureFlagTests.fs` - 11 tests
  - `Tests/EventSourcing/ProjectionTests.fs` - 10 tests

**Test Coverage:**
- ✅ 29/29 tests passing (8 CommandHandler + 11 FeatureFlag + 10 Projection)
- ✅ 100% coverage for all EventSourcing modules
- ✅ TDD approach: all tests written before implementation

**Key Features:**
- Generic command handler (load→replay→decide→append orchestration)
- Deterministic feature flag with environment variable override
- Projection combinators for complex projection logic
- Full compliance with software design conventions (no comments, type-safe, async-first)

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

