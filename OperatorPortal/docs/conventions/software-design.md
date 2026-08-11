# Software Design Principles for F#

Professional software design patterns and principles for writing maintainable, well-structured F# code.

## Critical Rules

🚨 **Fail-fast over silent fallbacks.** Never use option default chains (`option1 |> Option.orElse option2 |> Option.defaultValue "unknown"`). If data should exist, validate and return Error or raise a clear exception.

🚨 **Embrace type-safety. Use proper domain modeling.** F#'s type system is your most powerful tool. Design types that make illegal states unrepresentable.

🚨 **Make illegal states unrepresentable.** Use discriminated unions and single-case unions. If a state combination shouldn't exist, make the type system forbid it.

🚨 **Use composition root pattern for dependencies.** Dependencies are composed at the application boundary and passed down through function parameters or partial application. No service locator pattern.

🚨 **Intention-revealing names only.** Never use `data`, `utils`, `helpers`, `handler`, `processor`. Name things for what they do in the domain.

🚨 **No code comments.** Comments are a failure to express intent in code. If you need a comment to explain what code does, the code isn't clear enough—refactor it.

🚨 **Use Result type for expected errors.** Don't throw exceptions for business rule violations. Use `Result<'T, 'Error>` to make error paths explicit and type-safe.

🚨 **Async-first for I/O operations.** All I/O operations should return `Async<'T>`. Don't block asynchronous operations.

## When This Applies

- Writing new code (these are defaults, not just refactoring goals)
- Refactoring existing code
- Code reviews and design reviews
- During TDD REFACTOR phase
- When analyzing coupling and cohesion

## Core Philosophy

Well-designed, maintainable code is far more important than getting things done quickly. Every design decision should favor:
- **Clarity over cleverness**
- **Explicit over implicit**
- **Fail-fast over silent fallbacks**
- **Loose coupling over tight integration**
- **Intention-revealing over generic**
- **Functions over objects**
- **Composition over inheritance**
- **Railway-oriented programming** for error handling
- **Async over blocking** for I/O operations

## Code Without Comments

Never write comments - write expressive code instead. F#'s pipeline operator and descriptive function names should tell the story.

## Module Organization

**Principle:** Organize code by business capabilities using vertical slices. Each feature should be self-contained.

### Vertical Slice Structure

```fsharp
// ✅ VERTICAL SLICES - organized by business capability
src/
  OrderManagement/
    PlaceOrder.fs          // Everything for placing orders
    ConfirmOrder.fs        // Everything for confirming orders
    CancelOrder.fs         // Everything for canceling orders
    Types.fs               // Shared types for order domain
  
  PaymentProcessing/
    ProcessPayment.fs      // Everything for processing payments
    RefundPayment.fs       // Everything for refunds
    Types.fs               // Shared types for payment domain
  
  Inventory/
    ReserveStock.fs        // Everything for reserving stock
    ReleaseStock.fs        // Everything for releasing stock
    Types.fs               // Shared types for inventory domain
```

### Feature Module Pattern

Each feature module contains everything needed for that capability:

```fsharp
// PlaceOrder.fs - complete vertical slice
module OrderManagement.PlaceOrder

// Domain types specific to this feature
type UnvalidatedOrder = {
    CustomerId: string
    Items: UnvalidatedOrderItem list
}

type ValidatedOrder = {
    CustomerId: CustomerId
    Items: ValidatedOrderItem list
}

type PlaceOrderError =
    | ValidationFailed of ValidationError list
    | InsufficientInventory of ProductId
    | CustomerNotFound of string

// Private validation functions
let private validateCustomerId (customerId: string) : Result<CustomerId, ValidationError> =
    // validation logic

let private validateOrderItems items : Result<ValidatedOrderItem list, ValidationError list> =
    // validation logic

// Public workflow function
let placeOrder 
    (checkInventory: ProductId -> Async<Result<unit, InventoryError>>)
    (saveOrder: ValidatedOrder -> Async<Result<OrderId, DatabaseError>>)
    (unvalidatedOrder: UnvalidatedOrder)
    : Async<Result<OrderId, PlaceOrderError>> =
    async {
        // Workflow implementation
        let validationResult =
            result {
                let! customerId = validateCustomerId unvalidatedOrder.CustomerId
                let! items = validateOrderItems unvalidatedOrder.Items
                return { CustomerId = customerId; Items = items }
            }
        
        match validationResult with
        | Error errors -> return Error (ValidationFailed errors)
        | Ok validatedOrder ->
            // Check inventory for all items
            let! inventoryChecks = 
                validatedOrder.Items
                |> List.map (fun item -> checkInventory item.ProductId)
                |> Async.Sequential
            
            // Continue with order placement...
            return! saveOrder validatedOrder
                    |> Async.map (Result.mapError (fun _ -> PlaceOrderError.CustomerNotFound ""))
    }
```

