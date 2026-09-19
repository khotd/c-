# -*- coding: utf-8 -*-
"""
Lab 6 — что происходит с backend-сервисом после шардирования (Animal Shelter).

Живые проверки на шардах лабы 5 (shelter_shard, по 100 000 записей adoptions):
  2. Single-shard query      — запрос с shard key идёт ровно на один шард
  3. Distributed aggregation — COUNT/GROUP BY на каждом шарде + merge в приложении
  4. Distributed JOIN        — заявки на шарде, справочник животных «на Primary»:
                               два запроса + merge в приложении
  5. ORDER BY + LIMIT        — top-N с каждого шарда + merge (и почему LIMIT
                               меньше N даёт неверный ответ)
  6. Отказ одного шарда      — docker stop shelter_shard2: что доступно, что нет
  7. Hot shard               — строки равны, нагрузка нет (модель «китов»)

Запуск:
  python TASK/06_post_sharding/scripts/lab6_checks.py [--skip-failure]
Отчёт: TASK/06_post_sharding/results/lab6_run.md
"""

import argparse
import datetime as dt
import os
import subprocess
import sys
import time
from collections import Counter

import psycopg2

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)),
                                "..", "..", "05_sharding", "scripts"))
from shard_router import ShardRouterMod  # noqa: E402

SHARDS = [
    {"name": "shard0", "host": "localhost", "port": 5445, "container": "shelter_shard0"},
    {"name": "shard1", "host": "localhost", "port": 5446, "container": "shelter_shard1"},
    {"name": "shard2", "host": "localhost", "port": 5447, "container": "shelter_shard2"},
]
DB = "shelter_shard"
DB_USER, DB_PASS = "shelter", "shelter"
ROUTER = ShardRouterMod([0, 1, 2])


def connect(shard):
    return psycopg2.connect(host=shard["host"], port=shard["port"], dbname=DB,
                            user=DB_USER, password=DB_PASS, connect_timeout=3)


