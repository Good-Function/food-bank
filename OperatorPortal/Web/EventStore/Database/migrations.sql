-- Event Store Table
-- Stores events for event-sourced aggregates with optimistic concurrency control
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

-- Index for efficient event loading by aggregate
CREATE INDEX IF NOT EXISTS idx_events_aggregate
    ON events(aggregate_type, aggregate_id, version);
