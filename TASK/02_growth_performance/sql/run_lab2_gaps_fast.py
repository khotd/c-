"""Faster lab2 gap fill: separate tables per volume + INSERT timing + project growth."""
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
        sys.stderr.write(out)
        raise SystemExit(p.returncode)
    return out


def main():
    print("start", flush=True)
    psql("CREATE SCHEMA IF NOT EXISTS lab2;")

    # --- INSERT cost before/after indexes ---
    psql("""
DROP TABLE IF EXISTS lab2.events_ins CASCADE;
CREATE TABLE lab2.events_ins (
  id BIGSERIAL PRIMARY KEY,
  user_id BIGINT NOT NULL,
  event_type VARCHAR(50) NOT NULL,
  payload JSONB,
  created_at TIMESTAMP NOT NULL
);
""")
    print("timing INSERT 100k no indexes", flush=True)
    t0 = psql("""
\\timing on
INSERT INTO lab2.events_ins (user_id, event_type, payload, created_at)
SELECT (random()*100000)::bigint, 'OTHER', '{}'::jsonb, NOW() FROM generate_series(1,100000);
\\timing off
""")
    (RESULTS / "insert_100k_no_indexes.log").write_text(t0, encoding="utf-8")

    psql("""
CREATE INDEX ON lab2.events_ins(user_id);
CREATE INDEX ON lab2.events_ins(created_at);
CREATE INDEX ON lab2.events_ins(user_id, created_at DESC);
TRUNCATE lab2.events_ins RESTART IDENTITY;
""")
    print("timing INSERT 100k with indexes", flush=True)
    t1 = psql("""
\\timing on
INSERT INTO lab2.events_ins (user_id, event_type, payload, created_at)
SELECT (random()*100000)::bigint, 'OTHER', '{}'::jsonb, NOW() FROM generate_series(1,100000);
\\timing off
""")
    (RESULTS / "insert_100k_with_indexes.log").write_text(t1, encoding="utf-8")

    # --- Per-volume events tables ---
    for n, label in [(100_000, "100k"), (1_000_000, "1m"), (5_000_000, "5m")]:
        tbl = f"lab2.events_{label}"
        print(f"build {tbl} n={n}", flush=True)
        psql(f"""
DROP TABLE IF EXISTS {tbl} CASCADE;
CREATE TABLE {tbl} (
  id BIGSERIAL PRIMARY KEY,
  user_id BIGINT NOT NULL,
  event_type VARCHAR(50) NOT NULL,
  payload JSONB,
  created_at TIMESTAMP NOT NULL
);
INSERT INTO {tbl} (user_id, event_type, payload, created_at)
SELECT (random()*100000)::bigint,
  CASE WHEN random()<0.4 THEN 'MESSAGE' WHEN random()<0.7 THEN 'LOGIN'
       WHEN random()<0.9 THEN 'PURCHASE' ELSE 'OTHER' END,
  '{{}}'::jsonb,
  NOW() - (random()*INTERVAL '365 days')
FROM generate_series(1,{n});
ANALYZE {tbl};
""")
        no = psql(f"""
\\echo '=== {label} NOINDEX SIZE ==='
SELECT COUNT(*) FROM {tbl};
SELECT pg_size_pretty(pg_relation_size('{tbl}'));
SELECT pg_size_pretty(pg_total_relation_size('{tbl}'));
\\echo '=== SELECT ==='
EXPLAIN ANALYZE SELECT * FROM {tbl} WHERE user_id = 123;
\\echo '=== RANGE ==='
EXPLAIN ANALYZE SELECT * FROM {tbl} WHERE created_at >= NOW() - INTERVAL '1 day';
\\echo '=== ORDER ==='
EXPLAIN ANALYZE SELECT * FROM {tbl} WHERE user_id = 123 ORDER BY created_at DESC LIMIT 100;
""")
        (RESULTS / f"gap_{label}_noindex.log").write_text(no, encoding="utf-8")

        psql(f"""
CREATE INDEX ON {tbl}(user_id);
CREATE INDEX ON {tbl}(created_at);
CREATE INDEX ON {tbl}(user_id, created_at DESC);
ANALYZE {tbl};
""")
        yes = psql(f"""
\\echo '=== {label} WITHINDEX ==='
SELECT indexrelname, pg_size_pretty(pg_relation_size(indexrelid))
FROM pg_stat_user_indexes WHERE relid = '{tbl}'::regclass;
\\echo '=== SELECT ==='
EXPLAIN ANALYZE SELECT * FROM {tbl} WHERE user_id = 123;
\\echo '=== RANGE ==='
EXPLAIN ANALYZE SELECT * FROM {tbl} WHERE created_at >= NOW() - INTERVAL '1 day';
\\echo '=== ORDER ==='
EXPLAIN ANALYZE SELECT * FROM {tbl} WHERE user_id = 123 ORDER BY created_at DESC LIMIT 100;
\\echo '=== AGG ==='
EXPLAIN ANALYZE SELECT event_type, COUNT(*) FROM {tbl}
WHERE created_at >= NOW() - INTERVAL '30 days' GROUP BY event_type;
""")
        (RESULTS / f"gap_{label}_withindex.log").write_text(yes, encoding="utf-8")
        print(f"done {label}", flush=True)

    # --- Project entity growth bench ---
    print("project adoptions_growth 100k/1m/5m", flush=True)
    psql("""
DROP TABLE IF EXISTS lab2.adoptions_growth CASCADE;
CREATE TABLE lab2.adoptions_growth (
  id BIGSERIAL PRIMARY KEY,
  animal_id INT NOT NULL,
  user_id INT NOT NULL,
  adoption_date DATE NOT NULL,
  status VARCHAR(50) NOT NULL,
  notes TEXT,
  created_at TIMESTAMP NOT NULL
);
""")
    for n, label in [(100_000, "100k"), (1_000_000, "1m"), (5_000_000, "5m")]:
        print(f"fill growth {label}", flush=True)
        cur_out = psql("SELECT COUNT(*)::text FROM lab2.adoptions_growth;")
        cur = int([l.strip() for l in cur_out.splitlines() if l.strip().isdigit()][0])
        need = n - cur
        if need > 0:
            psql(f"""
INSERT INTO lab2.adoptions_growth (animal_id, user_id, adoption_date, status, notes, created_at)
SELECT ((g*7919)%50000)+1, ((g*9973)%5000)+1, CURRENT_DATE-(g%700),
  (ARRAY['Pending','Approved','Completed','Rejected'])[1+(g%4)], 'b',
  NOW()-((g%700)||' days')::interval
FROM generate_series(1,{need}) g;
ANALYZE lab2.adoptions_growth;
""")
        psql("DROP INDEX IF EXISTS lab2.adoptions_growth_user_id_idx; DROP INDEX IF EXISTS lab2.adoptions_growth_created_at_idx; DROP INDEX IF EXISTS lab2.adoptions_growth_user_id_created_at_idx;")
        # indexes may have auto names — drop by listing
        psql("""
DO $$ DECLARE r record;
BEGIN
  FOR r IN SELECT indexname FROM pg_indexes WHERE schemaname='lab2' AND tablename='adoptions_growth' AND indexname NOT LIKE '%_pkey'
  LOOP EXECUTE 'DROP INDEX IF EXISTS lab2.' || quote_ident(r.indexname); END LOOP;
END $$;
""")
        m1 = psql(f"""
\\echo 'GROWTH {label} NOINDEX'
SELECT COUNT(*) FROM lab2.adoptions_growth;
EXPLAIN ANALYZE SELECT * FROM lab2.adoptions_growth WHERE user_id = 2;
EXPLAIN ANALYZE SELECT * FROM lab2.adoptions_growth WHERE created_at >= NOW()-INTERVAL '30 days' AND created_at < NOW();
EXPLAIN ANALYZE SELECT * FROM lab2.adoptions_growth WHERE user_id = 2 ORDER BY created_at DESC LIMIT 50;
""")
        (RESULTS / f"project_growth_{label}_noindex.log").write_text(m1, encoding="utf-8")
        psql("""
CREATE INDEX ON lab2.adoptions_growth(user_id);
CREATE INDEX ON lab2.adoptions_growth(created_at);
CREATE INDEX ON lab2.adoptions_growth(user_id, created_at DESC);
ANALYZE lab2.adoptions_growth;
""")
        m2 = psql(f"""
\\echo 'GROWTH {label} WITHINDEX'
EXPLAIN ANALYZE SELECT * FROM lab2.adoptions_growth WHERE user_id = 2;
EXPLAIN ANALYZE SELECT * FROM lab2.adoptions_growth WHERE created_at >= NOW()-INTERVAL '30 days' AND created_at < NOW();
EXPLAIN ANALYZE SELECT * FROM lab2.adoptions_growth WHERE user_id = 2 ORDER BY created_at DESC LIMIT 50;
""")
        (RESULTS / f"project_growth_{label}_withindex.log").write_text(m2, encoding="utf-8")
        print(f"growth {label} done", flush=True)

    # Live adoptions measure + grow if needed (fast array insert)
    def live_count():
        out = psql("SELECT COUNT(*)::text FROM adoptions;")
        return int([l.strip() for l in out.splitlines() if l.strip().isdigit()][0])

    def grow(target):
        cur = live_count()
        need = target - cur
        if need <= 0:
            print(f"live already {cur}", flush=True)
            return
        print(f"grow live {cur}->{target}", flush=True)
        psql(f"""
WITH a AS (SELECT array_agg(id) ids FROM animals),
     u AS (SELECT array_agg(id) ids FROM users)
INSERT INTO adoptions (animal_id, user_id, adoption_date, status, notes, created_at)
SELECT a.ids[1+(g%array_length(a.ids,1))], u.ids[1+(g%array_length(u.ids,1))],
  CURRENT_DATE-(g%700), (ARRAY['Pending','Approved','Completed','Rejected'])[1+(g%4)], 'scale',
  TIMESTAMP '2024-09-15' + ((g%700)||' days')::interval
FROM generate_series(1,{need}) g, a, u;
ANALYZE adoptions;
""")

    for target, lab in [(1_000_000, "1m"), (5_000_000, "5m")]:
        grow(target)
        live = psql(f"""
\\echo 'LIVE {lab}'
SELECT COUNT(*) FROM adoptions;
EXPLAIN ANALYZE SELECT * FROM adoptions WHERE user_id = 2;
EXPLAIN ANALYZE SELECT * FROM adoptions WHERE created_at >= NOW()-INTERVAL '30 days' AND created_at < NOW();
EXPLAIN ANALYZE SELECT * FROM adoptions WHERE user_id = 2 ORDER BY created_at DESC LIMIT 50;
""")
        (RESULTS / f"project_live_{lab}.log").write_text(live, encoding="utf-8")

    print("ALL DONE", flush=True)


if __name__ == "__main__":
    main()
