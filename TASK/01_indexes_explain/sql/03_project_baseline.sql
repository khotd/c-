-- Lab 1 Part B: queries on project scaling entity (adoptions)
\echo '=== indexes on adoptions ==='
SELECT indexname, indexdef FROM pg_indexes WHERE tablename = 'adoptions';

\echo '=== Q1 by user_id ==='
EXPLAIN ANALYZE SELECT * FROM adoptions WHERE user_id = 2;

\echo '=== Q2 multi conditions ==='
EXPLAIN ANALYZE
SELECT * FROM adoptions
WHERE user_id = 2 AND status = 'Pending' AND created_at >= NOW() - INTERVAL '365 days';

\echo '=== Q3 filter+sort+limit ==='
EXPLAIN ANALYZE
SELECT * FROM adoptions
WHERE user_id = 2
ORDER BY created_at DESC
LIMIT 50;

\echo '=== Q JOIN detailed ==='
EXPLAIN ANALYZE
SELECT ad.id, a.name, u.username, s.name AS shelter
FROM adoptions ad
INNER JOIN animals a ON a.id = ad.animal_id
INNER JOIN users u ON u.id = ad.user_id
INNER JOIN shelters s ON s.id = a.shelter_id
WHERE ad.user_id = 2
ORDER BY ad.created_at DESC
LIMIT 50;

\echo '=== index usage ==='
SELECT schemaname, relname, indexrelname, idx_scan
FROM pg_stat_user_indexes
WHERE relname IN ('adoptions','animals','users','shelters')
ORDER BY idx_scan DESC;
