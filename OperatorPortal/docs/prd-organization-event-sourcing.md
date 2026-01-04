# PRD: Organization Event Sourcing Migration

## 1. Background

### Current State
The Organization vertical slice currently uses a traditional persistence approach where:
- Organization data is stored in a relational database
- Organizations are identified by `TeczkaId` (int64 identifier; "teczka" is domain terminology)
- Changes are made by reading current state, modifying it, and saving back
- Audit trails track changes via `findDiff` comparisons between old and new state
- Transactions ensure data consistency during updates

### Problem Statement
The current approach has limitations:
1. **Limited Historical Visibility** - Only current state is preserved; historical context requires separate audit logs
2. **Audit Trail Coupling** - Audit tracking is manually implemented for each change operation
3. **Temporal Querying Limitations** - Cannot reconstruct state at any point in time (time traveling)
4. **Business Event Capture** - Changes are recorded as state modifications, not as meaningful business events
5. **Temporal Analytics** - Cannot analyze how Organizations evolved over time

## 2. Objectives

### Primary Goal
Migrate the Organization aggregate to event sourcing to enable **temporal analytics and time traveling** - the ability to analyze Organization state at any point in time and understand how it evolved.

### Secondary Goals
- Capture all changes as immutable business events
- Automatically derive complete audit trail from events (no manual diff tracking)
- Maintain existing functionality and API contracts

### Success Criteria
1. All existing Organization change operations continue to work without API changes
2. Complete event history is preserved for every Organization
3. System can reconstruct Organization state at any point in time (time traveling capability)
4. Audit trail is automatically derived from events and used by business
5. No performance degradation under expected load (≤50 edits/second)
6. No errors or data integrity issues
7. System behaves identically to current implementation from user perspective
8. 100% test coverage maintained

## 3. Scope

### In Scope

#### 3.1 Organization Change Operations
The following operations must be event-sourced (currently in `Handlers.fs`). Each operation generates **one event per API call**:

1. **Kontakty (Contact Information)** → `KontaktyChanged` event
   - Email, Telefon, OsobaDoKontaktu, MailOsobyKontaktowej, OsobaOdbierajacaZywnosc, TelefonOsobyOdbierajacej, TelefonOsobyKontaktowej, Kontakt, Przedstawiciel, Dostepnosc, WwwFacebook

2. **ZrodlaZywnosci (Food Sources)** → `ZrodlaZywnosciChanged` event
   - Bazarki, FEPZ2024, OdbiorKrotkiTermin, Machfit, Sieci, TylkoNaszMagazyn

3. **AdresyKsiegowosci (Accounting Addresses)** → `AdresyKsiegowosciChanged` event
   - KsiegowanieAdres, NazwaOrganizacjiKsiegowanieDarowizn, TelOrganProwadzacegoKsiegowosc

4. **DaneAdresowe (Address Data)** → `DaneAdresoweChanged` event
   - AdresPlacowkiTrafiaZywnosc, AdresRejestrowy, NazwaOrganizacjiPodpisujacejUmowe, Powiat, GminaDzielnica, NazwaPlacowkiTrafiaZywnosc

5. **Beneficjenci (Beneficiaries)** → `BeneficjenciChanged` event
   - Beneficjenci, LiczbaBeneficjentow

6. **WarunkiPomocy (Aid Conditions)** → `WarunkiPomocyChanged` event
   - HACCP, Kategoria, RodzajPomocy, Sanepid, SposobUdzielaniaPomocy, TransportKategoria, TransportOpis, WarunkiMagazynowe

**Event Granularity:** One event type per handler/API call. Each event contains all fields for that section.

#### 3.2 Event Sourcing Infrastructure
Build foundational event sourcing infrastructure including:
- Event store for persisting immutable events
- Event stream management per Organization aggregate
- Projection system for maintaining current state
- Optimistic concurrency control