### Shared Types Module

```fsharp
// Types.fs - shared domain types for the bounded context
module OrderManagement.Types

type CustomerId = CustomerId of Guid
type OrderId = OrderId of Guid
type ProductId = ProductId of int

type OrderItem = {
    ProductId: ProductId
    Quantity: PositiveInt
    Price: Money
}

type Order = {
    OrderId: OrderId
    CustomerId: CustomerId
    Items: OrderItem list
    Status: OrderStatus
}

and OrderStatus =
    | Pending
    | Confirmed of ConfirmationNumber
    | Shipped of ShippingInfo
    | Cancelled of CancellationReason
```

### Cross-Cutting Concerns

```fsharp
// For truly shared utilities (use sparingly)
src/
  Common/
    Result.fs              // Result helper functions
    Async.fs               // Async helper functions
    Validation.fs          // Generic validation helpers
```

**Warning:** Be careful with "Common" or "Shared" modules. Most logic belongs in a specific business capability. Only extract to common when multiple bounded contexts genuinely need the same utility.

## Functional Design Principles

### Pure Functions First

**Principle:** Default to pure functions. Side effects should be pushed to boundaries.

```fsharp
// ❌ IMPURE - side effects buried inside
let processOrder orderId =
    let order = Database.getOrder orderId  // I/O hidden
    printfn "Processing order %A" orderId  // Side effect hidden
    order.Total * 0.9m

// ✅ PURE - explicit dependencies and effects
let calculateDiscountedTotal (order: Order) =
    order.Total * 0.9m

// Side effects at boundary
let processOrder 
    (getOrder: OrderId -> Async<Order>) 
    (log: string -> Async<unit>) 
    (orderId: OrderId) 
    : Async<decimal> =
    async {
        let! order = getOrder orderId
        do! log $"Processing order {orderId}"
        return calculateDiscountedTotal order
    }
```

### Function Composition

**Principle:** Build complex operations from simple functions using composition.

```fsharp
// ❌ IMPERATIVE - procedural steps
let processCustomerOrder order =
    let validated = validateOrder order
    let withTax = applyTax validated
    let withShipping = addShipping withTax
    let final = applyDiscount withShipping
    final

// ✅ FUNCTIONAL - composed pipeline
let processCustomerOrder =
    validateOrder
    >> applyTax
    >> addShipping
    >> applyDiscount
```

### Keep Functions Small

- Small functions (< 10 lines of meaningful code)
- Single level of indentation per function where possible (max 2-3)
- Each function does one thing

### Avoid Early Returns in Pipelines

```fsharp
// ❌ BREAKS PIPELINE FLOW
let processData data =
    if String.IsNullOrEmpty data then
        Error "Empty data"
    else
        let parsed = parseData data
        if isValid parsed then
            Ok (transform parsed)
        else
            Error "Invalid data"

// ✅ PIPELINE-FRIENDLY
let processData data =
    data
    |> validateNotEmpty
    |> Result.bind parseData
    |> Result.bind validateParsed
    |> Result.map transform
```

## Feature Envy Detection

Function uses another module's data more than its own? Move it there.

