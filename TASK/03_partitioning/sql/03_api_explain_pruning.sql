-- Lab3: 3 real API-like queries on partitioned adoptions + pruning analysis
\echo '=== API1 GET /api/adoptions?from&to  (date range -> pruning) ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT id, animal_id, user_id, status, created_at
FROM adoptions
WHERE created_at >= TIMESTAMP '2026-03-01'
  AND created_at <  TIMESTAMP '2026-04-01'
ORDER BY created_at DESC
LIMIT 50;

\echo '=== API2 GET /api/adoptions/{id} analogue (by id, no date) ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT id, animal_id, user_id, status, created_at
FROM adoptions
WHERE id = 1000;

\echo '=== API3 GET /api/adoptions/stats/by-status (aggregation, weak pruning) ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT status, COUNT(*) AS cnt
FROM adoptions
GROUP BY status
ORDER BY cnt DESC;

\echo '=== API3b stats with date window (pruning helps) ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT status, COUNT(*) AS cnt
FROM adoptions
WHERE created_at >= TIMESTAMP '2026-01-01'
  AND created_at <  TIMESTAMP '2026-04-01'
GROUP BY status;

\echo '=== API users/{id}/adoptions (JOIN + user filter) ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT ad.id, a.name AS animal_name, u.username, s.name AS shelter, ad.created_at
FROM adoptions ad
INNER JOIN animals a ON a.id = ad.animal_id
INNER JOIN users u ON u.id = ad.user_id
INNER JOIN shelters s ON s.id = a.shelter_id
WHERE ad.user_id = 2
ORDER BY ad.created_at DESC
LIMIT 50;

\echo '=== Which partitions exist ==='
SELECT c.relname AS partition_name
FROM pg_inherits i
JOIN pg_class c ON c.oid = i.inhrelid
JOIN pg_class p ON p.oid = i.inhparent
WHERE p.relname = 'adoptions'
ORDER BY 1;