#### 3.3 Audit Requirements
- Every change must be traceable to who made it and when (existing `Audit` record with `Who` and `OccuredAt`)
- Event history serves as the audit trail
- Must support change comparison (what changed from previous state)

### Out of Scope
- Migration of other vertical slices (only Organization)
- Changes to existing API contracts
- Automatic retry on concurrent modification conflicts (return error to user)
- Asynchronous projections (use inline/synchronous for strong consistency)
- Temporal query UI/API endpoints (infrastructure supports it, but queries implemented in future phase)
- Recreating events from existing Audit Trail (future consideration)
- Read model optimization beyond current query capabilities
- Cross-organization analytics or aggregations

## 4. Functional Requirements

### FR-1: Event Capture
**Requirement**: Every Organization change operation must generate corresponding business event(s)

**Details**:
- Events represent what happened in business terminology
- Events are immutable once stored
- Events contain all data needed to understand the change
- Events include audit information (who, when)

**Acceptance Criteria**:
- Each of the 6 change operation types produces domain events
- Events can be stored and retrieved
- Events are never modified after creation

### FR-2: State Reconstruction
**Requirement**: System must be able to reconstruct Organization current state from event history

**Details**:
- Loading an Organization replays its events to build current state
- State reconstruction is deterministic (same events = same state)
- No state is lost compared to current system

**Acceptance Criteria**:
- Organization state can be fully reconstructed from events
- Reconstructed state matches what would be returned by current system
- All Organization fields are correctly populated

### FR-3: Audit Trail
**Requirement**: Complete audit trail of all changes must be available for business use

**Details**:
- Event history serves as audit log (used by business, not just internal ops)
- Each event captures who made the change and when
- System can show what changed (diff) for any event
- Audit trail is tamper-proof (immutable events)
- Automatically derived from events (no manual diff tracking)
- Maintains compatibility with existing `AuditTrail` format: `{ Who, OccuredAt, Kind, Diff, EntityId }`

**Acceptance Criteria**:
- Can retrieve all events for an Organization in chronological order
- Each event includes audit metadata (who, when)
- Can determine what changed in each event (diff information)
- Existing audit queries/reports continue to work without modification
- Business users can view complete change history

### FR-4: Data Consistency
**Requirement**: All consistency guarantees of current system must be preserved with strong consistency model

**Details**:
- Inline projections within same transaction (no eventual consistency)
- Transaction boundaries ensure atomic updates
- No partial updates are persisted
- Concurrency conflicts are detected via optimistic locking (version number)
- Concurrent modifications return error to user (no automatic retry)
- Current state projections are always consistent with events (read-your-writes)

**Acceptance Criteria**:
- Events and projections are saved in single transaction
- Failed operations leave no partial data
- Concurrent modifications return error with clear message
- Read-your-writes consistency is guaranteed
- After successful API call, change is immediately visible in queries

### FR-5: Command Validation
**Requirement**: Business rules and validation must be enforced before events are created

**Details**:
- Invalid commands are rejected before events are generated
- Validation considers current aggregate state
- Error messages are clear and actionable
- All current validation rules are preserved

**Acceptance Criteria**:
- Invalid commands return errors, not events
- Validation failures are clearly communicated
- All existing business rules continue to be enforced
- No invalid state can be persisted

### FR-6: Temporal Query Support
**Requirement**: Event sourcing infrastructure must enable time traveling (querying state at any point in history)

**Details**:
- Infrastructure supports reconstructing Organization state as of any timestamp
- Events can be retrieved in chronological order
- Historical state reconstruction is accurate and complete
- **Note:** Actual temporal query API endpoints are out of scope for initial release (future phase)

**Acceptance Criteria**:
- Event replay mechanism can reconstitute state from partial event stream
- Events include timestamp metadata for temporal filtering
- Event store supports loading events up to specific version/timestamp
- Time traveling capability is demonstrated in tests (even if not exposed via API)

## 5. Non-Functional Requirements

### NFR-1: Performance
**Expected Load:** ≤50 edits per second across all Organizations

