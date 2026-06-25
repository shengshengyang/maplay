-- V6: reviews 表（FK → spots, users）
-- rating / clean_level 為 1–5；每位使用者對每個景點至多一則評價。
-- 軟刪除景點時評價資料列保留（FK 不設 spot 刪除即清，僅 spots 採軟刪除）。

CREATE TABLE reviews (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    spot_id     UUID        NOT NULL,
    user_id     UUID        NOT NULL,
    rating      SMALLINT    NOT NULL,
    clean_level SMALLINT    NOT NULL,
    content     TEXT,
    visited_at  DATE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at  TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_reviews_spot
        FOREIGN KEY (spot_id) REFERENCES spots (id) ON DELETE CASCADE,
    CONSTRAINT fk_reviews_user
        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE,

    CONSTRAINT chk_reviews_rating      CHECK (rating      BETWEEN 1 AND 5),
    CONSTRAINT chk_reviews_clean_level CHECK (clean_level BETWEEN 1 AND 5),

    -- 每位使用者每個景點至多一則
    CONSTRAINT uq_reviews_spot_user UNIQUE (spot_id, user_id)
);

CREATE INDEX ix_reviews_spot_id
    ON reviews (spot_id);

CREATE INDEX ix_reviews_user_id
    ON reviews (user_id);
