# -*- coding: utf-8 -*-
"""
Lab 5 — эксперимент по шардированию сущности `adoptions` (Animal Shelter).

Шард key = user_id. Сравниваются две стратегии:
  A) shard = hash(shard_key) % N          (прямой modulo)
  B) Consistent Hashing — hash ring с virtual nodes

Скрипт:
  1. Создаёт на каждом из 3 экземпляров PostgreSQL БД shelter_shard (modulo) и
     shelter_ring (consistent hashing).
  2. Генерирует 100 000 записей adoptions той же структуры, что в сервисе
     (см. project/liquibase/changelogs/005-add-adoptions.sql), БЕЗ межсерверных
     FK — экземпляры независимы, ссылки на animals/users остаются на Primary.
  3. Физически распределяет данные по шардам обеими стратегиями.
  4. Показывает распределение записей по шардам.
  5. Считает, сколько записей переедут при 3 -> 4 шарда для обеих стратегий
     (и 4 -> 3 при удалении шарда).

Хеш-функции совпадают с SQL: TASK/05_sharding/sql/02_router_functions.sql.
Запуск (шарды должны быть запущены: docker compose -f project/docker-compose.shards.yml up -d):

  python TASK/05_sharding/scripts/lab5_sharding.py [--total 100000] [--users 5000] [--vnodes 100]

Отчёт в Markdown пишется в TASK/05_sharding/results/lab5_run.md и в stdout.
"""

import argparse
import datetime as dt
import os
import random
import sys

import psycopg2
from psycopg2.extras import execute_values

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from shard_router import ShardRouterMod, ConsistentHashRing, hash32  # noqa: E402

# Хост-порты шардов из project/docker-compose.shards.yml
SHARDS = [
    {"name": "shard0", "host": "localhost", "port": 5445},
    {"name": "shard1", "host": "localhost", "port": 5446},
    {"name": "shard2", "host": "localhost", "port": 5447},
]
DB_USER = "shelter"
DB_PASS = "shelter"
DB_MOD = "shelter_shard"   # размещение hash(key) % N
DB_RING = "shelter_ring"   # размещение consistent hashing

STATUSES = ["Pending", "Approved", "Completed", "Rejected"]

SCHEMA_SQL = """
CREATE TABLE IF NOT EXISTS adoptions (
    id            INTEGER PRIMARY KEY,
    animal_id     INTEGER NOT NULL,
    user_id       INTEGER NOT NULL,
    adoption_date DATE NOT NULL,
    status        VARCHAR(50) NOT NULL,
    notes         TEXT,
    created_at    TIMESTAMP NOT NULL,
    updated_at    TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_adoptions_user_id   ON adoptions (user_id);
CREATE INDEX IF NOT EXISTS idx_adoptions_status    ON adoptions (status);
CREATE INDEX IF NOT EXISTS idx_adoptions_created_at ON adoptions (created_at);
"""


def connect(shard, dbname, autocommit=False):
    conn = psycopg2.connect(
        host=shard["host"], port=shard["port"], dbname=dbname,
        user=DB_USER, password=DB_PASS, connect_timeout=5,
    )
    conn.autocommit = autocommit
    return conn


def ensure_ring_db(shard):
    """Создаёт БД shelter_ring на экземпляре, если её ещё нет.

    CREATE DATABASE нельзя выполнять внутри транзакции, поэтому соединение
    используется в режиме autocommit и без контекст-менеджера транзакции.
    """
    conn = connect(shard, "postgres", autocommit=True)
    try:
        with conn.cursor() as cur:
            cur.execute(
                "SELECT 1 FROM pg_database WHERE datname = %s", (DB_RING,))
            if cur.fetchone() is None:
                cur.execute('CREATE DATABASE "%s"' % DB_RING)
    finally:
        conn.close()


def create_schema(shard, dbname):
    with connect(shard, dbname) as conn:
        with conn.cursor() as cur:
            cur.execute(SCHEMA_SQL)
        conn.commit()


def insert_rows(shard, dbname, rows, label):
    """Физическая загрузка строк в шард (батчами по 1000)."""
    sql = ("INSERT INTO adoptions (id, animal_id, user_id, adoption_date, "
           "status, notes, created_at, updated_at) VALUES %s")
    with connect(shard, dbname) as conn:
        with conn.cursor() as cur:
            cur.execute("TRUNCATE TABLE adoptions")
            for i in range(0, len(rows), 1000):
                execute_values(cur, sql, rows[i:i + 1000], page_size=1000)
        conn.commit()
    print("  %s: %s строк загружено" % (label, format(len(rows), ",")))


