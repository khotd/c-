"""Fill Lab2 gaps: index@100k/1m/5m, INSERT before/after, project entity volumes."""
from __future__ import annotations

import subprocess
import sys
from pathlib import Path

RESULTS = Path(__file__).resolve().parents[1] / "results"
RESULTS.mkdir(exist_ok=True)


def psql(sql: str) -> str:
    p = subprocess.run(
        [
            "docker", "exec", "-i", "animal-shelter-db",
            "psql", "-U", "project", "-d", "projectDB", "-v", "ON_ERROR_STOP=1",
        ],
        input=sql,
        text=True,
        capture_output=True,
        encoding="utf-8",
        errors="replace",
    )
    out = (p.stdout or "") + (p.stderr or "")
    if p.returncode != 0:
        print(out, file=sys.stderr)
        raise SystemExit(p.returncode)
    return out


def measure_select(label: str) -> str:
    return psql(f"""
\\echo '=== {label} SIZE ==='
SELECT COUNT(*) AS rows FROM lab2.events;
SELECT pg_size_pretty(pg_relation_size('lab2.events')) AS table_size;
SELECT pg_size_pretty(pg_total_relation_size('lab2.events')) AS total_size;

\\echo '=== {label} SELECT user_id ==='
EXPLAIN ANALYZE SELECT * FROM lab2.events WHERE user_id = 123;

\\echo '=== {label} RANGE ==='
EXPLAIN ANALYZE SELECT * FROM lab2.events WHERE created_at >= NOW() - INTERVAL '1 day';

\\echo '=== {label} ORDER LIMIT ==='
EXPLAIN ANALYZE SELECT * FROM lab2.events WHERE user_id = 123 ORDER BY created_at DESC LIMIT 100;
""")


def create_indexes() -> str:
    return psql("""
CREATE INDEX IF NOT EXISTS idx_events_user_id ON lab2.events(user_id);
CREATE INDEX IF NOT EXISTS idx_events_created_at ON lab2.events(created_at);
CREATE INDEX IF NOT EXISTS idx_events_user_created ON lab2.events(user_id, created_at DESC);
ANALYZE lab2.events;
SELECT indexrelname, pg_size_pretty(pg_relation_size(indexrelid)) AS index_size
FROM pg_stat_user_indexes WHERE schemaname='lab2' AND relname='events';
""")


def drop_indexes() -> str:
    return psql("""
DROP INDEX IF EXISTS lab2.idx_events_user_id;
DROP INDEX IF EXISTS lab2.idx_events_created_at;
DROP INDEX IF EXISTS lab2.idx_events_user_created;
""")


def insert_n(n: int) -> None:
    print(f"Insert {n} into lab2.events...")
    psql(f"""
INSERT INTO lab2.events (user_id, event_type, payload, created_at)
SELECT
    (random() * 100000)::bigint,
    CASE WHEN random() < 0.4 THEN 'MESSAGE' WHEN random() < 0.7 THEN 'LOGIN'
         WHEN random() < 0.9 THEN 'PURCHASE' ELSE 'OTHER' END,
    '{{}}'::jsonb,
    NOW() - (random() * INTERVAL '365 days')
FROM generate_series(1, {n});
ANALYZE lab2.events;
""")


def part_a():
    log = []
    print("=== PART A recreate events ===")
    psql("""
CREATE SCHEMA IF NOT EXISTS lab2;
DROP TABLE IF EXISTS lab2.events CASCADE;
CREATE TABLE lab2.events (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    event_type VARCHAR(50) NOT NULL,
    payload JSONB,
    created_at TIMESTAMP NOT NULL
);
""")

    # INSERT cost without indexes at empty-ish: insert 100k baseline
    print("INSERT 100k WITHOUT indexes...")
    t = psql("""
\\timing on
INSERT INTO lab2.events (user_id, event_type, payload, created_at)
SELECT (random()*100000)::bigint, 'OTHER', '{}'::jsonb, NOW()
FROM generate_series(1, 100000);
\\timing off
ANALYZE lab2.events;
""")
    (RESULTS / "insert_100k_no_indexes.log").write_text(t, encoding="utf-8")
    log.append(t)

    # Reset for checkpoints: truncate and build cumulatively
    psql("TRUNCATE lab2.events RESTART IDENTITY;")

    steps = [(100_000, "100k"), (900_000, "1m"), (4_000_000, "5m")]
    # also include 10k first for completeness
    steps = [(10_000, "10k")] + steps

    for n, label in steps:
        insert_n(n)
        no = measure_select(f"{label}_NOINDEX")
        (RESULTS / f"gap_{label}_noindex.log").write_text(no, encoding="utf-8")
        create_indexes()
        yes = measure_select(f"{label}_WITHINDEX")
        (RESULTS / f"gap_{label}_withindex.log").write_text(yes, encoding="utf-8")
        if label != "5m":
            drop_indexes()
        print(f"Done checkpoint {label}")

    # INSERT 100k WITH indexes (at 5m)
    print("INSERT 100k WITH indexes...")
    t2 = psql("""
\\timing on
INSERT INTO lab2.events (user_id, event_type, payload, created_at)
SELECT (random()*100000)::bigint, 'OTHER', '{}'::jsonb, NOW()
FROM generate_series(1, 100000);
\\timing off
""")
    (RESULTS / "insert_100k_with_indexes.log").write_text(t2, encoding="utf-8")

    sizes = psql("""
SELECT indexrelname, pg_size_pretty(pg_relation_size(indexrelid)) AS index_size
FROM pg_stat_user_indexes WHERE schemaname='lab2' AND relname='events';
""")
    (RESULTS / "gap_index_sizes_5m.log").write_text(sizes, encoding="utf-8")
    print("PART A done")


