# Лабораторная работа №3 — Партиционирование PostgreSQL

## Содержимое

| Путь | Описание |
|------|----------|
| `sql/01_experiments.sql` | RANGE/LIST/HASH, pruning, индексы на `lab3.*` |
| `sql/02_partition_adoptions.sql` | Партиционирование `adoptions` (RANGE по месяцам) |
| `sql/03_api_explain_pruning.sql` | 3 API-запроса + pruning на `adoptions` |
| `sql/04_drop_old_demo.sql` | Демо удаления старых партиций |
| `sql/demo_alert_recovery.py` | CRITICAL → recovery alert с дедупликацией |
| `АНАЛИЗ_API_и_pruning.md` | Письменный разбор pruning / lifecycle |
| `results/` | Логи EXPLAIN / распределения |
| `контрольные_вопросы.md` | Ответы на контрольные вопросы |

## Отчёт

Основной отчёт:  
[`отчёты_лабораторных/lab3/отчёт.md`](../../отчёты_лабораторных/lab3/отчёт.md)

Разбор pruning: `АНАЛИЗ_API_и_pruning.md` / `отчёты_лабораторных/lab3/отчёт_анализ.md`.  
Контрольные ответы: `отчёты_лабораторных/lab3/контрольные_ответы.md`.

Автоматизация партиций: **не cron** — `PartitionMaintenanceHostedService` (BackgroundService, каждый час).

## Применение к проекту Animal Shelter

| Решение | Значение |
|---------|----------|
| Таблица | `adoptions` |
| Ключ | `created_at` |
| Стратегия | **RANGE** по месяцам |
| Почему | Растущий поток усыновлений; запросы с датами/списками; удобный lifecycle |
| Pruning | `WHERE created_at >= ... AND created_at < ...` |

Код в проекте:
- `Services/Partitioning/PartitionServices.cs` — `CreatePartitionsJob` (создание горизонта + **удаление** партиций старше 24 мес.), `PartitionHealthCheck`, alerter, hosted service
- `Controllers/PartitionsController.cs` — `POST /api/partitions/create-missing`, `GET /api/partitions/health`
- `liquibase/changelogs/013-ensure-adoption-partitions.sql` — досоздание горизонта
- Алерты: `project/alerts/*.txt`
- Разбор API/pruning: `АНАЛИЗ_API_и_pruning.md`

### Проверка сценария сбоя
1. Создать горизонт партиций  
2. `DROP TABLE adoptions_2026_12`  
3. HealthCheck → CRITICAL + файл алерта  
4. Создать партицию снова → recovery OK (без спама при повторных CRITICAL)
