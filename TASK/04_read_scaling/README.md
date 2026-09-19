# Лабораторная работа №4 — Read Scaling (Primary + Replica)

## Содержимое

| Путь | Описание |
|------|----------|
| `sql/01_check_replication.sql` | `pg_stat_replication`, lag |
| `sql/02_prove_sync.sql` | запись на Primary → SELECT на Replica |
| `sql/03_readonly_replica.sql` | INSERT на Replica (ожидаем ошибку) |
| `scripts/run_checks.py` | прогон проверок через docker |
| `results/` | логи проверок |
| `контрольные_вопросы.md` | ответы на контрольные |
| Отчёт | `отчёты_лабораторных/lab4/отчёт.md` |

## Что сделано в проекте

- `docker-compose.yml`: **Primary** `db` (:5432), **Replica** `db-replica` (:5433), setup-скрипт репликации
- Streaming replication (WAL): `wal_level=replica`, роль `replicator`
- Backend: `ConnectionStrings:ReplicaConnection`
- Read Scaling: `GET /api/animals` (и related list reads) → Replica; запись → Primary
