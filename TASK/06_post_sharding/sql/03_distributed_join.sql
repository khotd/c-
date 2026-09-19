-- Lab 6, п.4. JOIN между шардами: adoptions (шард) + animals/users/shelters (Primary)
--
-- Обычный SQL-запрос сервиса невозможен: таблицы на разных экземплярах
-- PostgreSQL, а межсерверных FK/JOIN в PostgreSQL нет.
--
-- Шаг 1 — на шарде пользователя (user_id=42 -> shard1):
SELECT id, animal_id, status, created_at
FROM adoptions
WHERE user_id = 42
ORDER BY created_at DESC
LIMIT 5;

-- Шаг 2 — на Primary (справочники лаб 1–4):
-- SELECT id, name, species FROM animals WHERE id IN (18, 43, 261, 354);
-- SELECT id, username FROM users WHERE id = 42;

-- Шаг 3 — merge в backend: собираем DTO (заявка + имя животного + юзер).
--
-- Что пришлось бы передавать между шардами: либо все заявки пользователя на
-- узел со справочником, либо весь справочник животных на каждый шард.
--
-- Co-location: если шаршировать animals по тому же ключу (owner/user_id),
-- JOIN заявки+животное чаще выполняется внутри одного шарда; но JOIN
-- «заявки пользователя + все животные приюта» всё равно остаётся
-- распределённым.
