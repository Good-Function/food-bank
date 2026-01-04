# Event Sourcing with Inline Projections - F# Best Practices

A pragmatic guide for migrating to Event Sourcing using inline projections with idiomatic F#.

## Philosophy: Start Simple, Scale Later

**Inline projections are NOT an anti-pattern when:**
- You're starting with Event Sourcing and need to learn the patterns
- You need strong consistency guarantees (read-your-writes)
- Your write volume is reasonable (<1000 writes/sec)
- Your projections are simple and fast
- You want to avoid distributed system complexity initially

**This guide embraces inline projections as a valid first step.**

---

## Core Principles

### 1. Events are Immutable Facts
- Store what happened, not current state
- Past tense naming: `OrderPlaced`, not `PlaceOrder`
- Events represent business behavior, not CRUD operations

```fsharp
// ✅ GOOD - Behavioral events
type OrderPlacedData = {
    OrderId: OrderId
    CustomerId: CustomerId
    Items: OrderItem list
    TotalAmount: decimal
    OccurredAt: DateTime
}

type ItemAddedData = {
    OrderId: OrderId
    ItemId: ItemId
    Quantity: int
    Price: decimal
}

// ❌ BAD - CRUD-style events
type OrderUpdatedData = {
    OrderId: OrderId
    // ... entire order state
}
```

### 2. Aggregates are Consistency Boundaries
- One aggregate = one stream = one transaction boundary
- Keep aggregates small and focused
- Load full history, validate, append new events

### 3. Strong Consistency Within Transaction
- Events + projections in single database transaction
- No eventual consistency gap for inline projections
- Transaction commits or rolls back as atomic unit

### 4. Events are Your Source of Truth
- Event store is primary persistence, not an audit log
- Projections are derived views, rebuildable from events
- Never modify events after they're stored

---

## Domain Types (Single-Case Discriminated Unions)

Use single-case DUs for type safety and domain modeling:

```fsharp
// Primitive obsession: BAD
type Order = {
    Id: Guid
    CustomerId: Guid
    Items: List<Item>
}

// ✅ GOOD - Wrapped primitives
type OrderId = OrderId of Guid
type CustomerId = CustomerId of Guid
type ProductId = ProductId of Guid
type ItemId = ItemId of Guid
type Quantity = Quantity of int
type Money = Money of decimal

module OrderId =
    let create () = Guid.NewGuid() |> OrderId
    let value (OrderId id) = id
    let toString (OrderId id) = id.ToString()

module Quantity =
    let create n =
        if n > 0 then Some (Quantity n)
        else None
    let value (Quantity q) = q

module Money =
    let create amount =
        if amount >= 0m then Some (Money amount)
        else None
    let value (Money m) = m
    let add (Money a) (Money b) = Money (a + b)
    let multiply (Money m) (Quantity q) = Money (m * decimal q)
```

---

## Domain Events (Discriminated Unions)

```fsharp
// Event data types
type OrderPlacedData = {
    OrderId: OrderId
    CustomerId: CustomerId
    Items: OrderItem list
    TotalAmount: Money
    OccurredAt: DateTime
}

and OrderItem = {
    ItemId: ItemId
    ProductId: ProductId
    Quantity: Quantity
    Price: Money
}

type OrderConfirmedData = {
    OrderId: OrderId
    ConfirmedAt: DateTime
}

type OrderShippedData = {
    OrderId: OrderId
    TrackingNumber: string
    ShippedAt: DateTime
}

type OrderCancelledData = {
    OrderId: OrderId
    Reason: string
    CancelledAt: DateTime
}

// Discriminated union of all order events
type OrderEvent =
    | OrderPlaced of OrderPlacedData
    | OrderConfirmed of OrderConfirmedData
    | OrderShipped of OrderShippedData
    | OrderCancelled of OrderCancelledData

// Event envelope for persistence
type DomainEvent<'EventData> = {
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
```

---

## Commands (Discriminated Unions)

```fsharp
type PlaceOrderData = {
    OrderId: OrderId
    CustomerId: CustomerId
    Items: OrderItem list
}

type OrderCommand =
    | PlaceOrder of PlaceOrderData
    | ConfirmOrder
    | ShipOrder of trackingNumber: string
    | CancelOrder of reason: string
```

---

## Aggregate State

```fsharp
type OrderStatus =
    | Draft
    | Placed
    | Confirmed
    | Shipped
    | Cancelled

type Order = {
    Id: OrderId
    CustomerId: CustomerId
    Items: OrderItem list
    Status: OrderStatus
    Version: int
}

module Order =
    // Create empty order
    let empty id = {
        Id = id
        CustomerId = CustomerId Guid.Empty
        Items = []
        Status = Draft
        Version = 0
    }
    
    // Check if order exists
    let exists order = order.Version > 0
```

---

## Decision Logic (Pure Functions)

```fsharp
module OrderDecision =
    
    // Validate and produce events from command
    let decide (command: OrderCommand) (order: Order option) : Result<OrderEvent list, string> =
        match command, order with
        
        // Place new order
        | PlaceOrder data, None ->
            // Validation
            if List.isEmpty data.Items then
                Error "Order must have at least one item"
            else
                let totalAmount =
                    data.Items
                    |> List.map (fun item -> Money.multiply item.Price item.Quantity)
                    |> List.fold Money.add (Money 0m)
                
                let event = OrderPlaced {
                    OrderId = data.OrderId
                    CustomerId = data.CustomerId
                    Items = data.Items
                    TotalAmount = totalAmount
                    OccurredAt = DateTime.UtcNow
                }
                Ok [ event ]
        
        // Can't place order that already exists
        | PlaceOrder _, Some _ ->
            Error "Order already exists"
        
        // Confirm placed order
        | ConfirmOrder, Some order when order.Status = Placed ->
            let event = OrderConfirmed {
                OrderId = order.Id
                ConfirmedAt = DateTime.UtcNow
            }
            Ok [ event ]
        
        | ConfirmOrder, Some order ->
            Error $"Cannot confirm order in {order.Status} status"
        
        | ConfirmOrder, None ->
            Error "Order does not exist"
        
        // Ship confirmed order
        | ShipOrder trackingNumber, Some order when order.Status = Confirmed ->
            let event = OrderShipped {
                OrderId = order.Id
                TrackingNumber = trackingNumber
                ShippedAt = DateTime.UtcNow
            }
            Ok [ event ]
        
        | ShipOrder _, Some order ->
            Error $"Cannot ship order in {order.Status} status"
        
        | ShipOrder _, None ->
            Error "Order does not exist"
        
        // Cancel order (only if not shipped)
        | CancelOrder reason, Some order ->
            match order.Status with
            | Shipped ->
                Error "Cannot cancel shipped order"
            | Cancelled ->
                Error "Order already cancelled"
            | _ ->
                let event = OrderCancelled {
                    OrderId = order.Id
                    Reason = reason
                    CancelledAt = DateTime.UtcNow
                }
                Ok [ event ]
        
        | CancelOrder _, None ->
            Error "Order does not exist"
```