```fsharp
// ❌ FEATURE ENVY - obsessed with Order's data
module InvoiceGenerator =
    let generate (order: Order) =
        let itemsTotal = 
            order.Items 
            |> List.sumBy (fun i -> i.Price * decimal i.Quantity)
        let total = itemsTotal + itemsTotal * order.TaxRate + order.ShippingCost
        { Total = total }

// ✅ Move logic to the module it belongs to
module Order =
    let calculateTotal (order: Order) =
        let itemsTotal = 
            order.Items 
            |> List.sumBy (fun i -> i.Price * decimal i.Quantity)
        itemsTotal + itemsTotal * order.TaxRate + order.ShippingCost

module InvoiceGenerator =
    let generate (order: Order) =
        { Total = Order.calculateTotal order }
```

**Detection:** Count external module references vs own. More external? Feature envy.

## Dependency Management with Composition Root

Don't instantiate dependencies deep in code. Use composition root pattern.

```fsharp
// ❌ TIGHT COUPLING - hard-coded dependency
module OrderProcessor =
    let process (order: Order) : Async<Result<Order, ProcessingError>> =
        async {
            let! validationResult = OrderValidator.validate order  // Hard to test
            let! _ = EmailService.send (createEmail order)         // Hidden dependency
            return validationResult
        }

// ✅ COMPOSITION ROOT - dependencies injected
module OrderProcessor =
    let process 
        (validateOrder: Order -> Async<Result<Order, ValidationError>>)
        (sendEmail: Email -> Async<unit>)
        (order: Order)
        : Async<Result<Order, ProcessingError>> =
        async {
            let! validationResult = validateOrder order
            match validationResult with
            | Ok validOrder ->
                do! sendEmail (createEmail validOrder)
                return Ok validOrder
            | Error e ->
                return Error (ProcessingError.ValidationFailed e)
        }

// Compose at application boundary
module CompositionRoot =
    let configureOrderProcessing() =
        let validator = OrderValidator.validate
        let emailer = EmailService.send
        OrderProcessor.process validator emailer
```

### Partial Application for Dependency Injection

```fsharp
// ✅ Use partial application to "inject" dependencies
module Program =
    let main() =
        // Composition root - wire up dependencies
        let processOrderWithDeps = 
            OrderProcessor.process 
                OrderValidator.validate 
                EmailService.send
        
        // Now processOrderWithDeps : Order -> Async<Result<Order, ProcessingError>>
        async {
            let! results = 
                orders
                |> List.map processOrderWithDeps
                |> Async.Sequential
            return results
        }
```

## Fail-Fast Error Handling

**NEVER use option default chains that hide missing data:**
```fsharp
option1 |> Option.orElse option2 |> Option.defaultValue "unknown"  // ❌
```

Use Result type and fail fast with clear errors:

```fsharp
// ❌ SILENT FAILURE - hides problems
let getEventType content =
    content.EventType 
    |> Option.orElse content.ClassName 
    |> Option.defaultValue "Unknown"

// ✅ FAIL FAST - immediate, debuggable
let getEventType content =
    match content.EventType with
    | Some eventType -> Ok eventType
    | None -> 
        let availableFields = 
            content.GetType().GetProperties() 
            |> Array.map (fun p -> p.Name)
            |> String.concat ", "
        Error $"Expected 'EventType', got None. Available fields: {availableFields}"
```

**Error format:** `Expected [X]. Got [Y]. Context: [debugging info]`

### When Option.defaultValue Is Acceptable

Single `defaultValue` with a legitimate default is fine when:
- The field is intentionally optional by design
- The default represents the correct initial state (not a fallback hiding missing data)
- No data is being masked or lost

```fsharp
// ✅ OK - optional list with correct initial state
let allItems = (existingItems |> Option.defaultValue []) @ newItems

// ✅ OK - optional config with sensible default
let timeout = config.Timeout |> Option.defaultValue 5000

// ❌ BAD - hiding which field actually had data
let name = 
    user.DisplayName 
    |> Option.orElse user.Username 
    |> Option.orElse user.Email 
    |> Option.defaultValue "Anonymous"

// ❌ BAD - masking missing required data
let id = 
    response.Id 
    |> Option.orElse generatedId 
    |> Option.defaultValue "unknown"
```