def part_b():
    """Growth experiment on project scaling entity (adoptions) via bench table + live growth."""
    print("=== PART B adoptions volumes ===")
    psql("""
CREATE SCHEMA IF NOT EXISTS lab2;
DROP TABLE IF EXISTS lab2.adoptions_growth CASCADE;
CREATE TABLE lab2.adoptions_growth (
    id BIGSERIAL PRIMARY KEY,
    animal_id INT NOT NULL,
    user_id INT NOT NULL,
    adoption_date DATE NOT NULL,
    status VARCHAR(50) NOT NULL,
    notes TEXT,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP
);
""")

    def fill_to(target: int):
        cur = int(psql("SELECT COUNT(*) FROM lab2.adoptions_growth;").splitlines()[2].strip() or "0")
        # parse carefully
        out = psql("SELECT COUNT(*)::text FROM lab2.adoptions_growth;")
        # last non-empty data line
        lines = [l.strip() for l in out.splitlines() if l.strip() and l.strip().isdigit()]
        cur = int(lines[0]) if lines else 0
        need = target - cur
        if need <= 0:
            return
        print(f"adoptions_growth: {cur} -> {target} (+{need})")
        psql(f"""
INSERT INTO lab2.adoptions_growth (animal_id, user_id, adoption_date, status, notes, created_at)
SELECT
    ((random()*50000)::int + 1),
    ((random()*5000)::int + 1),
    (CURRENT_DATE - (random()*730)::int),
    (ARRAY['Pending','Approved','Completed','Rejected'])[floor(random()*4+1)],
    'bench',
    NOW() - (random() * INTERVAL '730 days')
FROM generate_series(1, {need});
ANALYZE lab2.adoptions_growth;
""")

    def measure(label: str) -> str:
        return psql(f"""
\\echo '=== ADOPTIONS_GROWTH {label} ==='
SELECT COUNT(*) FROM lab2.adoptions_growth;
SELECT pg_size_pretty(pg_relation_size('lab2.adoptions_growth'));
SELECT pg_size_pretty(pg_total_relation_size('lab2.adoptions_growth'));

\\echo 'Q1 user_id'
EXPLAIN ANALYZE SELECT * FROM lab2.adoptions_growth WHERE user_id = 2;

\\echo 'Q2 date range'
EXPLAIN ANALYZE SELECT * FROM lab2.adoptions_growth
WHERE created_at >= NOW() - INTERVAL '30 days' AND created_at < NOW();

\\echo 'Q3 filter sort limit'
EXPLAIN ANALYZE SELECT * FROM lab2.adoptions_growth
WHERE user_id = 2 ORDER BY created_at DESC LIMIT 50;
""")

    for target, label in [(100_000, "100k"), (1_000_000, "1m"), (5_000_000, "5m")]:
        fill_to(target)
        # no extra index first
        psql("DROP INDEX IF EXISTS lab2.idx_ag_user; DROP INDEX IF EXISTS lab2.idx_ag_created; DROP INDEX IF EXISTS lab2.idx_ag_user_created;")
        m1 = measure(f"{label}_noindex")
        (RESULTS / f"project_growth_{label}_noindex.log").write_text(m1, encoding="utf-8")
        psql("""
CREATE INDEX idx_ag_user ON lab2.adoptions_growth(user_id);
CREATE INDEX idx_ag_created ON lab2.adoptions_growth(created_at);
CREATE INDEX idx_ag_user_created ON lab2.adoptions_growth(user_id, created_at DESC);
ANALYZE lab2.adoptions_growth;
""")
        m2 = measure(f"{label}_withindex")
        (RESULTS / f"project_growth_{label}_withindex.log").write_text(m2, encoding="utf-8")
        print(f"Project growth {label} done")

    # Also measure LIVE partitioned adoptions at current count + after grow to 1m then 5m
    live = psql("""
\\echo '=== LIVE adoptions now ==='
SELECT COUNT(*) FROM adoptions;
EXPLAIN ANALYZE SELECT * FROM adoptions WHERE user_id = 2;
EXPLAIN ANALYZE SELECT * FROM adoptions
WHERE created_at >= NOW() - INTERVAL '30 days' AND created_at < NOW();
EXPLAIN ANALYZE SELECT * FROM adoptions WHERE user_id = 2 ORDER BY created_at DESC LIMIT 50;
""")
    (RESULTS / "project_live_current.log").write_text(live, encoding="utf-8")

    # grow live adoptions toward 1m and 5m (partitioned)
    def live_count() -> int:
        out = psql("SELECT COUNT(*)::text FROM adoptions;")
        lines = [l.strip() for l in out.splitlines() if l.strip().isdigit()]
        return int(lines[0])

    def grow_live(target: int):
        cur = live_count()
        need = target - cur
        if need <= 0:
            print(f"live adoptions already {cur} >= {target}")
            return
        print(f"Grow live adoptions {cur} -> {target} (+{need})")
        # one-shot insert using arrays of valid FKs (fast, FK-safe)
        psql(f"""
WITH a AS (SELECT array_agg(id) AS ids FROM animals),
     u AS (SELECT array_agg(id) AS ids FROM users)
INSERT INTO adoptions (animal_id, user_id, adoption_date, status, notes, created_at)
SELECT
    a.ids[1 + (g % array_length(a.ids, 1))],
    u.ids[1 + (g % array_length(u.ids, 1))],
    CURRENT_DATE - ((g % 700)),
    (ARRAY['Pending','Approved','Completed','Rejected'])[1 + (g % 4)],
    'scale',
    TIMESTAMP '2024-09-15' + ((g % 700) || ' days')::interval
FROM generate_series(1, {need}) g, a, u;
ANALYZE adoptions;
""")
        print(f"  live now {live_count()}")

    grow_live(1_000_000)
    live1 = psql("""
\\echo '=== LIVE 1m ==='
SELECT COUNT(*) FROM adoptions;
EXPLAIN ANALYZE SELECT * FROM adoptions WHERE user_id = 2;
EXPLAIN ANALYZE SELECT * FROM adoptions
WHERE created_at >= NOW() - INTERVAL '30 days' AND created_at < NOW();
EXPLAIN ANALYZE SELECT * FROM adoptions WHERE user_id = 2 ORDER BY created_at DESC LIMIT 50;
""")
    (RESULTS / "project_live_1m.log").write_text(live1, encoding="utf-8")

    grow_live(5_000_000)
    live5 = psql("""
\\echo '=== LIVE 5m ==='
SELECT COUNT(*) FROM adoptions;
EXPLAIN ANALYZE SELECT * FROM adoptions WHERE user_id = 2;
EXPLAIN ANALYZE SELECT * FROM adoptions
WHERE created_at >= NOW() - INTERVAL '30 days' AND created_at < NOW();
EXPLAIN ANALYZE SELECT * FROM adoptions WHERE user_id = 2 ORDER BY created_at DESC LIMIT 50;
""")
    (RESULTS / "project_live_5m.log").write_text(live5, encoding="utf-8")
    print("PART B done")


