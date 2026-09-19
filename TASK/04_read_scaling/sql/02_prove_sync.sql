-- Run on PRIMARY, then SELECT on REPLICA
INSERT INTO shelters (name, address, phone, email, capacity, created_at)
VALUES ('lab4_replication_probe', 'Read Scaling Lab', '000', 'lab4@example.com', 5, NOW())
RETURNING id, name;

-- On REPLICA:
-- SELECT id, name FROM shelters WHERE name = 'lab4_replication_probe';
