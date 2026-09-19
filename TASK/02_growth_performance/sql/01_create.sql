-- Lab 2 Part A: growth experiment
CREATE SCHEMA IF NOT EXISTS lab2;
DROP TABLE IF EXISTS lab2.events CASCADE;
CREATE TABLE lab2.events (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    event_type VARCHAR(50) NOT NULL,
    payload JSONB,
    created_at TIMESTAMP NOT NULL
);
