-- Lab 1 experiments on lab1.orders
\echo '=== T3 EXPLAIN ==='
EXPLAIN SELECT * FROM lab1.orders WHERE user_id = 123;

\echo '=== T4 EXPLAIN ANALYZE ==='
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123;

\echo '=== T5 full ==='
EXPLAIN ANALYZE SELECT * FROM lab1.orders;
\echo '=== T5 amount>0 ==='
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE amount > 0;

\echo '=== T6 index user_id ==='
CREATE INDEX IF NOT EXISTS idx_orders_user_id ON lab1.orders(user_id);
ANALYZE lab1.orders;
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123;

\echo '=== T7 status ==='
CREATE INDEX IF NOT EXISTS idx_orders_status ON lab1.orders(status);
ANALYZE lab1.orders;
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE status = 'PAID';
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE status = 'NEW';
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE status = 'DELIVERED';
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE status = 'CANCELLED';

\echo '=== T8 distribution ==='
SELECT status, COUNT(*) FROM lab1.orders GROUP BY status;

\echo '=== T9 range ==='
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE created_at > NOW() - INTERVAL '7 days';
CREATE INDEX IF NOT EXISTS idx_orders_created_at ON lab1.orders(created_at);
ANALYZE lab1.orders;
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE created_at > NOW() - INTERVAL '7 days';
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE created_at > NOW() - INTERVAL '1 day';
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE created_at > NOW() - INTERVAL '1 month';
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE created_at > NOW() - INTERVAL '1 year';

\echo '=== T10 bitmap amount ==='
CREATE INDEX IF NOT EXISTS idx_orders_amount ON lab1.orders(amount);
ANALYZE lab1.orders;
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE amount BETWEEN 1000 AND 3000;

\echo '=== T11 multi ==='
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123 AND status = 'PAID';

\echo '=== T12 composite ==='
CREATE INDEX IF NOT EXISTS idx_orders_user_status ON lab1.orders(user_id, status);
ANALYZE lab1.orders;
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123 AND status = 'PAID';

\echo '=== T13 order ==='
CREATE INDEX IF NOT EXISTS idx_orders_user_created_at ON lab1.orders(user_id, created_at);
CREATE INDEX IF NOT EXISTS idx_orders_created_at_user ON lab1.orders(created_at, user_id);
ANALYZE lab1.orders;
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123;
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123 AND created_at > NOW() - INTERVAL '30 days';
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE created_at > NOW() - INTERVAL '30 days';

\echo '=== T14 sort ==='
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123 ORDER BY created_at DESC;
CREATE INDEX IF NOT EXISTS idx_orders_user_created_at_desc ON lab1.orders(user_id, created_at DESC);
ANALYZE lab1.orders;
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123 ORDER BY created_at DESC;

\echo '=== T15 pagination ==='
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE user_id = 123 ORDER BY created_at DESC LIMIT 20;

\echo '=== T16 index only ==='
EXPLAIN ANALYZE SELECT id, user_id FROM lab1.orders WHERE user_id = 123;
CREATE INDEX IF NOT EXISTS idx_orders_user_id_include ON lab1.orders(user_id) INCLUDE (id, status, created_at);
ANALYZE lab1.orders;
EXPLAIN ANALYZE SELECT id, user_id, status, created_at FROM lab1.orders WHERE user_id = 123;

\echo '=== T17 partial ==='
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE status = 'NEW' ORDER BY created_at;
CREATE INDEX IF NOT EXISTS idx_orders_new ON lab1.orders(created_at) WHERE status = 'NEW';
ANALYZE lab1.orders;
EXPLAIN ANALYZE SELECT * FROM lab1.orders WHERE status = 'NEW' ORDER BY created_at;

\echo '=== T18 expression ==='
DROP TABLE IF EXISTS lab1.demo_users;
CREATE TABLE lab1.demo_users (id BIGSERIAL PRIMARY KEY, email VARCHAR(255) NOT NULL);
INSERT INTO lab1.demo_users (email) SELECT 'User' || g || '@Example.COM' FROM generate_series(1, 100000) g;
INSERT INTO lab1.demo_users (email) VALUES ('test@example.com');
CREATE INDEX idx_demo_users_email ON lab1.demo_users(email);
ANALYZE lab1.demo_users;
EXPLAIN ANALYZE SELECT * FROM lab1.demo_users WHERE LOWER(email) = 'test@example.com';
CREATE INDEX idx_demo_users_lower_email ON lab1.demo_users(LOWER(email));
ANALYZE lab1.demo_users;
EXPLAIN ANALYZE SELECT * FROM lab1.demo_users WHERE LOWER(email) = 'test@example.com';

\echo '=== T19 insert cost ==='
DROP TABLE IF EXISTS lab1.orders_insert_test;
CREATE TABLE lab1.orders_insert_test (
    id BIGSERIAL PRIMARY KEY, user_id BIGINT NOT NULL, product_id BIGINT NOT NULL,
    status VARCHAR(20) NOT NULL, amount NUMERIC(10,2) NOT NULL,
    created_at TIMESTAMP NOT NULL, updated_at TIMESTAMP NOT NULL
);
\timing on
INSERT INTO lab1.orders_insert_test (user_id, product_id, status, amount, created_at, updated_at)
SELECT (random()*100000)::bigint, (random()*10000)::bigint, 'NEW', random()*100, NOW(), NOW()
FROM generate_series(1, 100000);
CREATE INDEX idx_ins_user ON lab1.orders_insert_test(user_id);
CREATE INDEX idx_ins_status ON lab1.orders_insert_test(status);
CREATE INDEX idx_ins_created ON lab1.orders_insert_test(created_at);
INSERT INTO lab1.orders_insert_test (user_id, product_id, status, amount, created_at, updated_at)
SELECT (random()*100000)::bigint, (random()*10000)::bigint, 'NEW', random()*100, NOW(), NOW()
FROM generate_series(1, 100000);
\timing off

\echo '=== T20 stats ==='
SELECT schemaname, relname, indexrelname, idx_scan
FROM pg_stat_user_indexes WHERE schemaname = 'lab1' ORDER BY idx_scan;

\echo '=== T21 final before ==='
EXPLAIN ANALYZE
SELECT id, amount, status, created_at FROM lab1.orders
WHERE user_id = 123 AND status = 'PAID' AND created_at >= NOW() - INTERVAL '30 days'
ORDER BY created_at DESC LIMIT 50;

CREATE INDEX IF NOT EXISTS idx_orders_user_status_created ON lab1.orders(user_id, status, created_at DESC);
ANALYZE lab1.orders;

\echo '=== T21 final after ==='
EXPLAIN ANALYZE
SELECT id, amount, status, created_at FROM lab1.orders
WHERE user_id = 123 AND status = 'PAID' AND created_at >= NOW() - INTERVAL '30 days'
ORDER BY created_at DESC LIMIT 50;