**Requirements:**
- No performance degradation compared to current system under expected load
- Write operations (command handling) complete in comparable time to current system
- Read operations (queries) are not slower than current system
- Event replay is performant (sub-second for typical aggregate sizes)
- Inline projections do not create bottlenecks at expected load

**Measurement:**
- Establish performance baseline before migration
- Monitor p50, p95, p99 latencies post-migration
- Performance degradation >20% triggers investigation

### NFR-2: Reliability
- Event store has same reliability guarantees as current database
- Transaction failures result in clean rollbacks
- No data loss under any failure scenario

### NFR-3: Maintainability
- Code follows existing F# conventions and patterns
- Event sourcing patterns are clear and documented
- Testing approach is consistent with current standards
- 100% test coverage

### NFR-4: Compatibility
- Existing API contracts are unchanged
- Current clients continue to work without modification
- No breaking changes to command signatures

## 6. Technical Constraints

### TC-1: Technology Stack
- F# language
- PostgreSQL database (existing infrastructure)
- Npgsql for database access
- Existing async workflows pattern

### TC-2: Consistency Model
- Start with inline (synchronous) projections
- Strong consistency within aggregate boundary
- Single database transaction for events + projections

### TC-3: Aggregate Boundary
- Organization is the aggregate root
- One event stream per Organization (identified by TeczkaId)
- No cross-aggregate transactions

## 7. Data Requirements

### DR-1: Event Storage
- Events must be stored permanently (immutable, never deleted)
- Events must be immutable after creation
- Events must be retrievable in chronological order (by version number)
- Events must support optimistic concurrency control (unique constraint on version)
- Event ordering via version number (integer sequence)
- Timestamp included in events for metadata (but version is source of truth for ordering)

### DR-2: Projection Storage
- Current state projections must be queryable
- Projections must be rebuildable from events
- Projections maintain same schema compatibility as current system
- Projections stored in existing Organization tables (reuse current schema)

### DR-3: Event Schema
- Events contain all data needed for projection
- Events include audit metadata (Who, OccuredAt)
- Events use domain terminology (not technical jargon)
- **Event versioning:** Use versioned event types with upcasting (Strategy 2 from event-sourcing.md)
  - Initial events: V1 schema
  - Future schema changes: Create V2 event types, upcast V1→V2 in application logic
  - Example: `KontaktyChangedV1`, `KontaktyChangedV2`, discriminated union with upcast function

### DR-4: Data Retention & GDPR
- Events kept forever (permanent audit trail)
- GDPR compliance: Use tombstone pattern for deleted Organizations
  - Append `OrganizationDeleted` event (marks aggregate as deleted)
  - Event stream preserved for compliance/audit
  - Queries return "Organization not found" for deleted aggregates
  - Physical deletion out of scope for initial release

## 8. Migration Requirements

### MR-1: Existing Organizations
**Strategy:** Create synthetic "OrganizationCreated" event on first edit after migration

**Details:**
- Organizations created before event sourcing migration continue to work
- On first edit after migration:
  1. Generate synthetic `OrganizationCreated` event from current state (initialize event stream)
  2. Append the actual change event
  3. Future edits are pure event-sourced
- No bulk data migration required
- Event stream starts from first post-migration edit

**Future Consideration (Out of Scope):**
- May recreate full event history from existing Audit Trail data
- This is explicitly out of scope for initial release

### MR-2: Feature Flag Rollout
**Strategy:** 4-week gradual rollout with feature flag

**Phases:**
1. **Week 1:** Deploy with feature flag OFF (`USE_EVENT_SOURCING=false`)
   - Event sourcing infrastructure deployed but inactive
   - Smoke test in production environment
2. **Week 2:** Enable for 10% of Organizations
   - Selected TeczkaIds based on criteria (e.g., low activity, test accounts)
3. **Week 3:** Enable for 50% if metrics acceptable
   - Monitor error rates, performance, audit completeness
4. **Week 4:** Enable for 100% if metrics acceptable
   - Full rollout

