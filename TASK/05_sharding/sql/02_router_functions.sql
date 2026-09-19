-- Lab 5, С€Р°Рі 2. Router РЅР° С‡РёСЃС‚РѕРј SQL: С‚Рµ Р¶Рµ С…РµС€Рё, С‡С‚Рѕ РІ scripts/shard_router.py.
-- Р¤СѓРЅРєС†РёРё session-local (pg_temp) вЂ” РЅРёС‡РµРіРѕ РЅРµ РѕСЃС‚Р°С‘С‚СЃСЏ РІ СЃС…РµРјРµ.
-- Р’С‹РїРѕР»РЅРёС‚СЊ РЅР° Р»СЋР±РѕРј С€Р°СЂРґРµ РІ Р‘Р” shelter_shard.

BEGIN;
-- 32-Р±РёС‚РЅС‹Р№ С…РµС€: 0 .. 2^32-1  (СЃРѕРІРїР°РґР°РµС‚ СЃ hash32 РІ Python/C#-СЂРѕСѓС‚РµСЂРµ)
CREATE OR REPLACE FUNCTION pg_temp.hash32(key bigint)
RETURNS bigint LANGUAGE sql IMMUTABLE AS $$
    SELECT ('x' || substr(md5(key::text), 1, 8))::bit(32)::bigint;
$$;

-- 64-Р±РёС‚РЅС‹Р№ С…РµС€ РґР»СЏ hash ring (СЃРѕ Р·РЅР°РєРѕРј, РєР°Рє РІ Python)
CREATE OR REPLACE FUNCTION pg_temp.hash64(key text)
RETURNS bigint LANGUAGE sql IMMUTABLE AS $$
    SELECT ('x' || substr(md5(key), 1, 16))::bit(64)::bigint;
$$;

-- РЎС‚СЂР°С‚РµРіРёСЏ A: shard = hash(shard_key) % N
CREATE OR REPLACE FUNCTION pg_temp.shard_mod(key bigint, n int)
RETURNS int LANGUAGE sql IMMUTABLE AS $$
    SELECT (pg_temp.hash32(key) % n)::int;
$$;

-- РџСЂРёРјРµСЂС‹ РјР°СЂС€СЂСѓС‚РёР·Р°С†РёРё (Р·Р°РґР°РЅРёРµ 3):
SELECT user_id AS key, pg_temp.hash32(user_id) AS hash32,
       pg_temp.shard_mod(user_id, 3) AS shard_mod_3
FROM (VALUES (101), (102), (103), (1), (2), (3)) AS v(user_id);

-- РљСѓРґР° РїРѕРїР°РґС‘С‚ РєРѕРЅРєСЂРµС‚РЅР°СЏ Р·Р°РїРёСЃСЊ РїСЂРё N=3:
--   SELECT pg_temp.shard_mod(12345, 3);   -- 2  -> РѕР±СЂР°С‰Р°РµРјСЃСЏ Рє shelter_shard2

ROLLBACK;
