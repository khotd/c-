# Лабораторная работа №6 — Сервис после шардирования

## Содержимое

| Путь | Описание |
|------|----------|
| `scripts/lab6_checks.py` | Живые проверки: single-shard, агрегация, JOIN, ORDER BY+LIMIT, отказ шарда, hot shard |
| `sql/01_single_shard.sql` | Запрос по shard key на одном шарде (+EXPLAIN) |
| `sql/02_distributed_aggregation.sql` | COUNT/GROUP BY на каждом шарде + правила merge |
| `sql/03_distributed_join.sql` | Почему JOIN не выполняется обычным SQL; паттерн «2 запроса + merge» |
| `sql/04_order_limit.sql` | Топ-N с каждого шарда + merge |
| `results/lab6_run.md` | Фактический прогон (включая docker stop/start shard2) |
| `контрольные_вопросы.md` | Ответы на 11 контрольных вопросов |
| Отчёт | `отчёты_лабораторных/lab6/отчёт.md` |

## Запуск

```bash
# шарды лабы 5 должны быть запущены и заполнены
python TASK/06_post_sharding/scripts/lab6_checks.py          # с тестом отказа
python TASK/06_post_sharding/scripts/lab6_checks.py --skip-failure
```

Скрипт сам останавливает и запускает `shelter_shard2` (раздел 6),
в конце все шарды снова должны быть healthy.