**Key principles:**
- Pure function: no side effects
- Pattern matching: compiler ensures all cases handled
- Result type: explicit error handling
- All validation before creating events
- Returns list of events (usually just one, but allows for multiple)

---

## Evolution Logic (Pure Functions)

```fsharp
module OrderEvolution =
    
    // Apply single event to evolve state
    let evolve (state: Order option) (event: OrderEvent) : Order =
        match event, state with
        
        | OrderPlaced data, None ->
            {
                Id = data.OrderId
                CustomerId = data.CustomerId
                Items = data.Items
                Status = Placed
                Version = 1
            }
        
        | OrderConfirmed _, Some order ->
            { order with 
                Status = Confirmed
                Version = order.Version + 1
            }
        
        | OrderShipped data, Some order ->
            { order with 
                Status = Shipped
                Version = order.Version + 1
            }
        
        | OrderCancelled _, Some order ->
            { order with 
                Status = Cancelled
                Version = order.Version + 1
            }
        
        // Invalid state transitions (should never happen if decide is correct)
        | OrderPlaced _, Some _ ->
            failwith "Cannot place order that already exists"
        
        | _, None ->
            failwith "Cannot apply event to non-existent order"
    
    // Fold events to reconstitute current state
    let replay (events: OrderEvent list) : Order option =
        match events with
        | [] -> None
        | _ -> 
            events
            |> List.fold (fun state event -> Some (evolve state event)) None
```

**Key principles:**
- Pure function: deterministic
- Fold pattern: standard functional approach
- No validation: events are facts, always valid
- Matches decide logic: symmetric relationship

---

## Database Schema

```sql
-- Event Store table
CREATE TABLE events (
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

-- Projection tables (keep your existing schema)
CREATE TABLE orders (
    order_id UUID PRIMARY KEY,
    customer_id UUID NOT NULL,
    status VARCHAR(50) NOT NULL,
    total_amount DECIMAL(10,2),
    created_at TIMESTAMP,
    updated_at TIMESTAMP
);

CREATE TABLE order_items (
    id BIGSERIAL PRIMARY KEY,
    order_id UUID REFERENCES orders(order_id),
    product_id UUID,
    quantity INTEGER,
    price DECIMAL(10,2)
);
```

---

## Event Store Implementation

```fsharp
module EventStore =
    open Npgsql
    open System.Text.Json
    
    // Serialization helpers
    let private serializeEvent (event: OrderEvent) : string =
        JsonSerializer.Serialize(event)
    
    let private deserializeEvent (json: string) : OrderEvent =
        JsonSerializer.Deserialize<OrderEvent>(json)
    
    let private getEventType (event: OrderEvent) : string =
        match event with
        | OrderPlaced _ -> "OrderPlaced"
        | OrderConfirmed _ -> "OrderConfirmed"
        | OrderShipped _ -> "OrderShipped"
        | OrderCancelled _ -> "OrderCancelled"
    
    // Load events for an aggregate
    let loadEvents 
        (connectionString: string)
        (aggregateType: string)
        (aggregateId: Guid)
        : Async<OrderEvent list> =
        async {
            use connection = new NpgsqlConnection(connectionString)
            do! connection.OpenAsync() |> Async.AwaitTask
            
            use command = new NpgsqlCommand(
                "SELECT event_data, version 
                 FROM events 
                 WHERE aggregate_type = @aggregateType 
                   AND aggregate_id = @aggregateId
                 ORDER BY version ASC",
                connection)
            
            command.Parameters.AddWithValue("aggregateType", aggregateType) |> ignore
            command.Parameters.AddWithValue("aggregateId", aggregateId) |> ignore
            
            use! reader = command.ExecuteReaderAsync() |> Async.AwaitTask
            
            let events = ResizeArray<OrderEvent>()
            while! reader.ReadAsync() |> Async.AwaitTask do
                let json = reader.GetString(0)
                let event = deserializeEvent json
                events.Add(event)
            
            return List.ofSeq events
        }
    
    // Get current version for optimistic concurrency
    let getCurrentVersion
        (connectionString: string)
        (aggregateType: string)
        (aggregateId: Guid)
        : Async<int> =
        async {
            use connection = new NpgsqlConnection(connectionString)
            do! connection.OpenAsync() |> Async.AwaitTask
            
            use command = new NpgsqlCommand(
                "SELECT COALESCE(MAX(version), 0) 
                 FROM events 
                 WHERE aggregate_type = @aggregateType 
                   AND aggregate_id = @aggregateId",
                connection)
            
            command.Parameters.AddWithValue("aggregateType", aggregateType) |> ignore
            command.Parameters.AddWithValue("aggregateId", aggregateId) |> ignore
            
            let! result = command.ExecuteScalarAsync() |> Async.AwaitTask
            return result :?> int
        }
    
    // Append events with inline projection
    let appendEvents
        (connectionString: string)
        (aggregateType: string)
        (aggregateId: Guid)
        (expectedVersion: int)
        (events: OrderEvent list)
        (project: OrderEvent -> NpgsqlTransaction -> Async<unit>)
        : Async<Result<unit, string>> =
        async {
            use connection = new NpgsqlConnection(connectionString)
            do! connection.OpenAsync() |> Async.AwaitTask
            
            use transaction = connection.BeginTransaction()
            
            try
                // Append each event
                for i, event in List.indexed events do
                    let version = expectedVersion + i + 1
                    let eventId = Guid.NewGuid()
                    let eventType = getEventType event
                    let eventData = serializeEvent event
                    
                    use command = new NpgsqlCommand(
                        "INSERT INTO events (
                            event_id, aggregate_type, aggregate_id,
                            event_type, event_data, version, occurred_at
                         ) VALUES (
                            @eventId, @aggregateType, @aggregateId,
                            @eventType, @eventData::jsonb, @version, @occurredAt
                         )",
                        connection,
                        transaction)
                    
                    command.Parameters.AddWithValue("eventId", eventId) |> ignore
                    command.Parameters.AddWithValue("aggregateType", aggregateType) |> ignore
                    command.Parameters.AddWithValue("aggregateId", aggregateId) |> ignore
                    command.Parameters.AddWithValue("eventType", eventType) |> ignore
                    command.Parameters.AddWithValue("eventData", eventData) |> ignore
                    command.Parameters.AddWithValue("version", version) |> ignore
                    command.Parameters.AddWithValue("occurredAt", DateTime.UtcNow) |> ignore
                    
                    do! command.ExecuteNonQueryAsync() |> Async.AwaitTask |> Async.Ignore
                    
                    // Project inline (same transaction)
                    do! project event transaction
                
                do! transaction.CommitAsync() |> Async.AwaitTask
                return Ok ()
                
            with
            | :? PostgresException as ex when ex.SqlState = "23505" ->
                do! transaction.RollbackAsync() |> Async.AwaitTask
                return Error "Concurrency conflict: another process modified this aggregate"
            
            | ex ->
                do! transaction.RollbackAsync() |> Async.AwaitTask
                return Error $"Failed to append events: {ex.Message}"
        }
```