def write_answers():
    # gather key numbers into markdown answers file
    text = Path(RESULTS / "gap_100k_noindex.log").read_text(encoding="utf-8", errors="replace") if (RESULTS / "gap_100k_noindex.log").exists() else ""
    md = """# Лаба 2 — дозаполнение пробелов

## Задание 5. Индекс на разных объёмах (events)

См. `results/gap_*_noindex.log` и `gap_*_withindex.log`.

## Задание 9. INSERT до/после индексов

- `insert_100k_no_indexes.log`
- `insert_100k_with_indexes.log`

## Задание 11. Когда индекс не спасает (письменно)

Запрос вида:
```sql
SELECT DATE(created_at), COUNT(*)
FROM events
WHERE created_at >= NOW() - INTERVAL '365 days'
GROUP BY DATE(created_at);
```
на ~100 млн строк с индексом по `created_at`:

1. Индекс может сузить диапазон, но за год это почти вся «живая» таблица — нужно прочитать огромный объём TID/heap или сделать Seq Scan + HashAggregate.
2. Обрабатываются сотни миллионов строк / дни * count — дорого по I/O и CPU.
3. Индекс не делает агрегацию бесплатной: каждая подходящая строка всё равно участвует в GROUP BY.
4. Решения: партиционирование по дате + prune, предагрегированные rollup-таблицы, материализованные представления, архивация старых данных, колоночные/OLAP движки, ограничение окна анализа.

## Часть B (свой сервис)

Сущность: **adoptions** (и зеркало `lab2.adoptions_growth` для контролируемых 100k/1m/5m).

Логи:
- `project_growth_{100k,1m,5m}_{noindex|withindex}.log`
- `project_live_current.log`, `project_live_1m.log`, `project_live_5m.log`

### Узкое место
Запрос по диапазону дат без высокой селективности (`created_at` за 30 дней) сильнее всего растёт с объёмом (часто Parallel Seq Scan / большой Bitmap), тогда как `user_id + LIMIT` остаётся быстрым с индексом.

### Улучшение
Составной индекс `(user_id, created_at DESC)` и/или `(created_at)` + партиционирование по `created_at` (уже внедрено в проекте, лаба 3).
"""
    (RESULTS.parent / "ДОПОЛНЕНИЕ_пробелы.md").write_text(md, encoding="utf-8")


if __name__ == "__main__":
    part_a()
    part_b()
    write_answers()
    print("ALL LAB2 GAPS DONE")
