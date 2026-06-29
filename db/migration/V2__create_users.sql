-- V2: users 表（無 FK 依賴）
-- 自訂 users 表 + PasswordHasher/BCrypt（schema 擁有權歸 Flyway，禁用 EF Core Migrations）。
-- email 允許為 NULL（支援 Line OAuth 無 email），唯一性以「部分唯一索引」僅約束非 NULL 值。

CREATE TABLE users (
    id            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    email         VARCHAR(255),
    password_hash VARCHAR(255),
    display_name  VARCHAR(100) NOT NULL,
    avatar_url    TEXT,
    role          VARCHAR(20)  NOT NULL DEFAULT 'user',
    provider      VARCHAR(20)  NOT NULL DEFAULT 'local',
    provider_id   VARCHAR(255),
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT now(),
    updated_at    TIMESTAMPTZ  NOT NULL DEFAULT now(),

    CONSTRAINT chk_users_role     CHECK (role     IN ('user', 'admin')),
    CONSTRAINT chk_users_provider CHECK (provider IN ('local', 'google', 'line'))
);

-- 非 NULL email 唯一；允許多筆 email IS NULL（Line 無 email）
CREATE UNIQUE INDEX uq_users_email
    ON users (email)
    WHERE email IS NOT NULL;

-- OAuth 以 (provider, provider_id) 唯一識別
CREATE UNIQUE INDEX uq_users_provider_identity
    ON users (provider, provider_id)
    WHERE provider_id IS NOT NULL;
