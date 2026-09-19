-- Lab 6, п.3. Агрегирующий запрос: GET /api/adoptions/stats/by-status
-- Выполнить НА КАЖДОМ шарде, затем сложить результаты в backend (merge).

SELECT status, COUNT(*) AS count
FROM adoptions
GROUP BY status
ORDER BY count DESC;

-- Фактический прогон (см. results/lab6_run.md):
--   shard0: 24 944 / 24 915 / 25 063 / 25 078  (всего 33 802)
--   shard1: ...                                (всего 33 592)
--   shard2: ...                                (всего 32 606)
--   merge(sum): Pending 24 944, Approved 24 915, Completed 25 063,
--               Rejected 25 078  ->  100 000
--
-- Правила merge: COUNT и SUM складываются; MAX/MIN берётся по результатам;
-- AVG на шардах НЕ усредняется — нужен SUM и COUNT с каждого шарда.
-- На одном шарде такой запрос выполнить нельзя.
