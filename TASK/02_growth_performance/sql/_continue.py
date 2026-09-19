from pathlib import Path
import subprocess, sys
RESULTS = Path(r"c:\Users\User\Desktop\project\c-\TASK\02_growth_performance\results")

def psql(sql: str) -> str:
    p = subprocess.run(
        ["docker","exec","-i","animal-shelter-db","psql","-U","project","-d","projectDB","-v","ON_ERROR_STOP=1"],
        input=sql, text=True, capture_output=True, encoding="utf-8", errors="replace",
    )
    out = (p.stdout or "") + (p.stderr or "")
    if p.returncode != 0:
        print(out); raise SystemExit(p.returncode)
    return out

def count(table: str) -> int:
    out = psql(f"SELECT COUNT(*)::text FROM {table};")
    return int([l.strip() for l in out.splitlines() if l.strip().isdigit()][0])

def drop_extra_indexes():
    psql("""
DROP INDEX IF EXISTS lab2.adoptions_growth_user_id_idx;
DROP INDEX IF EXISTS lab2.adoptions_growth_created_at_idx;
DROP INDEX IF EXISTS lab2.adoptions_growth_user_id_created_at_idx;
""")

for n, label in [(1_000_000, "1m"), (5_000_000, "5m")]:
    cur = count("lab2.adoptions_growth")
    need = n - cur
    print(f"{label}: have {cur}, need +{need}", flush=True)
    if need > 0:
        psql(f"""
INSERT INTO lab2.adoptions_growth (animal_id, user_id, adoption_date, status, notes, created_at)
SELECT ((g::bigint * 7919) % 50000) + 1,
       ((g::bigint * 9973) % 5000) + 1,
       CURRENT_DATE - ((g % 700)::int),
       (ARRAY['Pending','Approved','Completed','Rejected'])[1 + (g % 4)],
       'b',
       NOW() - ((g % 700) || ' days')::interval
FROM generate_series(1, {need}) g;
ANALYZE lab2.adoptions_growth;
""")
    drop_extra_indexes()
    m1 = psql(f"""
\\echo GROWTH {label} NOINDEX
SELECT COUNT(*) FROM lab2.adoptions_growth;
EXPLAIN ANALYZE SELECT * FROM lab2.adoptions_growth WHERE user_id = 2;
EXPLAIN ANALYZE SELECT * FROM lab2.adoptions_growth WHERE created_at >= NOW() - INTERVAL '30 days' AND created_at < NOW();
EXPLAIN ANALYZE SELECT * FROM lab2.adoptions_growth WHERE user_id = 2 ORDER BY created_at DESC LIMIT 50;
""")
    (RESULTS / f"project_growth_{label}_noindex.log").write_text(m1, encoding="utf-8")
    psql("""
CREATE INDEX IF NOT EXISTS adoptions_growth_user_id_idx ON lab2.adoptions_growth(user_id);
CREATE INDEX IF NOT EXISTS adoptions_growth_created_at_idx ON lab2.adoptions_growth(created_at);
CREATE INDEX IF NOT EXISTS adoptions_growth_user_id_created_at_idx ON lab2.adoptions_growth(user_id, created_at DESC);
ANALYZE lab2.adoptions_growth;
""")
    m2 = psql(f"""
\\echo GROWTH {label} WITHINDEX
EXPLAIN ANALYZE SELECT * FROM lab2.adoptions_growth WHERE user_id = 2;
EXPLAIN ANALYZE SELECT * FROM lab2.adoptions_growth WHERE created_at >= NOW() - INTERVAL '30 days' AND created_at < NOW();
EXPLAIN ANALYZE SELECT * FROM lab2.adoptions_growth WHERE user_id = 2 ORDER BY created_at DESC LIMIT 50;
""")
    (RESULTS / f"project_growth_{label}_withindex.log").write_text(m2, encoding="utf-8")
    print(label, "done", flush=True)

def grow_live(target: int):
    cur = count("adoptions")
    need = target - cur
    if need <= 0:
        print("live already", cur, flush=True)
        return
    print(f"grow live {cur}->{target}", flush=True)
    psql(f"""
WITH a AS (SELECT array_agg(id) ids FROM animals),
     u AS (SELECT array_agg(id) ids FROM users)
INSERT INTO adoptions (animal_id, user_id, adoption_date, status, notes, created_at)
SELECT a.ids[1 + (g % array_length(a.ids, 1))],
       u.ids[1 + (g % array_length(u.ids, 1))],
       CURRENT_DATE - ((g % 700)::int),
       (ARRAY['Pending','Approved','Completed','Rejected'])[1 + (g % 4)],
       'scale',
       TIMESTAMP '2024-09-15' + ((g % 700) || ' days')::interval
FROM generate_series(1, {need}) g, a, u;
ANALYZE adoptions;
""")

for target, lab in [(1_000_000, "1m"), (5_000_000, "5m")]:
    grow_live(target)
    live = psql(f"""
\\echo LIVE {lab}
SELECT COUNT(*) FROM adoptions;
EXPLAIN ANALYZE SELECT * FROM adoptions WHERE user_id = 2;
EXPLAIN ANALYZE SELECT * FROM adoptions WHERE created_at >= NOW() - INTERVAL '30 days' AND created_at < NOW();
EXPLAIN ANALYZE SELECT * FROM adoptions WHERE user_id = 2 ORDER BY created_at DESC LIMIT 50;
""")
    (RESULTS / f"project_live_{lab}.log").write_text(live, encoding="utf-8")

print("ALL DONE", flush=True)
