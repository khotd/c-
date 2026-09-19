# Лаба 3 — разбор API / pruning / lifecycle

Сырые планы: `results/03_api_explain_pruning.log`, `results/04_drop_old_demo.log`.

## Три API-запроса проекта

### 1) Диапазон дат (список усыновлений за месяц) — pruning ЕСТЬ

Эндпоинт-аналог: фильтр `from`/`to` по `created_at`.

```sql
SELECT ... FROM adoptions
WHERE created_at >= '2026-03-01' AND created_at < '2026-04-01'
ORDER BY created_at DESC LIMIT 50;
```

В плане фактически читается **`adoptions_2026_03`** (Seq Scan ~21k строк, ~10 ms).  
Остальные месяцы отсекаются partition pruning — это главный выигрыш RANGE по дате.

### 2) GET по id — pruning СЛАБЫЙ / отсутствует

```sql
SELECT ... FROM adoptions WHERE id = 1000;
```

Ключ партиционирования — `created_at`, не `id`.  
Планировщик обходит **все** партиции (Index Scan по локальному PK каждой).  
Время всё ещё небольшое (~1.6 ms на 500k), но при росте числа партиций overhead планирования растёт.  
Вывод: partitioning не ускоряет lookup только по `id`.

### 3) Агрегация `GROUP BY status` — без даты pruning НЕТ

```sql
SELECT status, COUNT(*) FROM adoptions GROUP BY status;
```

Нужны все партиции (~60 ms). Индексы по `status` на партициях помогают (Index Only Scan), но **не заменяют** prune.

### 3b) Та же агрегация с окном дат — pruning ПОМОГАЕТ

```sql
WHERE created_at >= '2026-01-01' AND created_at < '2026-04-01'
GROUP BY status;
```

Читаются только партиции Jan–Mar 2026.

### JOIN `GET /api/users/{id}/adoptions`

Фильтр по `user_id` + сортировка по `created_at`: индекс `(user_id, status, created_at)` / bitmap на нужных партициях.  
Без условия по дате prune по месяцам ограничен.

## Создание / удаление партиций

- **Create:** `CreatePartitionsJob` — горизонт = текущий месяц + N (по умолчанию 3).  
- **Delete:** `DropOldPartitionsAsync` — DROP месячных партиций старше retention (24 месяца), кроме `adoptions_default`.  
- Демо ручного drop: `sql/04_drop_old_demo.sql`.

## Ответы на ключевые эксперименты lab3 schema

| Вопрос | Ответ |
|--------|--------|
| `2026-09-10 12:00` | `events_2026_09_10` |
| `2026-09-11 00:00` | `events_2026_09_11` (граница TO не включается) |
| Insert `2026-09-12` без партиции | ERROR (нет DEFAULT) |
| Pruning по дате | одна нужная партиция |
| `WHERE event_type='click'` | все партиции |
| DEFAULT | ловит неизвестные LIST-значения; риск превратиться в «свалку» |
| HASH | равномерно; плохо для «удалить старше 3 лет» |