**Key principles:**
- Async workflow for I/O operations
- Single transaction for events + projections
- Optimistic concurrency via unique constraint
- Explicit error handling with Result type

---

## Projection Implementation

```fsharp
module OrderProjections =
    open Npgsql
    
    // Helper to execute SQL within transaction
    let private executeNonQuery 
        (sql: string)
        (parameters: (string * obj) list)
        (tx: NpgsqlTransaction)
        : Async<unit> =
        async {
            use command = new NpgsqlCommand(sql, tx.Connection, tx)
            for name, value in parameters do
                command.Parameters.AddWithValue(name, value) |> ignore
            do! command.ExecuteNonQueryAsync() |> Async.AwaitTask |> Async.Ignore
        }
    
    // Project OrderPlaced event
    let private projectOrderPlaced 
        (data: OrderPlacedData)
        (tx: NpgsqlTransaction)
        : Async<unit> =
        async {
            let (OrderId orderId) = data.OrderId
            let (CustomerId customerId) = data.CustomerId
            let (Money totalAmount) = data.TotalAmount
            
            // Insert order
            do! executeNonQuery
                "INSERT INTO orders (order_id, customer_id, status, total_amount, created_at, updated_at)
                 VALUES (@orderId, @customerId, @status, @totalAmount, @createdAt, @updatedAt)"
                [
                    "orderId", box orderId
                    "customerId", box customerId
                    "status", box "placed"
                    "totalAmount", box totalAmount
                    "createdAt", box data.OccurredAt
                    "updatedAt", box data.OccurredAt
                ]
                tx
            
            // Insert order items
            for item in data.Items do
                let (ItemId itemId) = item.ItemId
                let (ProductId productId) = item.ProductId
                let (Quantity quantity) = item.Quantity
                let (Money price) = item.Price
                
                do! executeNonQuery
                    "INSERT INTO order_items (order_id, product_id, quantity, price)
                     VALUES (@orderId, @productId, @quantity, @price)"
                    [
                        "orderId", box orderId
                        "productId", box productId
                        "quantity", box quantity
                        "price", box price
                    ]
                    tx
        }
    
    // Project OrderConfirmed event
    let private projectOrderConfirmed
        (data: OrderConfirmedData)
        (tx: NpgsqlTransaction)
        : Async<unit> =
        async {
            let (OrderId orderId) = data.OrderId
            
            do! executeNonQuery
                "UPDATE orders 
                 SET status = @status, updated_at = @updatedAt
                 WHERE order_id = @orderId"
                [
                    "orderId", box orderId
                    "status", box "confirmed"
                    "updatedAt", box data.ConfirmedAt
                ]
                tx
        }
    
    // Project OrderShipped event
    let private projectOrderShipped
        (data: OrderShippedData)
        (tx: NpgsqlTransaction)
        : Async<unit> =
        async {
            let (OrderId orderId) = data.OrderId
            
            do! executeNonQuery
                "UPDATE orders 
                 SET status = @status, updated_at = @updatedAt
                 WHERE order_id = @orderId"
                [
                    "orderId", box orderId
                    "status", box "shipped"
                    "updatedAt", box data.ShippedAt
                ]
                tx
        }
    
    // Project OrderCancelled event
    let private projectOrderCancelled
        (data: OrderCancelledData)
        (tx: NpgsqlTransaction)
        : Async<unit> =
        async {
            let (OrderId orderId) = data.OrderId
            
            do! executeNonQuery
                "UPDATE orders 
                 SET status = @status, updated_at = @updatedAt
                 WHERE order_id = @orderId"
                [
                    "orderId", box orderId
                    "status", box "cancelled"
                    "updatedAt", box data.CancelledAt
                ]
                tx
        }
    
    // Main projection dispatcher
    let project (event: OrderEvent) (tx: NpgsqlTransaction) : Async<unit> =
        match event with
        | OrderPlaced data -> projectOrderPlaced data tx
        | OrderConfirmed data -> projectOrderConfirmed data tx
        | OrderShipped data -> projectOrderShipped data tx
        | OrderCancelled data -> projectOrderCancelled data tx
```

**Key principles:**
- One projection function per event type
- Pattern matching for event routing
- Same transaction passed through all projections
- Unwrap single-case DUs before SQL

---

## Application Service (Command Handler)

