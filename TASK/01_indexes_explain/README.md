# Лабораторная работа №1 — Индексы и EXPLAIN ANALYZE

## Содержимое папки

| Путь | Описание |
|------|----------|
| `sql/01_setup.sql` | Создание `lab1.orders`, генерация 1M строк |
| `sql/02_experiments.sql` | Задания 3–21 на тестовой таблице |
| `sql/03_project_baseline.sql` | EXPLAIN ANALYZE на `adoptions` проекта |
| `results/` | Логи выполнения |
| `контрольные_вопросы.md` | Ответы на контрольные вопросы |

## Отчёт

Шаблон указан в задании → отчёт:  
[`project/docs/lab-01-indexes.md`](../../project/docs/lab-01-indexes.md)

## Использование проекта

Scaling entity: **`adoptions`**.  
Индексы добавлены миграцией `project/liquibase/changelogs/012-lab1-adoptions-indexes.sql`.
