CREATE TABLE IF NOT EXISTS audit_trail (
    id SERIAL PRIMARY KEY,
    related_entity_id BIGINT NOT NULL,
    who TEXT NOT NULL,
    occured_at TIMESTAMP NOT NULL,
    kind TEXT NOT NULL,
    diff JSONB
);