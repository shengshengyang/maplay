-- V5: spot_images 表（FK → spots, users）
-- DB 僅存物件儲存 URL 與 object_key（供刪除），不依賴特定供應商 SDK。

CREATE TABLE spot_images (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    spot_id     UUID        NOT NULL,
    url         TEXT        NOT NULL,
    object_key  TEXT        NOT NULL,
    is_cover    BOOLEAN     NOT NULL DEFAULT false,
    uploaded_by UUID,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_spot_images_spot
        FOREIGN KEY (spot_id)     REFERENCES spots (id) ON DELETE CASCADE,
    CONSTRAINT fk_spot_images_uploaded_by
        FOREIGN KEY (uploaded_by) REFERENCES users (id) ON DELETE SET NULL
);

CREATE INDEX ix_spot_images_spot_id
    ON spot_images (spot_id);

-- 每個景點至多一張封面
CREATE UNIQUE INDEX uq_spot_images_cover
    ON spot_images (spot_id)
    WHERE is_cover = true;
