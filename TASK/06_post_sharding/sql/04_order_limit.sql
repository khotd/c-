-- Lab 6, п.5. ORDER BY + LIMIT на трёх шардах.
-- Выполнить НА КАЖДОМ шарде, затем merge в backend.

SELECT id, user_id, created_at
FROM adoptions
ORDER BY created_at DESC
LIMIT 10;

-- Merge: отсортировать объединённый top-10 трёх шардов, взять первые 10.
-- Фактический прогон (results/lab6_run.md): глобальный top-3:
--   id=30 965 (shard?), id=75 840, id=67 834 — пришедшие с РАЗНЫХ шардов,
--   поэтому LIMIT 10 только с одного шарда дал бы неполный/неверный ответ.
--
-- Правило: с каждого шарда берём top-K (K = LIMIT), merge даёт точный top-K.
-- При K < LIMIT на шарде ответ может потерять кандидатов.