```fsharp
module OrderService =
    
    type OrderService(connectionString: string) =
        let aggregateType = "Order"
        
        // Generic command handler
        member private this.HandleCommand
            (orderId: OrderId)
            (command: OrderCommand)
            : Async<Result<unit, string>> =
            async {
                let (OrderId guid) = orderId
                
                // 1. Load events
                let! events = EventStore.loadEvents connectionString aggregateType guid
                
                // 2. Reconstitute state
                let currentState = OrderEvolution.replay events
                
                // 3. Get current version
                let currentVersion = 
                    match currentState with
                    | Some order -> order.Version
                    | None -> 0
                
                // 4. Decide what events to produce
                match OrderDecision.decide command currentState with
                | Error err -> 
                    return Error err
                
                | Ok newEvents ->
                    // 5. Append events with inline projections
                    return! EventStore.appendEvents
                        connectionString
                        aggregateType
                        guid
                        currentVersion
                        newEvents
                        OrderProjections.project
            }
        
        // Public API methods
        member this.PlaceOrder
            (orderId: OrderId)
            (customerId: CustomerId)
            (items: OrderItem list)
            : Async<Result<unit, string>> =
            let command = PlaceOrder {
                OrderId = orderId
                CustomerId = customerId
                Items = items
            }
            this.HandleCommand orderId command
        
        member this.ConfirmOrder (orderId: OrderId) : Async<Result<unit, string>> =
            this.HandleCommand orderId ConfirmOrder
        
        member this.ShipOrder 
            (orderId: OrderId)
            (trackingNumber: string)
            : Async<Result<unit, string>> =
            this.HandleCommand orderId (ShipOrder trackingNumber)
        
        member this.CancelOrder
            (orderId: OrderId)
            (reason: string)
            : Async<Result<unit, string>> =
            this.HandleCommand orderId (CancelOrder reason)
        
        // Query from projection (not from events!)
        member this.GetOrder (orderId: OrderId) : Async<OrderView option> =
            async {
                use connection = new NpgsqlConnection(connectionString)
                do! connection.OpenAsync() |> Async.AwaitTask
                
                let (OrderId guid) = orderId
                
                use command = new NpgsqlCommand(
                    "SELECT order_id, customer_id, status, total_amount, created_at
                     FROM orders
                     WHERE order_id = @orderId",
                    connection)
                
                command.Parameters.AddWithValue("orderId", guid) |> ignore
                
                use! reader = command.ExecuteReaderAsync() |> Async.AwaitTask
                
                if! reader.ReadAsync() |> Async.AwaitTask then
                    let orderView = {
                        OrderId = reader.GetGuid(0) |> OrderId
                        CustomerId = reader.GetGuid(1) |> CustomerId
                        Status = reader.GetString(2)
                        TotalAmount = reader.GetDecimal(3) |> Money
                        CreatedAt = reader.GetDateTime(4)
                    }
                    return Some orderView
                else
                    return None
            }

// Read model type (different from aggregate state!)
and OrderView = {
    OrderId: OrderId
    CustomerId: CustomerId
    Status: string
    TotalAmount: Money
    CreatedAt: DateTime
}
```

**Key principles:**
- Private generic command handler
- Public API methods map to commands
- Result type for error handling
- Queries go to projections, not events
- Read models separate from aggregate state

---

## Concurrency Handling with Retry

```fsharp
module RetryLogic =
    open System.Threading
    
    let rec private retryWithDelay<'T>
        (maxRetries: int)
        (currentAttempt: int)
        (operation: unit -> Async<Result<'T, string>>)
        : Async<Result<'T, string>> =
        async {
            match! operation() with
            | Ok result -> 
                return Ok result
            
            | Error msg when msg.Contains("Concurrency conflict") && currentAttempt < maxRetries ->
                // Exponential backoff
                let delayMs = int (100.0 * (2.0 ** float currentAttempt))
                do! Async.Sleep delayMs
                return! retryWithDelay maxRetries (currentAttempt + 1) operation
            
            | Error msg ->
                return Error msg
        }
    
    let executeWithRetry<'T>
        (maxRetries: int)
        (operation: unit -> Async<Result<'T, string>>)
        : Async<Result<'T, string>> =
        retryWithDelay maxRetries 0 operation

// Usage
let placeOrderWithRetry orderId customerId items =
    RetryLogic.executeWithRetry 3 (fun () ->
        orderService.PlaceOrder orderId customerId items
    )
```

---

## Critical Best Practices

### ✅ DO: Version Events from Day 1

```fsharp
// Strategy 1: Versioned event types
type OrderPlacedV1 = {
    OrderId: OrderId
    CustomerId: CustomerId
    Items: OrderItem list
}

type OrderPlacedV2 = {
    OrderId: OrderId
    CustomerId: CustomerId
    Items: OrderItem list
    ShippingAddress: Address  // New field
}

type OrderEvent =
    | OrderPlacedV1 of OrderPlacedV1
    | OrderPlacedV2 of OrderPlacedV2
    // ... other events

// Strategy 2: Optional fields for evolution
type OrderPlacedData = {
    OrderId: OrderId
    CustomerId: CustomerId
    Items: OrderItem list
    // Optional fields for future evolution
    ShippingAddress: Address option
    BillingAddress: Address option
}
```

### ✅ DO: Keep Aggregates Small

```fsharp
// ❌ BAD - God aggregate
type Order = {
    Id: OrderId
    Items: OrderItem list
    Payment: Payment option
    Shipment: Shipment option
    Returns: Return list
    Reviews: Review list
    // ... 50+ events per order lifecycle
}

// ✅ GOOD - Focused aggregates
type Order = {
    Id: OrderId
    CustomerId: CustomerId
    Items: OrderItem list
    Status: OrderStatus
    Version: int
}

type Shipment = {
    Id: ShipmentId
    OrderId: OrderId
    TrackingNumber: string
    Status: ShipmentStatus
    Version: int
}

type Return = {
    Id: ReturnId
    OrderId: OrderId
    Reason: string
    Status: ReturnStatus
    Version: int
}
```

### ✅ DO: Use Single-Case DUs for Domain Primitives

```fsharp
// Prevents mixing up IDs and other primitives
let processOrder 
    (orderId: OrderId)      // Can't accidentally pass CustomerId
    (customerId: CustomerId)
    : unit =
    // ...

// Compile error - type safety!
let orderId = OrderId.create()
let customerId = CustomerId.create()
processOrder customerId orderId  // Won't compile!
```

### ✅ DO: Separate Commands from Events

```fsharp
// Commands (imperative, can fail)
type OrderCommand =
    | PlaceOrder of PlaceOrderData
    | ConfirmOrder
    | CancelOrder of reason: string

// Events (past tense, immutable facts)
type OrderEvent =
    | OrderPlaced of OrderPlacedData
    | OrderConfirmed of OrderConfirmedData
    | OrderCancelled of OrderCancelledData
```

### ✅ DO: Use Result Type for Error Handling

```fsharp
// Don't throw exceptions in business logic
let decide (command: OrderCommand) (order: Order option) : Result<OrderEvent list, string> =
    match command, order with
    | PlaceOrder data, None when List.isEmpty data.Items ->
        Error "Order must have at least one item"  // ✅ Explicit error
    | PlaceOrder data, None ->
        Ok [ OrderPlaced { (* ... *) } ]           // ✅ Success
    | _ ->
        Error "Invalid command"

// Not this:
let decideBad (command: OrderCommand) (order: Order option) : OrderEvent list =
    match command, order with
    | PlaceOrder data, None when List.isEmpty data.Items ->
        failwith "Order must have at least one item"  // ❌ Exception
    | _ -> []
```

