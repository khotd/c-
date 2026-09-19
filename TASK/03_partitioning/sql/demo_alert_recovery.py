from datetime import datetime, timezone
from pathlib import Path
import json
import subprocess
import sys

sys.stdout.reconfigure(encoding="utf-8")


def psql(sql: str) -> str:
    p = subprocess.run(
        [
            "docker", "exec", "-i", "animal-shelter-db",
            "psql", "-U", "project", "-d", "projectDB", "-t", "-A",
        ],
        input=sql,
        text=True,
        capture_output=True,
    )
    return p.stdout.strip()


def main() -> None:
    subprocess.run(
        [
            "docker", "exec", "animal-shelter-db",
            "psql", "-U", "project", "-d", "projectDB", "-c",
            "CREATE TABLE IF NOT EXISTS adoptions_2026_12 PARTITION OF adoptions "
            "FOR VALUES FROM ('2026-12-01') TO ('2027-01-01');",
        ],
        check=True,
    )

    alerts = Path(r"c:\Users\User\Desktop\project\c-\project\alerts")
    alerts.mkdir(exist_ok=True)
    state_file = Path(r"c:\Users\User\Desktop\project\c-\project\partition-alert-state.json")

    existing = {
        x.strip()
        for x in psql(
            "SELECT c.relname FROM pg_inherits i "
            "JOIN pg_class c ON c.oid=i.inhrelid "
            "JOIN pg_class p ON p.oid=i.inhparent "
            "JOIN pg_namespace n ON n.oid=p.relnamespace "
            "WHERE p.relname='adoptions' AND n.nspname='public';"
        ).splitlines()
        if x.strip()
    }

    now = datetime.now(timezone.utc)
    start = datetime(now.year, now.month, 1, tzinfo=timezone.utc)
    required = []
    for i in range(4):
        m = start.month - 1 + i
        y = start.year + m // 12
        mo = m % 12 + 1
        required.append(f"adoptions_{y:04d}_{mo:02d}")

    miss = [r for r in required if r not in existing]
    status = "Critical" if miss else "Ok"
    prev = None
    if state_file.exists():
        prev = json.loads(state_file.read_text(encoding="utf-8")).get("Status")

    print("prev", prev, "now", status, "missing", miss)
    if status != prev:
        checked = now.strftime("%Y-%m-%d %H:%M:%S")
        if status == "Critical":
            body = (
                "Partition alert\n"
                f"Table: adoptions\nMissing partitions: {', '.join(miss)}\n"
                f"Expected horizon: 3 months\nChecked at: {checked}"
            )
        else:
            body = (
                "Partition check OK\nTable: adoptions\n"
                f"All required partitions exist.\nChecked at: {checked}"
            )
        path = alerts / f"partition-alert-{now.strftime('%Y%m%d-%H%M%S')}.txt"
        path.write_text(body, encoding="utf-8")
        state_file.write_text(json.dumps({"Status": status}), encoding="utf-8")
        print("wrote", path)
        print(body)
    else:
        print("no duplicate alert")

    print("alert files:", sorted(p.name for p in alerts.glob("*.txt")))


if __name__ == "__main__":
    main()
