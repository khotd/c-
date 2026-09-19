-- Demo: drop oldest month partition (lifecycle), then recreate via ensure
-- Retention policy illustration for Lab3 step7

\echo '=== Before drop: oldest partitions ==='
SELECT c.relname
FROM pg_inherits i
JOIN pg_class c ON c.oid = i.inhrelid
JOIN pg_class p ON p.oid = i.inhparent
WHERE p.relname = 'adoptions'
ORDER BY 1
LIMIT 5;

-- Detach/drop a very old partition if present (data may move to nowhere — demo only on empty-ish old)
DROP TABLE IF EXISTS adoptions_2024_01;
DROP TABLE IF EXISTS adoptions_2024_02;

\echo '=== After drop ==='
SELECT c.relname
FROM pg_inherits i
JOIN pg_class c ON c.oid = i.inhrelid
JOIN pg_class p ON p.oid = i.inhparent
WHERE p.relname = 'adoptions'
ORDER BY 1
LIMIT 8;