def fmt(n):
    return format(n, ",").replace(",", " ")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--skip-failure", action="store_true",
                    help="не останавливать контейнер shard2")
    args = ap.parse_args()
    out = []

    def emit(text=""):
        print(text)
        out.append(text)

    emit("# Lab 6 — фактические проверки после шардирования `adoptions`")
    emit()
    emit("Дата: %s. Данные: 100 000 записей adoptions в БД shelter_shard на трёх шардах."
         % dt.datetime.now().strftime("%Y-%m-%d %H:%M"))
    emit()

    # ---- 2. Single-shard query ----------------------------------------------
    emit("## 2. Single-shard query: GET /api/users/{id}/adoptions")
    emit()
    user_id = 42
    shard_idx = ROUTER.route(user_id)
    shard = SHARDS[shard_idx]
    emit("Router: hash32(%d) %% 3 = %d -> запрос идёт только на %s."
         % (user_id, shard_idx, shard["name"]))
    with connect(shard) as conn, conn.cursor() as cur:
        cur.execute("SELECT COUNT(*), MIN(id), MAX(id) FROM adoptions WHERE user_id = %s",
                    (user_id,))
        cnt, mn, mx = cur.fetchone()
    emit("SQL на %s: `SELECT ... FROM adoptions WHERE user_id = %d` -> %s строк (id %s..%s)."
         % (shard["name"], user_id, fmt(cnt), mn, mx))
    emit("Другие шарды НЕ опрашиваются: запись не может находиться где-то ещё —")
    emit("роутер детерминирован. Это самый дешёвый класс запросов после шардирования.")
    emit()

    # ---- 3. Distributed aggregation -----------------------------------------
    emit("## 3. Агрегация: GET /api/adoptions/stats/by-status")
    emit()
    per_shard = {}
    for s in SHARDS:
        with connect(s) as conn, conn.cursor() as cur:
            cur.execute("SELECT status, COUNT(*) FROM adoptions GROUP BY status ORDER BY 2 DESC")
            per_shard[s["name"]] = cur.fetchall()
    emit("| Шард | Pending | Approved | Completed | Rejected | Всего |")
    emit("|------|--------:|---------:|----------:|---------:|------:|")
    merged = Counter()
    for s in SHARDS:
        row = {st: c for st, c in per_shard[s["name"]]}
        for st, c in row.items():
            merged[st] += c
        total = sum(row.values())
        emit("| %s | %s | %s | %s | %s | %s |" % (
            s["name"], fmt(row.get("Pending", 0)), fmt(row.get("Approved", 0)),
            fmt(row.get("Completed", 0)), fmt(row.get("Rejected", 0)), fmt(total)))
    emit("| **merge (sum)** | **%s** | **%s** | **%s** | **%s** | **%s** |" % (
        fmt(merged["Pending"]), fmt(merged["Approved"]), fmt(merged["Completed"]),
        fmt(merged["Rejected"]), fmt(sum(merged.values()))))
    emit()
    emit("На одном шарде запрос выполнить нельзя: каждый шард видит только свою часть.")
    emit("Результаты объединяются в backend (merge): COUNT/SUM складываются, AVG — нет")
    emit("(нужны sum и count с каждого шарда, среднее — после merge). Время = max(шард)")
    emit("+ merge; запросы к шардам идут параллельно, но с ростом N растёт merge-работа.")
    emit()

    # ---- 4. Distributed JOIN --------------------------------------------------
    emit("## 4. JOIN между шардами: adoptions (шард) + animals (Primary)")
    emit()
    emit("GetDetailedByUserIdAsync (JOIN adoptions+animals+users+shelters) обычным SQL")
    emit("выполнить нельзя: заявки лежат на шарде, справочники — на Primary.")
    emit("Практический паттерн — два запроса и merge в приложении:")
    with connect(shard) as conn, conn.cursor() as cur:
        cur.execute("SELECT id, animal_id, status, created_at FROM adoptions "
                    "WHERE user_id = %s ORDER BY created_at DESC LIMIT 5", (user_id,))
        adoptions = cur.fetchall()
    animal_ids = [a[1] for a in adoptions]
    emit("1) шарде %s: заявки пользователя -> %d строк, animal_id = %s"
         % (shard["name"], len(adoptions), animal_ids))
    animal_names = ["Animal #%d" % aid for aid in animal_ids]  # имитация справочника Primary
    emit("2) Primary: SELECT id, name FROM animals WHERE id IN (%s)"
         % ", ".join(map(str, sorted(set(animal_ids)))))
    emit("3) merge в приложении: собираем DTO на стороне backend, например:")
    for aid, name in zip(animal_ids, animal_names):
        emit("   - %s (animal_id=%d): заявка на %s, справочник на Primary" % (name, aid, shard["name"]))
    emit()
    emit("Между шардами пришлось бы передавать либо все заявки пользователя, либо")
    emit("все животные — таблицы не могут встретиться в одном SQL-движке.")
    emit("Co-location: шардируем animals по тому же ключу — тогда JOIN заявки+животное")
    emit("чаще выполняется внутри одного шарда.")
    emit()


    # ---- 5. ORDER BY + LIMIT ---------------------------------------------------
    emit("## 5. ORDER BY + LIMIT: топ заявок (аналог GET /api/animals с сортировкой)")
    emit()
    limit = 10
    tops = {}
    for s in SHARDS:
        with connect(s) as conn, conn.cursor() as cur:
            cur.execute("SELECT id, user_id, created_at FROM adoptions "
                        "ORDER BY created_at DESC LIMIT %s", (limit,))
            tops[s["name"]] = cur.fetchall()
        emit("%s: top %d по created_at DESC, самая старая в выборке = %s"
             % (s["name"], limit, tops[s["name"]][-1][2].strftime("%Y-%m-%d %H:%M:%S")))
    merged_top = sorted((row for rows in tops.values() for row in rows),
                        key=lambda r: r[2], reverse=True)[:limit]
    emit()
    emit("Merge -> глобальный top %d (первые 3):" % limit)
    for r in merged_top[:3]:
        emit("   id=%s user_id=%s created_at=%s" % (fmt(r[0]), r[1], r[2]))
    emit()
    emit("Брать LIMIT %d только с одного шарда нельзя: его самые свежие записи могут" % limit)
    emit("оказаться старше свежих записей других шардов. Merge требует top-%d с КАЖДОГО" % limit)
    emit("шарда; при LIMIT < %d на шарде глобальный top может оказаться неполным." % limit)
    emit("Чем глубже пагинация, тем дороже merge (OFFSET в распределённом виде дорог).")
    emit()

    # ---- 6. Отказ одного шарда ---------------------------------------------------
    if args.skip_failure:
        emit("## 6. Отказ одного шарда — ПРОПУЩЕНО (--skip-failure)")
        emit()
    else:
        emit("## 6. Отказ одного шарда (docker stop shelter_shard2)")
        emit()
        victim = SHARDS[2]
        subprocess.run(["docker", "stop", victim["container"]],
                       capture_output=True, text=True)
        try:
            with connect(victim) as conn, conn.cursor() as cur:
                cur.execute("SELECT COUNT(*) FROM adoptions")
                cur.fetchone()
            emit("- %s: неожиданно доступен (проверить контейнер!)" % victim["name"])
        except Exception as e:
            emit("- %s: недоступен — %s" % (victim["name"], str(e).strip().splitlines()[0]))
        healthy = SHARDS[0]
        with connect(healthy) as conn, conn.cursor() as cur:
            cur.execute("SELECT COUNT(*) FROM adoptions")
            cnt0 = cur.fetchone()[0]
        emit("- %s: запросы работают (%s строк)." % (healthy["name"], fmt(cnt0)))
        emit()
        emit("Недоступны: все adoptions пользователей, маршрутизируемых на shard 2")
        emit("(~1/3 данных). Перестают работать их endpoint'ы (GET/POST/PUT заявок)")
        emit("и глобальные агрегаты (COUNT неполон). Остальное (шард 0, 1, справочники")
        emit("Primary, чтение животных) продолжает работать. Backend обязан: таймауты,")
        emit("понятная ошибка 503, частичная деградация, ретраи только чтений, healthcheck.")
        emit("Отказоустойчивость: реплика каждого шарда + failover, бэкапы per-shard,")
        emit("буферизация записей (outbox) на время недоступности.")
        subprocess.run(["docker", "start", victim["container"]],
                       capture_output=True, text=True)
        for _ in range(30):
            try:
                with connect(victim) as conn, conn.cursor() as cur:
                    cur.execute("SELECT 1")
                    cur.fetchone()
                emit("- %s запущен обратно и отвечает." % victim["container"])
                break
            except Exception:
                time.sleep(1)
        emit()

    # ---- 7. Hot shard -------------------------------------------------------------
    emit("## 7. Hot shard: строки равны, нагрузка — нет")
    emit()
    rows_per_shard = {}
    for s in SHARDS:
        with connect(s) as conn, conn.cursor() as cur:
            cur.execute("SELECT COUNT(*) FROM adoptions")
            rows_per_shard[s["name"]] = cur.fetchone()[0]
    emit("| Шард | Строк | Доля строк | Запросов (модель) | Доля запросов |")
    emit("|------|------:|-----------:|------------------:|--------------:|")
    whales = {7: 300_000, 42: 300_000, 1337: 300_000}
    load = {s["name"]: rows_per_shard[s["name"]] for s in SHARDS}
    for uid, extra in whales.items():
        load[SHARDS[ROUTER.route(uid)]["name"]] += extra
    total_rows = sum(rows_per_shard.values())
    total_load = sum(load.values())
    for s in SHARDS:
        emit("| %s | %s | %.2f%% | %s | %.2f%% |" % (
            s["name"], fmt(rows_per_shard[s["name"]]),
            100.0 * rows_per_shard[s["name"]] / total_rows,
            fmt(load[s["name"]]), 100.0 * load[s["name"]] / total_load))
    emit()
    emit("Модель: по одному чтению ленты на запись + «киты» user_id 7/42/1337 (по 300k")
    emit("опросов своего профиля). Причины перекоса: популярные ключи, тип запросов,")
    emit("неудачный ключ. Равные строки НЕ означают равную нагрузку. Лечение: кэш")
    emit("горячих ключей (Redis уже в стеке), ребалансировка дуг, реплика горячего")
    emit("шарда, составной shard key.")
    emit()

    out_path = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                            "..", "results", "lab6_run.md")
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    with open(out_path, "w", encoding="utf-8") as f:
        f.write("\n".join(out) + "\n")
    print()
    print("Отчёт сохранён: %s" % os.path.normpath(out_path))


if __name__ == "__main__":
    main()