**The test:** Would you want to know if the value was missing? If yes, fail fast. If the default is genuinely correct, `defaultValue` is fine.

## Railway-Oriented Programming

**Principle:** Use Result type to handle errors explicitly. Build error-handling into your data flow.

```fsharp
// Define your error types
type ValidationError =
    | EmptyEmail
    | InvalidEmailFormat of string
    | EmailTooLong of int

type OrderError =
    | ValidationFailed of ValidationError list
    | InsufficientInventory of ProductId
    | PaymentDeclined of string

// Build pipelines that handle errors
let processOrder 
    (checkInventory: ProductId list -> Async<Result<unit, InventoryError>>)
    (processPayment: Order -> Async<Result<PaymentConfirmation, PaymentError>>)
    (order: UnvalidatedOrder)
    : Async<Result<Invoice, OrderError>> =
    async {
        let validationResult =
            order
            |> validateOrder          // UnvalidatedOrder -> Result<Order, ValidationError>
            |> Result.mapError (fun e -> ValidationFailed [e])
        
        match validationResult with
        | Error e -> return Error e
        | Ok validatedOrder ->
            let productIds = validatedOrder.Items |> List.map (fun i -> i.ProductId)
            let! inventoryResult = checkInventory productIds
            
            match inventoryResult with
            | Error e -> return Error (InsufficientInventory (ProductId 0))
            | Ok () ->
                let! paymentResult = processPayment validatedOrder
                return 
                    paymentResult 
                    |> Result.map createInvoice
                    |> Result.mapError (fun e -> PaymentDeclined e.ToString())
    }
```

### AsyncResult Pattern

For operations that are both async and can fail:

```fsharp
// Helper module for AsyncResult
module AsyncResult =
    let bind (f: 'a -> Async<Result<'b, 'e>>) (asyncResult: Async<Result<'a, 'e>>) : Async<Result<'b, 'e>> =
        async {
            let! result = asyncResult
            match result with
            | Ok x -> return! f x
            | Error e -> return Error e
        }
    
    let map (f: 'a -> 'b) (asyncResult: Async<Result<'a, 'e>>) : Async<Result<'b, 'e>> =
        async {
            let! result = asyncResult
            return Result.map f result
        }
    
    let mapError (f: 'e1 -> 'e2) (asyncResult: Async<Result<'a, 'e1>>) : Async<Result<'a, 'e2>> =
        async {
            let! result = asyncResult
            return Result.mapError f result
        }

// Usage
let processOrder order =
    validateOrderAsync order
    |> AsyncResult.bind checkInventoryAsync
    |> AsyncResult.bind processPaymentAsync
    |> AsyncResult.map createInvoice
```

## Async-First for I/O Operations

**Principle:** All I/O operations should be async. Never block on async operations.

```fsharp
// ❌ BLOCKING - defeats async purpose
let getCustomerOrders customerId =
    async {
        let! customer = getCustomerAsync customerId
        let orders = getOrdersAsync customer.Id |> Async.RunSynchronously  // ❌ BLOCKING!
        return orders
    }

// ✅ ASYNC ALL THE WAY
let getCustomerOrders customerId =
    async {
        let! customer = getCustomerAsync customerId
        let! orders = getOrdersAsync customer.Id
        return orders
    }
```

### Async Function Signatures

```fsharp
// ✅ I/O operations return Async
let readFromDatabase: CustomerId -> Async<Customer>
let writeToDatabase: Customer -> Async<unit>
let callExternalApi: Request -> Async<Response>

// ✅ Pure computations don't need Async
let calculateTotal: Order -> decimal
let validateEmail: string -> Result<Email, ValidationError>
let transformData: Input -> Output
```

### Combining Multiple Async Operations