def count_per_shard(shards, dbname):
    counts = {}
    for s in shards:
        with connect(s, dbname) as conn:
            with conn.cursor() as cur:
                cur.execute("SELECT COUNT(*), COUNT(DISTINCT user_id) FROM adoptions")
                counts[s["name"]] = cur.fetchone()
    return counts


def fmt(n):
    return format(n, ",").replace(",", " ")


def stddev(vals):
    if not vals:
        return 0.0
    mean = sum(vals) / len(vals)
    return (sum((v - mean) ** 2 for v in vals) / len(vals)) ** 0.5


def dist_table(title, counter, shard_names):
    total = sum(counter.values()) or 1
    lines = ["", "### %s" % title, "",
             "| Шард | Записей | Доля |", "|------|--------:|-----:|"]
    vals = []
    for name in shard_names:
        c = counter.get(name, 0)
        vals.append(c)
        lines.append("| %s | %s | %.2f%% |" % (name, fmt(c), 100.0 * c / total))
    lines.append("| **Итого** | **%s** | **100.00%%** |" % fmt(total))
    lines.append("")
    lines.append("max/min = %.3f (идеал 1.0), std-отклонение = %.1f записей"
                 % (max(vals) / min(vals), stddev(vals)))
    return "\n".join(lines)


def generate_rows(total, users, seed=42):
    """Записи adoptions с той же структурой, что у сервиса.

    user_id равномерно по `users` пользователям (shard key),
    animal_id по 1000 животным, даты за последние ~700 дней.
    """
    rng = random.Random(seed)
    now = dt.datetime(2026, 9, 17, 12, 0, 0)
    rows = []
    for i in range(1, total + 1):
        created = now - dt.timedelta(minutes=rng.randint(0, 700 * 24 * 60))
        rows.append((
            i,
            rng.randint(1, 1000),                     # animal_id
            rng.randint(1, users),                    # user_id  <- shard key
            created.date(),                           # adoption_date
            STATUSES[rng.randrange(len(STATUSES))],   # status
            "lab5 seed",                              # notes
            created,                                  # created_at
            None,                                     # updated_at
        ))
    return rows


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--total", type=int, default=100_000)
    ap.add_argument("--users", type=int, default=5_000)
    ap.add_argument("--vnodes", type=int, default=100)
    args = ap.parse_args()

    out_lines = []

    def emit(text=""):
        print(text)
        out_lines.append(text)

    shard_names = [s["name"] for s in SHARDS]

    emit("# Lab 5 — фактический прогон шардирования `adoptions`")
    emit()
    emit("- Дата: %s" % dt.datetime.now().strftime("%Y-%m-%d %H:%M"))
    emit("- Записей: %s, различных user_id: %s, vnodes: %d"
         % (fmt(args.total), fmt(args.users), args.vnodes))
    emit("- Шарды: " + ", ".join(
        "%s (localhost:%d)" % (s["name"], s["port"]) for s in SHARDS))
    emit()

    # ---- 0. Подготовка БД и схемы -------------------------------------------
    emit("## Подготовка")
    for s in SHARDS:
        ensure_ring_db(s)
        create_schema(s, DB_MOD)
        create_schema(s, DB_RING)
    emit("Схема adoptions создана в БД %s и %s на всех экземплярах." % (DB_MOD, DB_RING))
    emit()

    rows = generate_rows(args.total, args.users)
    keys = [r[2] for r in rows]  # user_id каждой записи

    # ---- 1. Стратегия A: hash(user_id) % N ----------------------------------
    emit("## Стратегия A: shard = hash(user_id) % N")
    mod3 = ShardRouterMod([0, 1, 2])
    placement_mod = [mod3.route(k) for k in keys]

    emit("Примеры маршрутизации:")
    emit()
    emit("| user_id | hash32(user_id) | mod 3 |")
    emit("|--------:|----------------:|------:|")
    for uid in (101, 102, 103, 1, 2, 3):
        emit("| %d | %d | Shard %d |" % (uid, hash32(uid), mod3.route(uid)))
    emit()

    for s in SHARDS:
        sub = [rows[i] for i, sh in enumerate(placement_mod) if sh == int(s["name"][-1])]
        insert_rows(s, DB_MOD, sub, s["name"] + " (" + DB_MOD + ")")

    counts_mod = count_per_shard(SHARDS, DB_MOD)
    emit(dist_table("Распределение записей по шардам (modulo)",
                    {k: v[0] for k, v in counts_mod.items()}, shard_names))
    emit()

    # ---- 2. Что будет при 3 -> 4 для modulo ---------------------------------
    mod4 = ShardRouterMod([0, 1, 2, 3])
    moved = sum(1 for k in keys if mod3.route(k) != mod4.route(k))
    emit("## Задание 5: добавление 4-го шарда (modulo)")
    emit()
    emit("| Метрика | Значение |")
    emit("|---------|---------:|")
    emit("| Всего записей | %s |" % fmt(args.total))
    emit("| Изменили шард (mod 3 -> mod 4) | %s |" % fmt(moved))
    emit("| Остались на прежнем шарде | %s |" % fmt(args.total - moved))
    emit("| **Процент перемещаемых** | **%.2f%%** |" % (100.0 * moved / args.total))
    emit()
    emit("Modulo меняет делитель 3 -> 4, поэтому почти для всех ключей меняется")
    emit("остаток: теория предсказывает ~75% перемещений, что и наблюдается.")
    emit()

    # ---- 3. Стратегия B: Consistent Hashing ---------------------------------
    emit("## Стратегия B: Consistent Hash Ring (%d vnodes)" % args.vnodes)
    ring3 = ConsistentHashRing([0, 1, 2], args.vnodes)
    placement_ring = [ring3.route(k) for k in keys]

    for s in SHARDS:
        sub = [rows[i] for i, sh in enumerate(placement_ring) if sh == int(s["name"][-1])]
        insert_rows(s, DB_RING, sub, s["name"] + " (" + DB_RING + ")")

    counts_ring = count_per_shard(SHARDS, DB_RING)
    emit(dist_table("Распределение записей по шардам (consistent)",
                    {k: v[0] for k, v in counts_ring.items()}, shard_names))
    emit("Различных user_id на шардах (modulo): "
         + ", ".join("%s=%s" % (k, fmt(v[1])) for k, v in counts_mod.items()))
    emit()

    # ---- 4. 3 -> 4 для consistent: сравнение --------------------------------
    ring4 = ConsistentHashRing([0, 1, 2, 3], args.vnodes)
    moved_ring = sum(1 for k in keys if ring3.route(k) != ring4.route(k))
    emit("## Задание 7: 3 -> 4 шарда, сравнение стратегий")
    emit()
    emit("| Стратегия | Перемещено | Из %s | Процент |" % fmt(args.total))
    emit("|-----------|-----------:|----------:|--------:|")
    emit("| hash(key) %% N | %s | %s | **%.2f%%** |"
         % (fmt(moved), fmt(args.total), 100.0 * moved / args.total))
    emit("| Consistent Hashing | %s | %s | **%.2f%%** |"
         % (fmt(moved_ring), fmt(args.total), 100.0 * moved_ring / args.total))
    emit()

    # ---- 5. Влияние vnodes + удаление шарда ---------------------------------
    emit("## Дополнительно: влияние vnodes и удаление шарда")
    emit()
    for v in (1, 10, 100):
        r3 = ConsistentHashRing([0, 1, 2], v)
        c = {}
        for k in keys:
            s = r3.route(k)
            c[s] = c.get(s, 0) + 1
        vals = [c.get(i, 0) for i in range(3)]
        emit("- vnodes=%d: распределение %s (max/min = %.3f)"
             % (v, vals, max(vals) / min(vals)))
    emit()

    moved_rm = sum(1 for k in keys if ring4.route(k) != ring3.route(k))
    emit("Удаление шарда 4 -> 3 (данные уходят только с удалённого узла):")
    emit("- modulo: перемещается ~%.2f%% записей" % (100.0 * moved / args.total))
    emit("- consistent: перемещается %.2f%% записей" % (100.0 * moved_rm / args.total))
    emit()
    emit("Проверка SQL: см. sql/03_distribution.sql и sql/04_migration_calc.sql.")

    out_path = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                            "..", "results", "lab5_run.md")
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    with open(out_path, "w", encoding="utf-8") as f:
        f.write("\n".join(out_lines) + "\n")
    print()
    print("Отчёт сохранён: %s" % os.path.normpath(out_path))


if __name__ == "__main__":
    main()

