-- V7: import_history 表（FK → users）
-- 每次批次匯入寫入一筆紀錄（資料集、操作者、total/success/skipped/failed）。

CREATE TABLE import_history (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    dataset     VARCHAR(100) NOT NULL,
    operated_by UUID,
    total       INTEGER      NOT NULL DEFAULT 0,
    success     INTEGER      NOT NULL DEFAULT 0,
    skipped     INTEGER      NOT NULL DEFAULT 0,
    failed      INTEGER      NOT NULL DEFAULT 0,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT now(),

    CONSTRAINT fk_import_history_operated_by
        FOREIGN KEY (operated_by) REFERENCES users (id) ON DELETE SET NULL
);

CREATE INDEX ix_import_history_created_at
    ON import_history (created_at DESC);