```fsharp
// Sequential operations
let processSequentially orders =
    async {
        let! results = 
            orders
            |> List.map processOrderAsync
            |> Async.Sequential
        return results
    }

// Parallel operations (when order doesn't matter)
let processInParallel orders =
    async {
        let! results = 
            orders
            |> List.map processOrderAsync
            |> Async.Parallel
        return results
    }

// Mixed sequential and parallel
let processOrderWithDetails orderId =
    async {
        let! order = getOrderAsync orderId
        
        // Fetch customer and inventory in parallel
        let! customer, inventory = 
            Async.Parallel2(
                getCustomerAsync order.CustomerId,
                getInventoryAsync order.Items
            )
        
        return createOrderDetails order customer inventory
    }
```

### Error Handling with Async

```fsharp
// ✅ Async with Result for expected errors
let placeOrder (order: UnvalidatedOrder) : Async<Result<OrderId, PlaceOrderError>> =
    async {
        let! validationResult = validateOrderAsync order
        
        match validationResult with
        | Error e -> return Error (ValidationFailed e)
        | Ok validOrder ->
            let! saveResult = saveOrderAsync validOrder
            return saveResult |> Result.mapError DatabaseError
    }

// ✅ Let unexpected errors bubble up
let readConfigFile path : Async<string> =
    async {
        try
            return! System.IO.File.ReadAllTextAsync(path) |> Async.AwaitTask
        with
        | :? System.IO.FileNotFoundException ->
            return failwith $"Configuration file not found: {path}"
    }
```

## Naming Conventions

**Principle:** Use business domain terminology and intention-revealing names. Never use generic programmer jargon.

### Forbidden Generic Names

**NEVER use these names partially or fully:**
- `data`
- `utils`
- `helpers`
- `common`
- `shared`
- `manager`
- `handler`
- `processor`

These names are meaningless - they tell you nothing about what the code actually does.

### Intention-Revealing Names

```fsharp
// ❌ GENERIC - meaningless
module DataProcessor =
    let processData data =
        DataUtils.transform data

// ✅ INTENTION-REVEALING - clear purpose
module OrderTotalCalculator =
    let calculateTotal (order: Order) : Money =
        TaxCalculator.applyTax order.Subtotal order.TaxRate
```

### F#-Specific Naming Conventions

- **Modules**: PascalCase, specific domain concepts (e.g., `OrderValidation`, `PaymentProcessing`)
- **Functions**: camelCase, verb phrases (e.g., `calculateDiscount`, `validateEmail`)
- **Types**: PascalCase, domain nouns (e.g., `Order`, `Customer`, `EmailAddress`)
- **DU cases**: PascalCase (e.g., `Pending`, `Confirmed`, `Shipped`)
- **Record fields**: PascalCase (e.g., `CustomerId`, `OrderDate`)
- **Function parameters**: camelCase (e.g., `customerId`, `orderTotal`)

### Naming Checklist

**For modules:**
- Does the name reveal the business capability?
- Is it a noun (or noun phrase) from the domain?
- Would a domain expert recognize this term?

**For functions:**
- Does the name reveal what the function does?
- Is it a verb (or verb phrase)?
- Does it describe the business operation?

**For types:**
- Does the name represent a domain concept?
- Is it specific to this context?
- Does it reveal constraints or business rules?

**For variables:**
- Does the name reveal what the variable contains?
- Is it specific to this context?
- Could someone understand it without reading the code?

## Type-Driven Design

**Principle:** Follow Scott Wlaschin's Domain Modeling Made Functional approach. Express domain concepts using the type system.

### Make Illegal States Unrepresentable

Use types to encode business rules:

```fsharp
// ❌ PRIMITIVE OBSESSION - illegal states possible
type Order = {
    Status: string  // Could be any string
    ShippedDate: DateTime option  // Could be set when Status <> "Shipped"
}

// ✅ TYPE-SAFE - illegal states impossible
type UnconfirmedOrder = {
    Items: OrderItem list
}

type ConfirmedOrder = {
    Items: OrderItem list
    ConfirmationNumber: ConfirmationNumber
}

type ShippedOrder = {
    Items: OrderItem list
    ConfirmationNumber: ConfirmationNumber
    ShippedDate: DateTime
}

type Order =
    | Unconfirmed of UnconfirmedOrder
    | Confirmed of ConfirmedOrder
    | Shipped of ShippedOrder
```

