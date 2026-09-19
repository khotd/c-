-- Lab 3 experimental partitioning (schema lab3)
CREATE SCHEMA IF NOT EXISTS lab3;

-- RANGE by date
DROP TABLE IF EXISTS lab3.events CASCADE;
CREATE TABLE lab3.events (
    id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    event_type VARCHAR(50) NOT NULL,
    payload TEXT,
    created_at TIMESTAMP NOT NULL
) PARTITION BY RANGE (created_at);

CREATE TABLE lab3.events_2026_09_09 PARTITION OF lab3.events
FOR VALUES FROM ('2026-09-09') TO ('2026-09-10');
CREATE TABLE lab3.events_2026_09_10 PARTITION OF lab3.events
FOR VALUES FROM ('2026-09-10') TO ('2026-09-11');
CREATE TABLE lab3.events_2026_09_11 PARTITION OF lab3.events
FOR VALUES FROM ('2026-09-11') TO ('2026-09-12');

INSERT INTO lab3.events SELECT g, (random()*10000)::bigint, 'click', '{}', '2026-09-09 12:00:00' FROM generate_series(1,100000) g;
INSERT INTO lab3.events SELECT g+100000, (random()*10000)::bigint, 'view', '{}', '2026-09-10 12:00:00' FROM generate_series(1,100000) g;
INSERT INTO lab3.events SELECT g+200000, (random()*10000)::bigint, 'click', '{}', '2026-09-11 12:00:00' FROM generate_series(1,100000) g;

SELECT tableoid::regclass AS partition_name, COUNT(*) FROM lab3.events GROUP BY tableoid ORDER BY 1;

-- pruning
EXPLAIN (ANALYZE, BUFFERS)
SELECT COUNT(*) FROM lab3.events
WHERE created_at >= '2026-09-10' AND created_at < '2026-09-11';

EXPLAIN (ANALYZE, BUFFERS)
SELECT COUNT(*) FROM lab3.events WHERE event_type = 'click';

CREATE INDEX idx_lab3_events_user_id ON lab3.events (user_id);
CREATE INDEX idx_lab3_events_event_type ON lab3.events (event_type);

EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM lab3.events
WHERE created_at >= '2026-09-10' AND created_at < '2026-09-11' AND user_id = 12345;

EXPLAIN (ANALYZE, BUFFERS)
SELECT COUNT(*) FROM lab3.events WHERE event_type = 'click';

-- RANGE by price
DROP TABLE IF EXISTS lab3.products CASCADE;
CREATE TABLE lab3.products (
    id BIGINT NOT NULL, name TEXT NOT NULL, price NUMERIC NOT NULL
) PARTITION BY RANGE (price);
CREATE TABLE lab3.products_cheap PARTITION OF lab3.products FOR VALUES FROM (0) TO (100);
CREATE TABLE lab3.products_medium PARTITION OF lab3.products FOR VALUES FROM (100) TO (1000);
CREATE TABLE lab3.products_expensive PARTITION OF lab3.products FOR VALUES FROM (1000) TO (MAXVALUE);
INSERT INTO lab3.products VALUES (1,'Pen',10),(2,'Book',250),(3,'Laptop',45000);
EXPLAIN (ANALYZE, BUFFERS) SELECT * FROM lab3.products WHERE price >= 100 AND price < 500;

-- LIST
DROP TABLE IF EXISTS lab3.customers CASCADE;
CREATE TABLE lab3.customers (
    id BIGINT NOT NULL, name TEXT NOT NULL, customer_type VARCHAR(30) NOT NULL
) PARTITION BY LIST (customer_type);
CREATE TABLE lab3.customers_b2c PARTITION OF lab3.customers FOR VALUES IN ('B2C');
CREATE TABLE lab3.customers_b2b PARTITION OF lab3.customers FOR VALUES IN ('B2B');
CREATE TABLE lab3.customers_enterprise PARTITION OF lab3.customers FOR VALUES IN ('Enterprise');
CREATE TABLE lab3.customers_default PARTITION OF lab3.customers DEFAULT;
INSERT INTO lab3.customers VALUES (1,'A','B2C'),(2,'B','B2B'),(3,'C','Enterprise'),(100,'Test','VIP');
EXPLAIN (ANALYZE, BUFFERS) SELECT * FROM lab3.customers WHERE customer_type = 'B2B';

-- HASH
DROP TABLE IF EXISTS lab3.user_events CASCADE;
CREATE TABLE lab3.user_events (
    id BIGINT NOT NULL, user_id BIGINT NOT NULL, event_type VARCHAR(50), created_at TIMESTAMP NOT NULL
) PARTITION BY HASH (user_id);
CREATE TABLE lab3.user_events_0 PARTITION OF lab3.user_events FOR VALUES WITH (MODULUS 4, REMAINDER 0);
CREATE TABLE lab3.user_events_1 PARTITION OF lab3.user_events FOR VALUES WITH (MODULUS 4, REMAINDER 1);
CREATE TABLE lab3.user_events_2 PARTITION OF lab3.user_events FOR VALUES WITH (MODULUS 4, REMAINDER 2);
CREATE TABLE lab3.user_events_3 PARTITION OF lab3.user_events FOR VALUES WITH (MODULUS 4, REMAINDER 3);
INSERT INTO lab3.user_events SELECT g, (random()*100000)::bigint, 'LOGIN', NOW() FROM generate_series(1, 400000) g;
SELECT tableoid::regclass, COUNT(*) FROM lab3.user_events GROUP BY 1 ORDER BY 1;
