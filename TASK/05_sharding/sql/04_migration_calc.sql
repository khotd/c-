-- Lab 5, Р·Р°РґР°РЅРёСЏ 5 Рё 7. Р Р°СЃС‡С‘С‚ РїРµСЂРµРјРµС‰РµРЅРёСЏ РґР°РЅРЅС‹С… 3 -> 4 С€Р°СЂРґР°.
-- РќРёС‡РµРіРѕ РЅРµ РїРµСЂРµРјРµС‰Р°РµС‚ вЂ” С‚РѕР»СЊРєРѕ СЃС‡РёС‚Р°РµС‚, РєРѕРјСѓ РїСЂРёРґС‘С‚СЃСЏ РїРµСЂРµРµС…Р°С‚СЊ.
--
-- modulo: РІС‹РїРѕР»РЅРёС‚СЊ РЅР° РљРђР–Р”РћРњ С€Р°СЂРґРµ РІ Р‘Р” shelter_shard Рё РїСЂРѕСЃСѓРјРјРёСЂРѕРІР°С‚СЊ moved
--         РїРѕ С‚СЂС‘Рј С€Р°СЂРґР°Рј (РґРѕР»Р¶РЅРѕ РїРѕР»СѓС‡РёС‚СЊСЃСЏ ~75% РѕС‚ 100000).
-- ring:   РІС‹РїРѕР»РЅРёС‚СЊ РЅР° Р›Р®Р‘РћРњ С€Р°СЂРґРµ (РєРѕР»СЊС†Рѕ СЃС‚СЂРѕРёС‚СЃСЏ РїРѕ user_id, РґР°РЅРЅС‹Рµ
--         С„РёР·РёС‡РµСЃРєРё РЅРµ РЅСѓР¶РЅС‹ вЂ” СЃС‡РёС‚Р°РµС‚СЃСЏ РїРѕ РіРµРЅРµСЂРёСЂСѓРµРјС‹Рј vnode-С‚РѕС‡РєР°Рј).

BEGIN;
CREATE OR REPLACE FUNCTION pg_temp.hash32(key bigint)
RETURNS bigint LANGUAGE sql IMMUTABLE AS $$
    SELECT ('x' || substr(md5(key::text), 1, 8))::bit(32)::bigint;
$$;

CREATE OR REPLACE FUNCTION pg_temp.hash64(key text)
RETURNS bigint LANGUAGE sql IMMUTABLE AS $$
    SELECT ('x' || substr(md5(key), 1, 16))::bit(64)::bigint;
$$;

-- --- РЎС‚СЂР°С‚РµРіРёСЏ A: hash(user_id) % N. Р’С‹РїРѕР»РЅРёС‚СЊ РЅР° РєР°Р¶РґРѕРј С€Р°СЂРґРµ -------------
SELECT
    COUNT(*) FILTER (WHERE pg_temp.hash32(user_id) % 3 <> pg_temp.hash32(user_id) % 4) AS moved_3_to_4,
    COUNT(*) FILTER (WHERE pg_temp.hash32(user_id) % 3 =  pg_temp.hash32(user_id) % 4) AS stay,
    COUNT(*) AS total_on_this_shard
FROM adoptions;

-- --- РЎС‚СЂР°С‚РµРіРёСЏ B: Consistent Hash Ring (100 vnodes), РїРѕ РІСЃРµРј РєР»СЋС‡Р°Рј --------
-- dist3/dist4: С€Р°СЂРґ РєР»СЋС‡Р° РїСЂРё 3 Рё РїСЂРё 4 С€Р°СЂРґР°С…; moved = true -> Р·Р°РїРёСЃСЊ РїРµСЂРµРµРґРµС‚.
WITH RECURSIVE vnode(s, i, h) AS (
    SELECT g.s, k.i, pg_temp.hash64('shard-' || g.s || '-vnode-' || k.i)
    FROM generate_series(0, 3) AS g(s),
         LATERAL (SELECT generate_series(1, 100) AS i) k
),
ring AS (SELECT s, h FROM vnode),
keys AS (SELECT DISTINCT user_id AS k, pg_temp.hash64(user_id::text) AS kh FROM adoptions),
route3 AS (
    SELECT keys.k, keys.kh,
           (SELECT ring.s FROM ring WHERE ring.s <= 2 AND ring.h >= keys.kh
            ORDER BY ring.h LIMIT 1) AS s3
    FROM keys
),
route3_wrapped AS (
    SELECT k, kh, COALESCE(s3,
        (SELECT ring.s FROM ring WHERE ring.s <= 2 ORDER BY ring.h LIMIT 1)) AS s3
    FROM route3
),
route4 AS (
    SELECT r3.k, r3.s3,
           COALESCE((SELECT ring.s FROM ring WHERE ring.h >= r3.kh ORDER BY ring.h LIMIT 1),
                    (SELECT ring.s FROM ring ORDER BY ring.h LIMIT 1)) AS s4
    FROM route3_wrapped r3
)
SELECT COUNT(*) FILTER (WHERE s3 <> s4) AS moved_3_to_4,
       COUNT(*) FILTER (WHERE s3 = s4)  AS stay,
       COUNT(*) AS distinct_users
FROM route4;

ROLLBACK;

-- РћР¶РёРґР°РµРјС‹Р№ РёС‚РѕРі (СЃРј. С„Р°РєС‚РёС‡РµСЃРєРёР№ РїСЂРѕРіРѕРЅ results/lab5_run.md):
--   hash(key) % N:        ~75%   Р·Р°РїРёСЃРµР№ РјРµРЅСЏСЋС‚ С€Р°СЂРґ
--   Consistent Hashing:   ~25%   Р·Р°РїРёСЃРµР№ РјРµРЅСЏСЋС‚ С€Р°СЂРґ