### Single-Case Unions for Primitives

Wrap primitives in single-case unions to add type safety:

```fsharp
// ✅ TYPE-SAFE - can't mix up different IDs
type CustomerId = CustomerId of Guid
type OrderId = OrderId of Guid
type ProductId = ProductId of int

// Compiler prevents mistakes
let getCustomer (customerId: CustomerId) : Async<Customer> = 
    async {
        let (CustomerId id) = customerId
        return! Database.getCustomer id
    }

let getOrder (orderId: OrderId) : Async<Order> = 
    async {
        let (OrderId id) = orderId
        return! Database.getOrder id
    }

// This won't compile:
// getCustomer orderId  ❌
```

### Smart Constructors for Validation

```fsharp
// ✅ TYPE-SAFE - validates at creation
type EmailAddress = private EmailAddress of string

module EmailAddress =
    let create (email: string) : Result<EmailAddress, ValidationError> =
        if String.IsNullOrWhiteSpace email then
            Error EmptyEmail
        elif not (email.Contains("@")) then
            Error (InvalidEmailFormat email)
        elif email.Length > 255 then
            Error (EmailTooLong email.Length)
        else
            Ok (EmailAddress email)
    
    let value (EmailAddress email) = email

// Can only create valid email addresses
let createCustomer emailString =
    result {
        let! email = EmailAddress.create emailString
        return { Email = email; /* ... */ }
    }
```

### Constrained Types

```fsharp
type PositiveInt = private PositiveInt of int

module PositiveInt =
    let create value : Result<PositiveInt, string> =
        if value > 0 then
            Ok (PositiveInt value)
        else
            Error $"Expected positive number, got {value}"
    
    let value (PositiveInt i) = i

type Quantity = private Quantity of PositiveInt

module Quantity =
    let create value =
        PositiveInt.create value
        |> Result.map Quantity
    
    let value (Quantity q) = PositiveInt.value q

// Can only be called with validated positive numbers
let calculateDiscount (price: PositiveInt) (rate: decimal) : decimal =
    decimal (PositiveInt.value price) * rate
```

## Prefer Immutability

**Principle:** F# defaults to immutability. Embrace it. Mutation should be rare and explicit.

### The Default: Immutable Records

```fsharp
// ✅ IMMUTABLE - predictable
type Order = {
    OrderId: OrderId
    Status: OrderStatus
    Items: OrderItem list
    Total: decimal
}

let addItem item order =
    { order with 
        Items = item :: order.Items
        Total = order.Total + item.Price }

let confirmOrder confirmationNumber order =
    { order with Status = Confirmed confirmationNumber }

// Caller controls what happens
let myOrder = getOrder()
let updatedOrder = myOrder |> addItem newItem |> confirmOrder confirmation
// myOrder unchanged, updatedOrder is new
```

### When Mutation Is Acceptable

Mutation is acceptable in limited scenarios:

```fsharp
// ✅ Local mutable for performance (encapsulated)
let sumLargeList items =
    let mutable total = 0
    for item in items do
        total <- total + item
    total

// ✅ Mutable collection builder (encapsulated)
let buildLookupTable items =
    let dict = System.Collections.Generic.Dictionary<string, Item>()
    for item in items do
        dict.[item.Key] <- item
    dict |> readOnlyDict  // Return immutable view
```

### Application Rules

- Use immutable records and discriminated unions by default
- Use `let` bindings (immutable) not `mutable`
- If you must use mutable state, encapsulate it in a minimal scope
- Prefer returning new values over mutation
- Use `with` syntax for record updates

## YAGNI - You Aren't Gonna Need It

**Principle:** Don't build features until they're actually needed. Speculative code is waste.