---

## Critical Anti-Patterns to Avoid

### ❌ DON'T: Store Current State in Events

```fsharp
// ❌ BAD - Storing entire state
type OrderUpdatedData = {
    Order: Order  // Entire aggregate state
}

// ✅ GOOD - Store only what changed
type OrderConfirmedData = {
    OrderId: OrderId
    ConfirmedAt: DateTime
}
```

### ❌ DON'T: Put Business Logic in Projections

```fsharp
// ❌ BAD - Validation in projection
let projectOrderPlaced (data: OrderPlacedData) (tx: NpgsqlTransaction) =
    async {
        let (Money amount) = data.TotalAmount
        
        // DON'T DO THIS!
        if amount > 10000m then
            failwith "Order too large"
        
        // ... projection logic
    }

// ✅ GOOD - Validation in decide
let decide (command: OrderCommand) (order: Order option) =
    match command with
    | PlaceOrder data ->
        let totalAmount = calculateTotal data.Items
        
        // Validation here!
        if Money.value totalAmount > 10000m then
            Error "Order exceeds maximum amount"
        else
            Ok [ OrderPlaced { (* ... *) } ]
```

### ❌ DON'T: Forget Transaction Boundaries

```fsharp
// ❌ BAD - Separate transactions
let appendEventsBad events =
    async {
        do! insertEvent event       // Transaction 1
        do! updateProjection event  // Transaction 2 - Can fail!
    }

// ✅ GOOD - Single transaction
let appendEventsGood events =
    async {
        use connection = new NpgsqlConnection(connectionString)
        do! connection.OpenAsync() |> Async.AwaitTask
        use tx = connection.BeginTransaction()
        
        try
            do! insertEvent event tx
            do! updateProjection event tx
            do! tx.CommitAsync() |> Async.AwaitTask
        with ex ->
            do! tx.RollbackAsync() |> Async.AwaitTask
            reraise()
    }
```

### ❌ DON'T: Load Events for Every Read

```fsharp
// ❌ BAD - Replaying events for queries
let getOrder (orderId: OrderId) : Async<Order option> =
    async {
        let! events = loadEvents orderId
        return OrderEvolution.replay events
    }

// ✅ GOOD - Query projection
let getOrder (orderId: OrderId) : Async<OrderView option> =
    async {
        return! queryOrderProjection orderId
    }
```

### ❌ DON'T: Use Mutable State in Aggregates

```fsharp
// ❌ BAD - Mutable class
type Order() =
    let mutable items = []
    let mutable status = Draft
    
    member this.Items 
        with get() = items
        and set(value) = items <- value  // Mutation!

// ✅ GOOD - Immutable record
type Order = {
    Items: OrderItem list
    Status: OrderStatus
}

let evolve (state: Order option) (event: OrderEvent) : Order =
    // Return new immutable record
    { state with Status = Confirmed }
```

---

## Event Schema Evolution

### Strategy 1: Optional Fields (Simplest)

```fsharp
type OrderPlacedData = {
    OrderId: OrderId
    CustomerId: CustomerId
    Items: OrderItem list
    // V1 fields above
    
    // V2 optional fields
    ShippingAddress: Address option
    BillingAddress: Address option
}

// Projection handles both versions
let projectOrderPlaced (data: OrderPlacedData) (tx: NpgsqlTransaction) =
    async {
        let shippingAddr = 
            data.ShippingAddress 
            |> Option.map Address.toString
            |> Option.toObj
        
        do! executeNonQuery 
            "INSERT INTO orders (..., shipping_address) VALUES (..., @shippingAddress)"
            [ "shippingAddress", box shippingAddr ]
            tx
    }
```

### Strategy 2: Versioned Types with Upcasting

```fsharp
// Define versions explicitly
type OrderPlacedV1 = {
    OrderId: OrderId
    CustomerId: CustomerId
    Items: OrderItem list
}

type OrderPlacedV2 = {
    OrderId: OrderId
    CustomerId: CustomerId
    Items: OrderItem list
    ShippingAddress: Address
}

// Discriminated union for all versions
type OrderPlacedData =
    | V1 of OrderPlacedV1
    | V2 of OrderPlacedV2

// Upcast old version to new
module OrderPlacedData =
    let upcast (data: OrderPlacedData) : OrderPlacedV2 =
        match data with
        | V1 v1 -> 
            {
                OrderId = v1.OrderId
                CustomerId = v1.CustomerId
                Items = v1.Items
                ShippingAddress = Address.default  // Default value
            }
        | V2 v2 -> v2

// Use latest version in business logic
let decide (command: OrderCommand) (order: Order option) =
    match command with
    | PlaceOrder data ->
        let v2Data = OrderPlacedData.upcast data
        // Work with V2 format
        Ok [ OrderPlaced (V2 v2Data) ]
```

### Strategy 3: Event Metadata for Version Info

```fsharp
type DomainEvent<'T> = {
    EventId: Guid
    EventData: 'T
    EventVersion: int  // Track schema version
    OccurredAt: DateTime
}

let deserializeEvent (eventType: string) (version: int) (json: string) : OrderEvent =
    match eventType, version with
    | "OrderPlaced", 1 -> 
        let v1 = JsonSerializer.Deserialize<OrderPlacedV1>(json)
        OrderPlaced (V1 v1)
    
    | "OrderPlaced", 2 ->
        let v2 = JsonSerializer.Deserialize<OrderPlacedV2>(json)
        OrderPlaced (V2 v2)
    
    | _ -> failwith $"Unknown event version: {eventType} v{version}"
```

---

## Testing Strategy

### Unit Tests (Pure Functions)

