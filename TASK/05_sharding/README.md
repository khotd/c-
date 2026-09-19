# Лабораторная работа №5 — Шардирование: Router и Consistent Hashing

## Содержимое

| Путь | Описание |
|------|----------|
| `scripts/shard_router.py` | Router: `hash(key) % N` + Consistent Hash Ring (vnodes) |
| `scripts/lab5_sharding.py` | Эксперимент: загрузка 100 000 записей `adoptions` в 3 шарда, расчёт 3→4 |
| `sql/01_create_schema.sql` | Схема `adoptions` на шарде (без межсерверных FK) |
| `sql/02_router_functions.sql` | Те же хеши/маршрутизация на чистом SQL (pg_temp, session-local) |
| `sql/03_distribution.sql` | Распределение записей по шардам |
| `sql/04_migration_calc.sql` | Расчёт перемещений 3→4: modulo и ring, сверка с Python |
| `results/lab5_run.md` | Фактический прогон (распределение, проценты перемещений) |
| `results/04_sql_check.md` | Лог сверки SQL vs Python |
| `контрольные_вопросы.md` | Ответы на контрольные вопросы |
| Отчёт | `отчёты_лабораторных/lab5/отчёт.md` |

## Суть эксперимента

- Сущность: **`adoptions`** (заявки на усыновление), shard key — **`user_id`**
- 3 независимых PostgreSQL: `project/docker-compose.shards.yml` →
  `shelter_shard0` :5445, `shelter_shard1` :5446, `shelter_shard2` :5447
  (+ `shelter_shard3` :5448 через `--profile four`), БД: `shelter_shard` (modulo)
  и `shelter_ring` (consistent), user/password: `shelter/shelter`
- 100 000 записей физически распределены обеими стратегиями

## Результаты

| Метрика | hash(key) % N | Consistent Hashing |
|---------|--------------:|-------------------:|
| Распределение (shard0/1/2) | 33 802 / 33 592 / 32 606 | 31 359 / 35 692 / 32 949 |
| Перемещается при 3→4 | **76 012 (76.01%)** | **27 806 (27.81%)** |

## Запуск

```bash
docker compose -f project/docker-compose.shards.yml up -d
python TASK/05_sharding/scripts/lab5_sharding.py
```

Сверка SQL (после прогона скрипта):

```bash
docker exec -i shelter_shard0 psql -U shelter -d shelter_shard \
  < TASK/05_sharding/sql/04_migration_calc.sql
```
