"""Lab 2 runner: generate events at checkpoints and measure EXPLAIN ANALYZE + sizes."""
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
    if p.returncode != 0:
        print(p.stderr, file=sys.stderr)
        raise SystemExit(p.returncode)
    return (p.stdout or "") + (p.stderr or "")


def insert_batch(n: int) -> None:
    print(f"Inserting {n} rows...")
    out = psql(f"""
INSERT INTO lab2.events (user_id, event_type, payload, created_at)
SELECT
    (random() * 100000)::bigint,
    CASE
        WHEN random() < 0.4 THEN 'MESSAGE'
        WHEN random() < 0.7 THEN 'LOGIN'
        WHEN random() < 0.9 THEN 'PURCHASE'
        ELSE 'OTHER'
    END,
    '{{}}'::jsonb,
    NOW() - (random() * INTERVAL '365 days')
FROM generate_series(1, {n});
ANALYZE lab2.events;
""")
    print(out[-200:])


def measure(label: str) -> str:
    sql = f"""
\\echo '=== SIZE {label} ==='
SELECT COUNT(*) AS rows FROM lab2.events;
SELECT pg_size_pretty(pg_relation_size('lab2.events')) AS table_size;
SELECT pg_size_pretty(pg_total_relation_size('lab2.events')) AS total_size;

\\echo '=== SELECT no index user_id={label} ==='
EXPLAIN ANALYZE SELECT * FROM lab2.events WHERE user_id = 123;

\\echo '=== RANGE created_at {label} ==='
EXPLAIN ANALYZE SELECT * FROM lab2.events WHERE created_at >= NOW() - INTERVAL '1 day';

\\echo '=== ORDER BY {label} ==='
EXPLAIN ANALYZE SELECT * FROM lab2.events WHERE user_id = 123 ORDER BY created_at DESC LIMIT 100;

\\echo '=== AGG {label} ==='
EXPLAIN ANALYZE
SELECT event_type, COUNT(*) FROM lab2.events
WHERE created_at >= NOW() - INTERVAL '30 days'
GROUP BY event_type;
"""
    return psql(sql)


def main():
    log = []
    psql(Path(__file__).with_name("01_create.sql").read_text(encoding="utf-8"))

    # cumulative checkpoints: 10k, 100k, 1m, 5m
    steps = [(10_000, "10k"), (90_000, "100k"), (900_000, "1m"), (4_000_000, "5m")]
    for n, label in steps:
        insert_batch(n)
        chunk = measure(label)
        (RESULTS / f"measure_{label}_noindex.log").write_text(chunk, encoding="utf-8")
        log.append(chunk)
        print(f"Measured {label}")

    # add indexes at 5m and remeasure
    idx = psql("""
CREATE INDEX idx_events_user_id ON lab2.events(user_id);
CREATE INDEX idx_events_created_at ON lab2.events(created_at);
CREATE INDEX idx_events_user_created ON lab2.events(user_id, created_at DESC);
ANALYZE lab2.events;

\\echo '=== AFTER INDEXES sizes ==='
SELECT indexrelname, pg_size_pretty(pg_relation_size(indexrelid)) AS index_size
FROM pg_stat_user_indexes WHERE relname = 'events' AND schemaname = 'lab2';

\\echo '=== SELECT with index ==='
EXPLAIN ANALYZE SELECT * FROM lab2.events WHERE user_id = 123;

\\echo '=== RANGE with index ==='
EXPLAIN ANALYZE SELECT * FROM lab2.events WHERE created_at >= NOW() - INTERVAL '1 day';

\\echo '=== ORDER BY with composite ==='
EXPLAIN ANALYZE SELECT * FROM lab2.events WHERE user_id = 123 ORDER BY created_at DESC LIMIT 100;

\\echo '=== AGG with created_at index ==='
EXPLAIN ANALYZE
SELECT event_type, COUNT(*) FROM lab2.events
WHERE created_at >= NOW() - INTERVAL '30 days'
GROUP BY event_type;

\\echo '=== INSERT cost with indexes (100k) ==='
\\timing on
INSERT INTO lab2.events (user_id, event_type, payload, created_at)
SELECT (random()*100000)::bigint, 'OTHER', '{}'::jsonb, NOW()
FROM generate_series(1, 100000);
\\timing off
""")
    (RESULTS / "measure_5m_with_indexes.log").write_text(idx, encoding="utf-8")

    # project part B on adoptions
    proj = psql("""
\\echo '=== PROJECT adoptions size ==='
SELECT COUNT(*) FROM adoptions;
SELECT pg_size_pretty(pg_relation_size('adoptions'));
SELECT pg_size_pretty(pg_total_relation_size('adoptions'));

\\echo '=== Q1 ==='
EXPLAIN ANALYZE SELECT * FROM adoptions WHERE user_id = 2;

\\echo '=== Q2 range ==='
EXPLAIN ANALYZE SELECT * FROM adoptions
WHERE created_at >= NOW() - INTERVAL '30 days'
  AND created_at < NOW();

\\echo '=== Q3 sort limit ==='
EXPLAIN ANALYZE SELECT * FROM adoptions
WHERE user_id = 2 ORDER BY created_at DESC LIMIT 50;

\\echo '=== indexes sizes adoptions ==='
SELECT indexrelname, pg_size_pretty(pg_relation_size(indexrelid))
FROM pg_stat_user_indexes WHERE relname = 'adoptions';
""")
    (RESULTS / "project_adoptions.log").write_text(proj, encoding="utf-8")
    print("Lab2 runner finished")


if __name__ == "__main__":
    main()
