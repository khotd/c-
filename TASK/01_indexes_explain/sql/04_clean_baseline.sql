-- Lab1 clean baseline: recreate orders WITHOUT indexes, measure, then add indexes
CREATE SCHEMA IF NOT EXISTS lab1;
DROP TABLE IF EXISTS lab1.orders CASCADE;

CREATE TABLE lab1.orders (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    product_id BIGINT NOT NULL,
    status VARCHAR(20) NOT NULL,
    amount NUMERIC(10, 2) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

INSERT INTO lab1.orders (user_id, product_id, status, amount, created_at, updated_at)
SELECT
    (random() * 100000)::BIGINT,
    (random() * 10000)::BIGINT,
    (ARRAY['NEW', 'PAID', 'DELIVERED', 'CANCELLED'])[floor(random() * 4 + 1)],
    random() * 10000,
    NOW() - (random() * INTERVAL '2 years'),
    NOW()
FROM generate_series(1, 1000000);
ANALYZE lab1.orders;

\echo '=== CLEAN BEFORE: EXPLAIN ==='
EXPLAIN SELECT * FROM lab1.orders WHERE user_id = 123;

\echo '=== CLEAN BEFORE: EXPLAIN ANALYZE ==='
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123;

\echo '=== CLEAN BEFORE: status PAID ==='
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE status = 'PAID';

\echo '=== CLEAN BEFORE: pagination ==='
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123 ORDER BY created_at DESC LIMIT 20;

\echo '=== CLEAN BEFORE: multi AND ==='
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123 AND status = 'PAID';

\echo '=== CREATE idx_orders_user_id ==='
CREATE INDEX idx_orders_user_id ON lab1.orders(user_id);
ANALYZE lab1.orders;

\echo '=== AFTER user_id index ==='
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123;

\echo '=== CREATE status + measure BitmapAnd path ==='
CREATE INDEX idx_orders_status ON lab1.orders(status);
ANALYZE lab1.orders;
-- Force separate indexes path by not creating composite yet
SET enable_indexscan = off;
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123 AND status = 'PAID';
RESET enable_indexscan;

\echo '=== CREATE composite and compare ==='
CREATE INDEX idx_orders_user_status ON lab1.orders(user_id, status);
ANALYZE lab1.orders;
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123 AND status = 'PAID';

\echo '=== ORDER BY before desc index ==='
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123 ORDER BY created_at DESC;
CREATE INDEX idx_orders_user_created_at_desc ON lab1.orders(user_id, created_at DESC);
ANALYZE lab1.orders;
\echo '=== ORDER BY after desc index ==='
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123 ORDER BY created_at DESC LIMIT 20;

\echo '=== PARTIAL: make NEW rare (~5%) then test ==='
-- Note: distribution is ~25%; show that planner may ignore partial when large
CREATE INDEX idx_orders_created_at ON lab1.orders(created_at);
CREATE INDEX idx_orders_new ON lab1.orders(created_at) WHERE status = 'NEW';
ANALYZE lab1.orders;
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE status = 'NEW' ORDER BY created_at LIMIT 100;

\echo '=== INDEX ONLY ==='
CREATE INDEX idx_orders_user_id_include ON lab1.orders(user_id) INCLUDE (id, status, created_at);
ANALYZE lab1.orders;
EXPLAIN ANALYZE SELECT id, user_id, status, created_at FROM lab1.orders WHERE user_id = 123;

\echo '=== FINAL query optimize ==='
EXPLAIN ANALYZE
SELECT id, amount, status, created_at FROM lab1.orders
WHERE user_id = 123 AND status = 'PAID' AND created_at >= NOW() - INTERVAL '30 days'
ORDER BY created_at DESC LIMIT 50;
CREATE INDEX idx_orders_user_status_created ON lab1.orders(user_id, status, created_at DESC);
ANALYZE lab1.orders;
EXPLAIN ANALYZE
SELECT id, amount, status, created_at FROM lab1.orders
WHERE user_id = 123 AND status = 'PAID' AND created_at >= NOW() - INTERVAL '30 days'
ORDER BY created_at DESC LIMIT 50;