```fsharp
module OrderTests =
    open Xunit
    open FsUnit.Xunit
    
    [<Fact>]
    let ``PlaceOrder with valid items creates OrderPlaced event`` () =
        // Arrange
        let command = PlaceOrder {
            OrderId = OrderId.create()
            CustomerId = CustomerId.create()
            Items = [
                {
                    ItemId = ItemId.create()
                    ProductId = ProductId.create()
                    Quantity = Quantity 2
                    Price = Money 50m
                }
            ]
        }
        
        // Act
        let result = OrderDecision.decide command None
        
        // Assert
        match result with
        | Ok [ OrderPlaced data ] ->
            data.Items |> should haveLength 1
            Money.value data.TotalAmount |> should equal 100m
        | _ ->
            Assert.Fail("Expected OrderPlaced event")
    
    [<Fact>]
    let ``PlaceOrder without items returns error`` () =
        // Arrange
        let command = PlaceOrder {
            OrderId = OrderId.create()
            CustomerId = CustomerId.create()
            Items = []
        }
        
        // Act
        let result = OrderDecision.decide command None
        
        // Assert
        match result with
        | Error msg ->
            msg |> should contain "at least one item"
        | Ok _ ->
            Assert.Fail("Should have failed")
    
    [<Fact>]
    let ``ConfirmOrder only works on placed orders`` () =
        // Arrange - Create placed order
        let order = Some {
            Id = OrderId.create()
            CustomerId = CustomerId.create()
            Items = []
            Status = Placed
            Version = 1
        }
        
        // Act
        let result = OrderDecision.decide ConfirmOrder order
        
        // Assert
        match result with
        | Ok [ OrderConfirmed _ ] -> ()
        | _ -> Assert.Fail("Should have confirmed order")
    
    [<Fact>]
    let ``ConfirmOrder fails on shipped orders`` () =
        // Arrange
        let order = Some {
            Id = OrderId.create()
            CustomerId = CustomerId.create()
            Items = []
            Status = Shipped
            Version = 2
        }
        
        // Act
        let result = OrderDecision.decide ConfirmOrder order
        
        // Assert
        match result with
        | Error msg ->
            msg |> should contain "Cannot confirm"
        | Ok _ ->
            Assert.Fail("Should have failed")
    
    [<Fact>]
    let ``Event replay reconstitutes correct state`` () =
        // Arrange
        let orderId = OrderId.create()
        let customerId = CustomerId.create()
        
        let events = [
            OrderPlaced {
                OrderId = orderId
                CustomerId = customerId
                Items = []
                TotalAmount = Money 100m
                OccurredAt = DateTime.UtcNow
            }
            OrderConfirmed {
                OrderId = orderId
                ConfirmedAt = DateTime.UtcNow
            }
        ]
        
        // Act
        let state = OrderEvolution.replay events
        
        // Assert
        match state with
        | Some order ->
            order.Status |> should equal Confirmed
            order.Version |> should equal 2
        | None ->
            Assert.Fail("Should have reconstituted order")
```

### Property-Based Tests

```fsharp
module OrderPropertyTests =
    open FsCheck
    open FsCheck.Xunit
    
    // Generators for domain types
    let genOrderId = 
        Arb.generate<Guid> |> Gen.map OrderId
    
    let genCustomerId = 
        Arb.generate<Guid> |> Gen.map CustomerId
    
    let genQuantity =
        Gen.choose(1, 100) |> Gen.map Quantity
    
    let genMoney =
        Gen.choose(1, 10000) |> Gen.map (decimal >> Money)
    
    let genOrderItem =
        gen {
            let! productId = Arb.generate<Guid> |> Gen.map ProductId
            let! quantity = genQuantity
            let! price = genMoney
            return {
                ItemId = ItemId.create()
                ProductId = productId
                Quantity = quantity
                Price = price
            }
        }
    
    [<Property>]
    let ``Replaying events produces same state as applying them sequentially`` 
        (events: OrderEvent list) =
        not (List.isEmpty events) ==> lazy
            // Apply events sequentially
            let stateSequential =
                events
                |> List.fold (fun state event -> Some (OrderEvolution.evolve state event)) None
            
            // Replay all at once
            let stateReplayed = OrderEvolution.replay events
            
            stateSequential = stateReplayed
    
    [<Property>]
    let ``Decide never produces empty event list on success``
        (command: OrderCommand)
        (state: Order option) =
        match OrderDecision.decide command state with
        | Ok events -> not (List.isEmpty events)
        | Error _ -> true
```

### Integration Tests

```fsharp
module OrderIntegrationTests =
    open Xunit
    open FsUnit.Xunit
    
    type DatabaseFixture() =
        let connectionString = "Host=localhost;Database=test;..."
        
        member _.ConnectionString = connectionString
        
        member _.SetupDatabase() =
            async {
                // Create tables, seed data, etc.
                ()
            }
        
        member _.CleanupDatabase() =
            async {
                // Truncate tables
                ()
            }
        
        interface IDisposable with
            member this.Dispose() =
                this.CleanupDatabase() |> Async.RunSynchronously
    
    type OrderIntegrationTests(fixture: DatabaseFixture) =
        
        [<Fact>]
        let ``Full order lifecycle persists correctly`` () =
            async {
                // Arrange
                do! fixture.SetupDatabase()
                let service = OrderService.OrderService(fixture.ConnectionString)
                let orderId = OrderId.create()
                let customerId = CustomerId.create()
                let items = [
                    {
                        ItemId = ItemId.create()
                        ProductId = ProductId.create()
                        Quantity = Quantity 1
                        Price = Money 100m
                    }
                ]
                
                // Act - Place order
                let! placeResult = service.PlaceOrder orderId customerId items
                placeResult |> should be (ofCase <@ Ok @>)
                
                // Act - Confirm order
                let! confirmResult = service.ConfirmOrder orderId
                confirmResult |> should be (ofCase <@ Ok @>)
                
                // Assert - Check projection
                let! orderView = service.GetOrder orderId
                orderView.IsSome |> should equal true
                orderView.Value.Status |> should equal "confirmed"
                
                // Assert - Check events stored
                let (OrderId guid) = orderId
                let! events = EventStore.loadEvents fixture.ConnectionString "Order" guid
                events |> should haveLength 2
                
                match events with
                | [ OrderPlaced _; OrderConfirmed _ ] -> ()
                | _ -> Assert.Fail("Expected OrderPlaced then OrderConfirmed")
                
                do! fixture.CleanupDatabase()
            } |> Async.RunSynchronously
        
        [<Fact>]
        let ``Concurrent modifications handled correctly`` () =
            async {
                // Arrange
                do! fixture.SetupDatabase()
                let service = OrderService.OrderService(fixture.ConnectionString)
                let orderId = OrderId.create()
                let customerId = CustomerId.create()
                let items = [
                    {
                        ItemId = ItemId.create()
                        ProductId = ProductId.create()
                        Quantity = Quantity 1
                        Price = Money 100m
                    }
                ]
                
                // Place initial order
                let! _ = service.PlaceOrder orderId customerId items
                
                // Act - Simulate concurrent confirmations
                let! results = 
                    [ service.ConfirmOrder orderId
                      service.ConfirmOrder orderId ]
                    |> Async.Parallel
                
                // Assert - One should succeed, one should fail
                let successes = results |> Array.filter Result.isOk
                let failures = results |> Array.filter Result.isError
                
                successes |> should haveLength 1
                failures |> should haveLength 1
                
                do! fixture.CleanupDatabase()
            } |> Async.RunSynchronously
        
        interface IClassFixture<DatabaseFixture>
```