**Rollback:**
- Feature flag can be flipped to `false` at any time
- Old code path retained during probationary period
- Minimum 4 weeks before removing legacy persistence code

### MR-3: Coexistence
- Event-sourced implementation developed alongside current system
- Both code paths active during migration (feature flag controlled)
- Shared database (events in new tables, projections in existing tables)

## 9. Quality Requirements

### QR-1: Testing
- 100% test coverage (mandatory)
- Unit tests for pure functions (decide, evolve)
- Integration tests for event store operations
- Tests verify event sourcing patterns are correctly implemented

### QR-2: Documentation
- Event schema is documented
- Event sourcing patterns are documented
- Migration guide for developers

## 10. Dependencies

### D-1: Event Store Infrastructure
- Event persistence layer must be built
- Event loading/appending functionality
- Concurrency control mechanism

### D-2: Projection Infrastructure
- Projection execution within transactions
- Projection rebuild capability
- Current state query support

## 11. Key Clarifications

### Business Justification
**Primary motivation:** Enable temporal analytics and time traveling to analyze how Organizations evolved over time.

### Event Design Decisions
- **Event Granularity:** One event per API call (6 event types for 6 handlers)
- **Event Versioning:** Versioned types with upcasting (V1, V2, etc.)
- **Event Ordering:** Version number (optimistic concurrency); timestamp for metadata

### Consistency Model
- **Strong consistency:** Inline projections within same transaction
- **Read-your-writes:** Guaranteed via synchronous projections
- **Concurrency:** Detected via version check; return error to user (no auto-retry)

### Migration Approach
- **Existing Organizations:** Create synthetic `OrganizationCreated` event on first edit
- **Rollout:** Feature flag with 4-week gradual rollout (10% → 50% → 100%)
- **Rollback:** Feature flag can be flipped off; old code path retained for 4+ weeks

### Performance
- **Expected load:** ≤50 edits/second
- **Target:** No performance degradation under expected load
- **Monitoring:** p50, p95, p99 latencies; alert if >20% degradation

### Temporal Queries
- **Infrastructure:** Must support time traveling (event replay to any point)
- **API Endpoints:** Out of scope for initial release (future phase)
- **Validation:** Demonstrated in tests

### Data Retention
- **Events:** Kept forever (permanent audit trail)
- **GDPR:** Tombstone pattern for deleted Organizations
- **Security:** Inherit existing authorization model

## 12. Success Metrics

### Functional Success
- ✅ All 6 Organization change operations work correctly via event sourcing
- ✅ System behaves identically to current implementation from user perspective
- ✅ No API contract changes; existing clients continue to work without modification
- ✅ Event history accurately captures all changes with complete audit metadata
- ✅ Audit trail is automatically derived from events (used by business)

### Quality Success
- ✅ 100% test coverage maintained (mandatory requirement)
- ✅ Zero production errors related to event sourcing
- ✅ No data loss or integrity issues
- ✅ All integration tests pass
- ✅ Event sourcing patterns correctly implemented per event-sourcing.md

### Performance Success
- ✅ No performance degradation under expected load (≤50 edits/second)
- ✅ Write latencies within baseline ±20%
- ✅ Read latencies within baseline ±20%
- ✅ Event replay completes in <100ms for typical Organization

### Infrastructure Success
- ✅ Time traveling capability demonstrated (can reconstruct state at any point)
- ✅ Event store handles concurrent modifications correctly (optimistic locking)
- ✅ Strong consistency maintained (read-your-writes guaranteed)
- ✅ Projections rebuildable from events

### Migration Success
- ✅ Feature flag rollout completes successfully (4-week plan)
- ✅ Existing Organizations work seamlessly (synthetic event on first edit)
- ✅ Zero downtime during migration
- ✅ Rollback capability validated

---

**Document Version**: 2.0
**Last Updated**: 2026-01-04
**Status**: Requirements Complete - Ready for Implementation Planning
