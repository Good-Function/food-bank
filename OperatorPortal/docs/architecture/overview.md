# OperatorPortal - Architecture Overview

## 1. Overall Architecture

### Technology Stack
- **Language**: F# (functional-first .NET language)
- **Framework**: ASP.NET Core (.NET 10.0)
- **Web Framework**: Oxpecker (F# web framework with HTMX support)
- **Database**: PostgreSQL with Dapper for data access
- **Storage**: Azure Blob Storage for document management
- **Authentication**: Azure AD (Microsoft Identity Web) with OpenID Connect
- **UI Approach**: Server-side rendering with HTMX for dynamic updates

### Project Structure
The solution consists of 3 projects:

1. **Web** - Main application (F# project)
2. **Tests** - Test suite
3. **Migrations** - Database migration project

### High-Level Architecture Pattern
The application follows a **Vertical Slice Architecture** with feature-based organization:

```
Web/
├── Organizations/         # Organizations feature module
├── Applications/          # Applications feature module
├── Users/                # User management module
├── Login/                # Authentication module
├── Layout/               # Shared UI components
└── PostgresPersistence/  # Shared data access utilities
```

### Core Architectural Patterns

1. **Functional Core, Imperative Shell**: Pure domain logic with side effects at the boundaries
2. **Dependency Injection via Composition Root**: Manual DI using higher-order functions
3. **CQRS-lite**: Separation of read models (queries) from write models (commands)
4. **Repository Pattern**: Data access abstraction via DAOs
5. **Transaction Script**: Command handlers orchestrate business logic
6. **Audit Trail**: Event sourcing-like audit tracking for all changes

---

## 2. Organizations Module - Detailed Analysis

The Organizations module is located at `Web/Organizations/` and follows a clean layered architecture.

### Module Structure

```
Organizations/
├── Domain/                    # Domain models and business rules
│   ├── Identifiers.fs        # Value objects with validation
│   ├── FormaPrawna.fs        # Legal form domain logic
│   └── Organization.fs       # Core domain entity
├── Application/              # Application layer
│   ├── Commands.fs           # Command DTOs
│   ├── Audit.fs              # Audit trail types
│   ├── FindDiffForAudit.fs   # Change tracking logic
│   ├── DocumentType.fs       # Document type enum
│   ├── CommandHandlers/      # Command handlers
│   │   ├── Handlers.fs       # Update handlers
│   │   ├── CreateOrganizationCommandHandler.fs
│   │   └── DocumentHandlers.fs
│   └── ReadModels/           # Query models (CQRS read side)
│       ├── OrganizationSummary.fs
│       ├── OrganizationDetails.fs
│       ├── Filter.fs
│       ├── MailingList.fs
│       └── ReadAuditTrail.fs
├── Database/                 # Persistence layer
│   ├── OrganizationsDao.fs   # Data access object
│   ├── OrganizationRow.fs    # DB row mapping
│   ├── AuditTrailDao.fs      # Audit persistence
│   ├── DateOnlyCoder.fs      # Custom type handler
│   └── migrations.sql        # DB schema
├── Templates/                # View layer (Oxpecker ViewEngine)
│   ├── DetailsTemplate.fs
│   ├── DaneAdresowe.fs
│   ├── Kontakty.fs
│   ├── Beneficjenci.fs
│   ├── Dokumenty.fs
│   ├── ZrodlaZywnosci.fs
│   ├── AdresyKsiegowosci.fs
│   ├── WarunkiPomocy.fs
│   ├── List/                 # List view components
│   └── Audit.fs
├── Router.fs                 # Main routing
├── SectionsRouter.fs         # Section-specific routing
├── DataApi.fs                # REST API endpoints
├── CompositionRoot.fs        # Dependency wiring
├── BlobStorage.fs            # Document storage
└── ClosedXmlExcelImport.fs   # Excel import
```

---

## 3. Persistence Layer Implementation

### Database Schema

#### Table: `organizacje` (PostgreSQL)
- **Primary Key**: `Teczka` (BIGINT) - Folder/File number
- **Structure**: Wide table with all organization fields
- **Documents**: Stored as JSONB column
- **Full-text search**: Uses PostgreSQL similarity functions (trigram)

**Key fields:**
- Identity: Teczka, IdentyfikatorEnova, NIP, Regon, KrsNr
- Legal form: FormaPrawna, OPP
- Address data: 6 fields for organization/facility addresses
- Contacts: 11 fields for contact information
- Beneficiaries: LiczbaBeneficjentow, Beneficjenci
- Food sources: 6 boolean flags (Sieci, Bazarki, Machfit, etc.)
- Aid conditions: 8 fields (Kategoria, HACCP, Sanepid, etc.)
- Documents: JSONB structure with 5 document types

#### Table: `audit_trail`
- Tracks all changes to organizations
- Stores field-level diffs as JSONB
- Records: who, when, what changed, entity ID, kind (section)

### Data Access Layer (DAOs)

**OrganizationsDao** (`Web/Organizations/Database/OrganizationsDao.fs`)

Functions:
- `readSummaries`: Paginated search with filtering and sorting
- `readDetailsBy`: Get organization by Teczka ID
- `readByEmail`: Look up by email address
- `readMailingListBy`: Extract email contacts
- `save`: Upsert organization (INSERT ON CONFLICT UPDATE)
- `saveMany`: Batch save for imports
- `saveDocMetadata`: Update document metadata in JSONB

**Key Features:**
1. **Fuzzy search**: Uses PostgreSQL `similarity()` for text matching (threshold 0.2)
2. **Dynamic filtering**: Builds WHERE clauses from filter parameters
3. **Dynamic sorting**: Column and direction from query params
4. **Type mapping**: Converts between Domain, ReadModel, and Row types

**AuditTrailDao** (`Web/Organizations/Database/AuditTrailDao.fs`)
- `SaveAuditTrail`: Persists change records
- `ReadAuditTrail`: Retrieves audit history with optional filtering by kind

### Type Conversions

**OrganizationRow** provides bidirectional mapping:
- `From(Organization)` → Row: Domain to DB
- `ToDomain()` → Organization: DB to Domain (strict validation)
- `ToReadModel()` → OrganizationDetails: DB to Read Model (relaxed)

---

## 4. Domain Layer

### Core Entity: Organization

**Location**: `Web/Organizations/Domain/Organization.fs`

The Organization aggregate is composed of multiple value objects:

```fsharp
type Organization = {
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
    Dokumenty: Documents
    WarunkiPomocy: WarunkiPomocy
}
```

### Value Objects (Smart Constructors)

**Location**: `Web/Organizations/Domain/Identifiers.fs`

All identifiers use private constructors with validation:

1. **TeczkaId**: Positive int64
2. **Nip**: 10 digits, validated
3. **Regon**: 9 or 14 digits
4. **Krs**: 10 digits
5. **Email**: Regex validation
6. **NotEmptyString**: Non-whitespace validation

Each has:
- `create`: Factory function returning `Result<T, Error>`
- `unwrap`: Accessor function

### FormaPrawna (Legal Form)

**Location**: `Web/Organizations/Domain/FormaPrawna.fs`

Discriminated union for registration type:
```fsharp
type Rejestracja =
    | PozaRejestrem of NumerRejestrowy: string
    | WRejestrzeKRS of Krs
```

Special handling for church organizations and simple associations that don't have KRS numbers.

### Domain Sub-aggregates

1. **DaneAdresowe**: Organization and facility addresses
2. **Kontakty**: Contact persons and methods (11 fields)
3. **Beneficjenci**: Number and description of beneficiaries
4. **ZrodlaZywnosci**: Food source flags (6 booleans)
5. **AdresyKsiegowosci**: Accounting addresses
6. **WarunkiPomocy**: Aid conditions (category, HACCP, Sanepid, transport)
7. **Documents**: 5 document types (Wniosek, Umowa, RODO, Odwiedziny, UpowaznienieDoOdbioru)

---

## 5. Application Layer - Business Capabilities

### Command Handlers

**Location**: `Web/Organizations/Application/CommandHandlers/Handlers.fs`

Pattern used: **Command + Audit** → **Result**

All handlers follow the same structure:
```fsharp
type Command<'EntityId, 'Payload> = 'EntityId * Audit * 'Payload

let changeXYZ
    (readBy: TeczkaId -> Async<Organization>)
    (save: Organization -> Async<unit>)
    (track: AuditTrail -> Async<unit>)
    : ChangeXYZ =
    fun (id, audit, cmd) ->
        asyncResult {
            let! teczkaId = id |> TeczkaId.create
            let! organization = readBy teczkaId
            use transaction = openTransaction()
            let updatedOrg = { organization with Field = newValue }
            do! save updatedOrg
            do! track { /* audit info */ }
            transaction.Complete()
        }
```

**Available Handlers:**
1. `changeDaneAdresowe`: Update address information
2. `changeKontakty`: Update contact information
3. `changeBeneficjenci`: Update beneficiary data
4. `changeZrodlaZywnosci`: Update food sources
5. `changeAdresyKsiegowosci`: Update accounting addresses
6. `changeWarunkiPomocy`: Update aid conditions

**Transaction Management:**
- Uses `System.Transactions.TransactionScope` with async flow enabled
- Coordinates database updates with audit trail writes
- Rollback on any failure

### Document Handlers

**Location**: `Web/Organizations/Application/CommandHandlers/DocumentHandlers.fs`

Handles document lifecycle:
- Upload to Azure Blob Storage
- Save metadata to database (JSONB)
- Delete from blob storage
- Generate SAS URIs for downloads (3-minute expiry)

### Import Handler

**Location**: `Web/Organizations/Application/CommandHandlers/CreateOrganizationCommandHandler.fs`

Excel import capability:
- Parses .xlsx files via ClosedXML
- Validates headers
- Row-by-row validation with error collection
- Batch save in transaction
- Returns summary with error details

### Audit System

**Location**: `Web/Organizations/Application/FindDiffForAudit.fs`

Uses F# reflection to compute field-level diffs:
```fsharp
let findDiff<'T> (oldVal: 'T) (newVal: 'T) : Map<string, DiffEntry>
```

Tracks:
- Old value
- New value
- Field type
- Who made the change
- When it occurred
- Which section (Kind: "DaneAdresowe", "Kontakty", etc.)

---

## 6. Read Models (CQRS Query Side)

### OrganizationSummary

**Location**: `Web/Organizations/Application/ReadModels/OrganizationSummary.fs`

Optimized for list view:
- 14 fields (subset of full data)
- Includes last visit date (extracted from JSONB)
- Supports pagination (50 items per page)
- Supports sorting by any column
- Supports filtering with operators (text/number)

**Query Structure:**
```fsharp
type Query = {
    SearchTerm: string
    SortBy: (QueriedColumn * Direction) option
    Pagination: Pagination
    Filters: Filter list
}
```

### OrganizationDetails

**Location**: `Web/Organizations/Application/ReadModels/OrganizationDetails.fs`

Complete view for detail page:
- All organization fields (simpler types than domain)
- Document list with dates and filenames
- Static factory methods to convert from Commands

### Filtering System

**Location**: `Web/Organizations/Application/ReadModels/Filter.fs`

Dynamic filtering with operators:
- Text operators: `ILIKE` with wildcard matching
- Number operators: `>`, `<`, `=`
- Builds WHERE clauses dynamically

### Mailing List

**Location**: `Web/Organizations/Application/ReadModels/MailingList.fs`

Extract email addresses:
- Applies same search/filter as main list
- Returns CSV-ready email list
- Identifies organizations with missing emails

---

## 7. Controllers/Endpoints

### Main Router

**Location**: `Web/Organizations/Router.fs`

Endpoints:
- `GET /`: List view (index page)
- `GET /summaries`: HTMX endpoint for table rows
- `GET /summaries/mailing-list`: Email extraction
- `GET /{id}`: Organization details
- `GET /{id}/audit-trail`: Audit history
- `GET /import`: Import page
- `POST /import/upload`: Excel upload handler

**Query Parameter Parsing:**
- Search term
- Sort column and direction
- Page number
- Dynamic filters (column_op, column)

### Sections Router

**Location**: `Web/Organizations/SectionsRouter.fs`

HTMX-based section editing:

Each section has 3 endpoints:
1. `GET /{id}/section-name`: View mode
2. `GET /{id}/section-name/edit`: Edit form (returns HTML fragment)
3. `PUT /{id}/section-name`: Update (returns updated view)

Sections:
- dane-adresowe (address data)
- kontakty (contacts)
- beneficjenci (beneficiaries)
- dokumenty (documents) - special handling for files
- zrodla-zywnosci (food sources)
- adresy-ksiegowosci (accounting addresses)
- warunki-pomocy (aid conditions)

**Security**: PUT endpoints require `EditOrganization` permission

### Data API Router

**Location**: `Web/Organizations/DataApi.fs`

REST API for external access:
- `GET /{id}/section`: JSON response for each section
- `PUT /{id}/section`: JSON update for each section
- `POST /lookup-by-email`: Find organization by email

Protected with JWT Bearer authentication (Azure AD service principal).

User identification via `X-User-Email` header for audit trail.

---

## 8. Composition Root (Dependency Injection)

**Location**: `Web/Organizations/CompositionRoot.fs`

Two dependency containers:

### Dependencies (Main App)
- Read functions (summaries, details, audit, mailing list)
- Command handlers (all 6 section update handlers)
- Document handlers (save, delete, generate URI)
- Import handler

### DataApiDependencies (API)
- Read functions (details by ID and email)
- Command handlers (same 6 handlers)
- No document/import capabilities

Wiring strategy:
```fsharp
ChangeDaneAdresowe =
    Handlers.changeDaneAdresowe
        (OrganizationsDao.readBy connectDb)      // Read function
        (OrganizationsDao.save connectDb)        // Save function
        (AuditTrailDao(...).SaveAuditTrail)      // Audit function
```

---

## 9. Business Flows

### Flow 1: View Organization List

1. User navigates to `/organizations`
2. Router serves initial page with empty table
3. HTMX triggers `GET /organizations/summaries?page=1`
4. `parseQueryParams` extracts filters/sort/search
5. `readSummaries` executes PostgreSQL query with pagination
6. Returns list of OrganizationSummary + total count
7. Template renders table rows
8. User can click column headers to sort (triggers new HTMX request)
9. User can filter by typing in column filters

### Flow 2: View Organization Details

1. User clicks organization in list → `GET /organizations/{teczka}`
2. `readDetailsBy` fetches full record from DB
3. `OrganizationRow.ToReadModel()` converts to read model
4. Template renders tabbed interface with sections
5. Each section is initially collapsed
6. Permission check determines if Edit buttons are shown

### Flow 3: Edit Organization Section

1. User clicks Edit on section → HTMX `GET /organizations/{id}/kontakty/edit`
2. `readDetailsBy` fetches current data
3. Form template rendered with antiforgery token
4. User modifies fields and submits
5. HTMX `PUT /organizations/{id}/kontakty`
6. `changeKontakty` handler:
   - Validates TeczkaId
   - Loads current Organization from DB (domain model)
   - Opens transaction
   - Creates updated Organization
   - Saves to DB
   - Computes diff via reflection
   - Saves audit trail
   - Commits transaction
7. Returns updated view HTML
8. HTMX swaps form back to view mode

### Flow 4: Upload Document

1. User clicks Edit on Dokumenty section
2. Form allows file upload + date selection + delete checkboxes
3. User submits → `PUT /organizations/{id}/dokumenty`
4. Handler:
   - Deletes files marked for deletion from blob storage
   - Uploads new files to Azure Blob Storage (container: `teczka-{id}`)
   - Updates metadata in JSONB column
5. Returns view with download links
6. Downloads use SAS URIs (3-minute expiry)

### Flow 5: Import Organizations from Excel

1. User navigates to `/organizations/import`
2. Upload form rendered
3. User selects .xlsx file → `POST /organizations/import/upload`
4. `ClosedXmlExcelImport.import`:
   - Validates 50 expected headers
   - Row-by-row parsing with validation
   - Collects errors (row number + field errors)
   - Returns valid organizations + error list
5. `importOrganizations`:
   - Opens transaction
   - Batch saves all valid organizations
   - Commits transaction
6. Returns summary: X imported, Y failed
7. Shows error details for failed rows

### Flow 6: View Audit Trail

1. User clicks Audit tab on details page
2. HTMX `GET /organizations/{id}/audit-trail`
3. Optional filter by kind (section)
4. `ReadAuditTrail` queries audit_trail table
5. Returns list of changes with diffs
6. Template renders timeline with field-level changes

### Flow 7: External API Access (Charity Portal Integration)

1. External service calls `POST /api/organizations/lookup-by-email`
2. JWT Bearer token validated (Azure AD service principal)
3. Returns organization ID and name
4. Service can then fetch details: `GET /api/organizations/{id}/kontakty`
5. Service can update: `PUT /api/organizations/{id}/kontakty`
6. Audit records "Pracownik organizacji charytatywnej" as user

---

## 10. Key Design Patterns & Principles

### Patterns Used

1. **Vertical Slice Architecture**: Features are self-contained modules
2. **Clean Architecture**: Domain → Application → Infrastructure separation
3. **CQRS**: Separate read models from write models
4. **Repository Pattern**: DAOs abstract data access
5. **Command Pattern**: Commands + Handlers for writes
6. **Value Objects**: Validated, immutable identifiers
7. **Smart Constructors**: Factory functions returning Results
8. **Composition Root**: Manual dependency wiring
9. **Railway-Oriented Programming**: Result types with asyncResult computation expression
10. **Audit Trail**: Event sourcing-lite for change tracking

### Security

- **Authentication**: Azure AD (OAuth2/OIDC)
- **Authorization**: Role-based (Admin, Editor, Reader)
- **Permission-based access**: Granular permissions (EditOrganization, etc.)
- **API Authentication**: JWT Bearer tokens
- **CSRF Protection**: Antiforgery tokens on all forms
- **Blob Access**: Time-limited SAS URIs

### Data Integrity

- **Transactions**: TransactionScope for multi-operation consistency
- **Optimistic Concurrency**: Last-write-wins (no version tracking)
- **Validation**: Domain model validation via smart constructors
- **Audit Trail**: Complete change history

### Performance Optimizations

- **Pagination**: 50 items per page
- **Lazy Loading**: Sections load on demand
- **HTMX**: Partial page updates instead of full page loads
- **Database Indexes**: Teczka as primary key
- **Fuzzy Search**: PostgreSQL trigram similarity (pg_trgm extension)
- **Batch Operations**: saveMany for imports

---

## 11. Technology Choices Rationale

- **F#**: Type safety, immutability, concise code
- **Oxpecker**: Lightweight, HTMX-friendly, F#-native
- **HTMX**: Reduces JavaScript complexity, server-side rendering
- **Dapper**: Simple, performant, no ORM overhead
- **PostgreSQL**: JSONB support, full-text search, open source
- **Azure Blob Storage**: Scalable document storage
- **TransactionScope**: Distributed transaction support

---

## Summary

The OperatorPortal Organizations module is a well-architected, feature-rich system for managing charitable organization data. It demonstrates:

1. **Clean separation of concerns** across domain, application, and infrastructure layers
2. **Functional programming principles** with immutable data and pure functions
3. **CQRS pattern** with optimized read and write models
4. **Comprehensive audit trail** for compliance and accountability
5. **Modern web architecture** with HTMX for progressive enhancement
6. **Strong typing** with validated value objects preventing invalid states
7. **Transaction safety** ensuring data consistency
8. **Flexible querying** with dynamic filtering and sorting
9. **Document management** with cloud storage integration
10. **Bulk import** capability for onboarding organizations

The codebase is maintainable, testable, and follows F# best practices throughout.
