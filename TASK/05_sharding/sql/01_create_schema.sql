-- Lab 5, С€Р°Рі 1. РЎС…РµРјР° С€Р°СЂРґРёСЂРѕРІР°РЅРЅРѕР№ СЃСѓС‰РЅРѕСЃС‚Рё РЅР° РєР°Р¶РґРѕРј С€Р°СЂРґРµ.
-- Р’С‹РїРѕР»РЅРёС‚СЊ РЅР° РљРђР–Р”РћРњ СЌРєР·РµРјРїР»СЏСЂРµ (shelter_shard0 :5445, shelter_shard1 :5446,
-- shelter_shard2 :5447) РІ Р‘Р” shelter_shard (Рё, РїСЂРё Р¶РµР»Р°РЅРёРё, РІ shelter_ring).
-- user/password: shelter / shelter
--
-- РўР°Р±Р»РёС†Р° РїРѕРІС‚РѕСЂСЏРµС‚ СЃС‚СЂСѓРєС‚СѓСЂСѓ adoptions РёР· СЃРµСЂРІРёСЃР°
-- (project/liquibase/changelogs/005-add-adoptions.sql), РќРћ:
--   * id РЅРµ SERIAL: РіР»РѕР±Р°Р»СЊРЅС‹Рµ id РЅР°Р·РЅР°С‡Р°РµС‚ Primary/РіРµРЅРµСЂР°С‚РѕСЂ, С€Р°СЂРґ РїРѕР»СѓС‡Р°РµС‚ РіРѕС‚РѕРІС‹Рµ;
--   * РЅРµС‚ FK РЅР° animals/users: СЌРєР·РµРјРїР»СЏСЂС‹ РЅРµР·Р°РІРёСЃРёРјС‹, СЃСЃС‹Р»РєРё РѕСЃС‚Р°СЋС‚СЃСЏ РЅР° Primary.

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

CREATE INDEX IF NOT EXISTS idx_adoptions_user_id    ON adoptions (user_id);
CREATE INDEX IF NOT EXISTS idx_adoptions_status     ON adoptions (status);
CREATE INDEX IF NOT EXISTS idx_adoptions_created_at ON adoptions (created_at);
