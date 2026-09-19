-- Lab 6, п.2. Single-shard query.
-- Выполнить ТОЛЬКО на шарде, куда маршрутизирует роутер:
--   shard = hash32(user_id) % 3,  hash32 = ('x'||substr(md5(user_id::text),1,8))::bit(32)::bigint
-- Пример: user_id = 42 -> shard1 (см. results/lab6_run.md).

-- Запрос сервиса: AdoptionRepository.GetByUserIdAsync
--   (GET /api/users/{userId}/adoptions)
SELECT id, animal_id, status, adoption_date, created_at
FROM adoptions
WHERE user_id = 42
ORDER BY created_at DESC;

EXPLAIN (ANALYZE, BUFFERS)
SELECT COUNT(*) FROM adoptions WHERE user_id = 42;

-- Почему запросу не нужно обращаться к другим шардам:
-- роутер детерминирован, запись с user_id=42 физически не может находиться
-- в другом экземпляре. Локальный индекс (user_id) на шарде сохранён.
