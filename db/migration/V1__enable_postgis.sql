-- V1: 啟用必要 extension
-- PostGIS 提供地理空間型別與 GIST 索引；pgcrypto 提供 gen_random_uuid()
-- （PostgreSQL 16 核心已內建 gen_random_uuid，pgcrypto 為相容保險）。

CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS pgcrypto;