---

## Performance Considerations

### Write Performance

**Inline projections are slower because:**
- Events + projections in single transaction
- Multiple table writes per command
- Projection complexity affects write latency directly

**Typical latencies (PostgreSQL):**
- Simple projection: 10-30ms
- Medium complexity: 30-100ms
- Complex projection: 100-300ms

**Mitigation:**
```fsharp
// 1. Keep projections simple
let projectOrderPlaced (data: OrderPlacedData) (tx: NpgsqlTransaction) =
    async {
        // Just insert, no complex calculations
        do! executeNonQuery sql params tx
    }

// 2. Use proper indexes
// CREATE INDEX idx_orders_customer ON orders(customer_id);
// CREATE INDEX idx_orders_status ON orders(status);

// 3. Batch operations where possible
let projectOrderItems (items: OrderItem list) (tx: NpgsqlTransaction) =
    async {
        // Use COPY or multi-value INSERT for better performance
        ()
    }
```

### Read Performance

**Projections enable fast reads:**
```fsharp
// Query projection, not events
let getOrdersByCustomer (customerId: CustomerId) : Async<OrderView list> =
    async {
        let (CustomerId guid) = customerId
        
        // Fast indexed query on projection
        use connection = new NpgsqlConnection(connectionString)
        // ... standard SQL query
        return orders
    }

// NOT this (slow event replay)
let getOrdersByCustomerBad (customerId: CustomerId) : Async<Order list> =
    async {
        // Load ALL order events for customer - SLOW!
        let! orderIds = getAllOrderIdsForCustomer customerId
        let! orders = 
            orderIds
            |> List.map (fun id -> 
                async {
                    let! events = EventStore.loadEvents "Order" id
                    return OrderEvolution.replay events
                })
            |> Async.Parallel
        return Array.toList orders
    }
```

### Scaling Path

```fsharp
// START HERE: Inline projections
type Config = {
    UseInlineProjections: bool
}

let config = { UseInlineProjections = true }

// GROW: Mix of inline + async
type ProjectionMode =
    | Inline
    | Async

type ProjectionConfig = {
    AccountBalance: ProjectionMode  // Critical - inline
    OrderList: ProjectionMode       // Important - inline
    SearchIndex: ProjectionMode     // Can be stale - async
    Analytics: ProjectionMode       // Definitely async
}

// SCALE: Mostly async with event bus
type EventBus = {
    Publish: OrderEvent -> Async<unit>
}
```

---

## Monitoring & Observability

### Logging

```fsharp
module Logging =
    open Microsoft.Extensions.Logging
    
    let logEventAppend 
        (logger: ILogger)
        (aggregateType: string)
        (aggregateId: Guid)
        (eventCount: int)
        (durationMs: int) =
        logger.LogInformation(
            "Appended {EventCount} events for {AggregateType}:{AggregateId} in {DurationMs}ms",
            eventCount, aggregateType, aggregateId, durationMs
        )
    
    let logConcurrencyConflict
        (logger: ILogger)
        (aggregateType: string)
        (aggregateId: Guid)
        (expectedVersion: int) =
        logger.LogWarning(
            "Concurrency conflict for {AggregateType}:{AggregateId}, expected version {ExpectedVersion}",
            aggregateType, aggregateId, expectedVersion
        )
    
    let logError
        (logger: ILogger)
        (operation: string)
        (ex: exn) =
        logger.LogError(ex, "Failed to {Operation}", operation)

// Use in EventStore
let appendEventsWithLogging
    (logger: ILogger)
    (connectionString: string)
    (aggregateType: string)
    (aggregateId: Guid)
    (expectedVersion: int)
    (events: OrderEvent list)
    (project: OrderEvent -> NpgsqlTransaction -> Async<unit>)
    : Async<Result<unit, string>> =
    async {
        let startTime = DateTime.UtcNow
        
        Logging.logEventAppend logger aggregateType aggregateId events.Length 0
        
        let! result = EventStore.appendEvents 
            connectionString aggregateType aggregateId 
            expectedVersion events project
        
        match result with
        | Ok () ->
            let duration = (DateTime.UtcNow - startTime).TotalMilliseconds |> int
            Logging.logEventAppend logger aggregateType aggregateId events.Length duration
            return Ok ()
        
        | Error msg when msg.Contains("Concurrency conflict") ->
            Logging.logConcurrencyConflict logger aggregateType aggregateId expectedVersion
            return Error msg
        
        | Error msg ->
            Logging.logError logger "append events" (exn msg)
            return Error msg
    }
```

### Metrics

```fsharp
module Metrics =
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
```

### Health Checks

```fsharp
module HealthChecks =
    open Microsoft.Extensions.Diagnostics.HealthChecks
    
    type EventStoreHealthCheck(connectionString: string) =
        interface IHealthCheck with
            member _.CheckHealthAsync(context, cancellationToken) =
                async {
                    try
                        use connection = new NpgsqlConnection(connectionString)
                        do! connection.OpenAsync(cancellationToken) |> Async.AwaitTask
                        
                        // Check event store size
                        use command = new NpgsqlCommand(
                            "SELECT COUNT(*) as event_count,
                                    pg_size_pretty(pg_total_relation_size('events')) as size
                             FROM events",
                            connection)
                        
                        use! reader = command.ExecuteReaderAsync(cancellationToken) |> Async.AwaitTask
                        
                        if! reader.ReadAsync(cancellationToken) |> Async.AwaitTask then
                            let eventCount = reader.GetInt64(0)
                            let size = reader.GetString(1)
                            
                            let data = dict [
                                "event_count", box eventCount
                                "storage_size", box size
                            ]
                            
                            return HealthCheckResult.Healthy("Event store is healthy", data = data)
                        else
                            return HealthCheckResult.Unhealthy("Could not query event store")
                            
                    with ex ->
                        return HealthCheckResult.Unhealthy("Event store connection failed", ex)
                } |> Async.StartAsTask
```

---

## Migration Strategy

### Phase 1: Dual Write