```fsharp
// ❌ YAGNI VIOLATION - over-engineered
type PaymentService = {
    ProcessPayment: Payment -> Async<Result<Receipt, PaymentError>>
    RefundPayment: Payment -> Async<Result<Receipt, RefundError>>
    PartialRefund: Payment -> Money -> Async<Result<Receipt, RefundError>>
    SchedulePayment: Payment -> DateTime -> Async<Result<ScheduledPayment, SchedulingError>>
    RecurringPayment: Payment -> Schedule -> Async<Result<Subscription, SubscriptionError>>
    // ... 10 more operations "we might need"
}

// Only ONE operation is actually used today

// ✅ YAGNI - build what you need
type ProcessPayment = Payment -> Async<Result<Receipt, PaymentError>>
```

### Application Rules

- Build the simplest thing that works
- Add capabilities when requirements demand them, not before
- "But we might need it" is not a requirement
- Don't create abstractions until you have at least two concrete implementations
- Start with functions, only introduce records of functions when you need multiple implementations

## Functional Error Handling Patterns

### Use Result for Expected Errors

```fsharp
// ✅ Expected business errors - use Result
type ValidationError = 
    | EmptyField of fieldName: string
    | InvalidFormat of fieldName: string * value: string
    | OutOfRange of fieldName: string * min: int * max: int * actual: int

let validateQuantity (quantity: int) : Result<Quantity, ValidationError> =
    if quantity <= 0 then
        Error (OutOfRange ("quantity", 1, 1000, quantity))
    elif quantity > 1000 then
        Error (OutOfRange ("quantity", 1, 1000, quantity))
    else
        Ok (Quantity quantity)
```

### Use Exceptions for Unexpected Errors

```fsharp
// ✅ Unexpected errors - use exceptions
let readConfigFile path : Async<string> =
    async {
        try
            return! System.IO.File.ReadAllTextAsync(path) |> Async.AwaitTask
        with
        | :? System.IO.FileNotFoundException ->
            return failwith $"Configuration file not found: {path}"
        | :? System.UnauthorizedAccessException ->
            return failwith $"Access denied to configuration file: {path}"
    }
```

### Don't Catch and Ignore

```fsharp
// ❌ SILENT FAILURE
let processData data =
    async {
        try
            return! someOperation data
        with
        | _ -> return ()  // Swallows errors!
    }

// ✅ EXPLICIT HANDLING
let processData data : Async<Result<ProcessedData, ProcessingError>> =
    async {
        try
            let! result = someOperation data
            return Ok result
        with
        | :? System.InvalidOperationException as ex ->
            return Error (ProcessingError.InvalidOperation ex.Message)
        | ex ->
            return Error (ProcessingError.UnexpectedError ex.Message)
    }
```

### Combining Results

```fsharp
// Validate multiple fields
let validateOrderForm form =
    result {
        let! customerId = validateCustomerId form.CustomerId
        let! items = validateItems form.Items
        let! deliveryAddress = validateAddress form.DeliveryAddress
        return {
            CustomerId = customerId
            Items = items
            DeliveryAddress = deliveryAddress
        }
    }

// Validate a list of items
let validateAllItems items =
    items
    |> List.map validateItem
    |> List.sequenceResultM  // Result<Item list, ValidationError list>
```

## When Tempted to Cut Corners

**STOP if you're about to:**
- Use `Option.orElse` chains → fail fast with Result instead
- Use `downcast` or `:?>` → fix the types, not the symptoms
- Create dependencies inside functions → inject through parameters or use partial application
- Name something `data`, `utils`, `handler` → use domain language
- Use mutable state unnecessarily → use immutable records with transformations
- Skip refactor because "it works" → refactor IS part of the work
- Write a comment → make the code self-explanatory
- Build "for later" → build what you need now
- Use exception for control flow → use Result type for expected errors
- Block on async operations with `RunSynchronously` → keep async all the way
- Create a "common" or "shared" module → put logic in the specific business capability it belongs to