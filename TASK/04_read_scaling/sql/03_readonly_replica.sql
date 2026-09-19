-- Run on REPLICA — must fail (read-only standby)
INSERT INTO shelters (name, address, capacity, created_at)
VALUES ('should_fail_on_replica', 'x', 1, NOW());