```fsharp
// Keep existing code, add event sourcing alongside
module LegacyOrderService =
    
    let placeOrder (orderId: Guid) (customerId: Guid) (items: Item list) =
        async {
            use connection = new NpgsqlConnection(connectionString)
            do! connection.OpenAsync() |> Async.AwaitTask
            use tx = connection.BeginTransaction()
            
            try
                // 1. OLD: Write to existing database
                do! executeSql 
                    "INSERT INTO orders (order_id, customer_id, status, ...) VALUES (...)"
                    tx
                
                // 2. NEW: Also write events
                let events = [
                    OrderPlaced {
                        OrderId = OrderId orderId
                        CustomerId = CustomerId customerId
                        Items = items |> List.map mapToOrderItem
                        TotalAmount = calculateTotal items
                        OccurredAt = DateTime.UtcNow
                    }
                ]
                
                do! EventStore.appendEvents
                    connectionString
                    "Order"
                    orderId
                    0
                    events
                    (fun event tx -> OrderProjections.project event tx)
                
                do! tx.CommitAsync() |> Async.AwaitTask
                
            with ex ->
                do! tx.RollbackAsync() |> Async.AwaitTask
                reraise()
        }
```

### Phase 2: Read from Projections

```fsharp
// Gradually switch reads to projections
module OrderQuery =
    
    // Feature flag for gradual rollout
    type ReadMode =
        | Legacy
        | Projection
    
    let getOrder (mode: ReadMode) (orderId: OrderId) : Async<OrderView option> =
        match mode with
        | Legacy ->
            // Read from old tables
            queryLegacyDatabase orderId
        
        | Projection ->
            // Read from projection (same tables, but via event-sourced writes)
            queryProjection orderId
```

### Phase 3: Remove Legacy Writes

```fsharp
// Pure event sourcing - no more dual writes
module OrderService =
    
    type OrderService(connectionString: string) =
        
        member this.PlaceOrder 
            (orderId: OrderId)
            (customerId: CustomerId)
            (items: OrderItem list) =
            async {
                let command = PlaceOrder {
                    OrderId = orderId
                    CustomerId = customerId
                    Items = items
                }
                
                return! this.HandleCommand orderId command
            }
```

---

## When to Move Beyond Inline Projections

### Consider async projections when:

1. **Write latency becomes problematic**
   ```fsharp
   // Metric shows high latency
   if avgWriteLatency > 100<ms> then
       // Consider async projections
       ()
   ```

2. **You need multiple read models**
   ```fsharp
   type ProjectionType =
       | OrderList          // For queries
       | SearchIndex        // For full-text search
       | Analytics          // For reporting
       | CustomerSummary    // Denormalized view
   
   // Same events → different projections
   // Async allows independent processing
   ```

3. **Projection complexity grows**
   ```fsharp
   // Complex calculation in projection
   let projectOrderAnalytics (event: OrderEvent) (tx: NpgsqlTransaction) =
       async {
           // Heavy computation
           let! customerLifetimeValue = calculateLTV customerId
           let! productRecommendations = generateRecommendations items
           let! fraudScore = checkFraudRisk order
           
           // This slows down writes - move to async!
           ()
       }
   ```

### Migration path to async:

```fsharp
type ProjectionStrategy =
    | InlineSync        // Strong consistency
    | AsyncEventual     // Eventual consistency

type ProjectionConfig = {
    OrderStatus: ProjectionStrategy         // Inline - critical
    AccountBalance: ProjectionStrategy      // Inline - critical
    OrderList: ProjectionStrategy          // Inline - important
    SearchIndex: ProjectionStrategy        // Async - can be stale
    Analytics: ProjectionStrategy          // Async - definitely
    CustomerSummary: ProjectionStrategy    // Async - derived data
}

let config = {
    OrderStatus = InlineSync
    AccountBalance = InlineSync
    OrderList = InlineSync
    SearchIndex = AsyncEventual
    Analytics = AsyncEventual
    CustomerSummary = AsyncEventual
}
```

---

## Quick Reference

### Command Handler Flow
```fsharp
Load Events 
→ Replay to State 
→ Decide (validate + produce events)
→ Append Events + Project (single transaction)
→ Commit or Rollback
```

### Event Store Pattern
```
BEGIN TRANSACTION
  ├─ INSERT INTO events (...)
  ├─ UPDATE projection_table_1
  ├─ UPDATE projection_table_2
  └─ COMMIT (atomic!)
```

### Project Structure
```
src/
  ├─ Domain/
  │   ├─ Types.fs          # Domain primitives (OrderId, etc.)
  │   ├─ Events.fs         # Event type definitions
  │   ├─ Commands.fs       # Command type definitions
  │   ├─ Aggregate.fs      # Order aggregate (state)
  │   ├─ Decision.fs       # decide function
  │   └─ Evolution.fs      # evolve function
  ├─ Infrastructure/
  │   ├─ EventStore.fs     # Event persistence
  │   ├─ Projections.fs    # Projection handlers
  │   └─ Database.fs       # Connection management
  └─ Application/
      ├─ OrderService.fs   # Application service
      └─ Queries.fs        # Read models
```

---

## Common Pitfalls Summary

| Pitfall | Why It's Bad | Do This Instead |
|---------|--------------|-----------------|
| Mutable aggregate state | Side effects, hard to reason about | Use immutable records |
| Throwing exceptions in decide | Breaks functional flow | Return Result type |
| Loading events for queries | Slow, doesn't scale | Query projections |
| Business logic in projections | Can't reconstitute state | Logic only in decide |
| Using primitives everywhere | No type safety | Single-case DUs |
| Forgetting optimistic concurrency | Lost updates | Always use expected version |
| Separate transactions | Data inconsistency | Single transaction for events + projections |

---

## Conclusion

**Key Principles:**
- Embrace F# strengths: immutability, discriminated unions, pattern matching
- Events are source of truth, immutable facts
- Aggregates enforce business rules via pure functions
- Projections are derived, rebuildable views
- Single transaction = strong consistency
- Result type for explicit error handling

**Inline projections are perfectly valid for:**
- Getting started with Event Sourcing
- Systems with reasonable write volume (<1000/sec)
- Critical read models requiring strong consistency
- Teams learning ES patterns without distributed complexity

**Remember:** Start with one aggregate, validate thoroughly, then expand. Inline projections let you learn ES patterns using idiomatic F# without distributed system complexity. Add async projections only when you actually need them.

Good luck with your migration! 🚀