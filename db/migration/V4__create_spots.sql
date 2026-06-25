-- V4: spots 表（FK -> users）
-- 集中所有跨 spec 欄位於同一 migration：
--   spot-management：name, category, status, spot_type, start/end_date, age_groups,
--                     facilities, location, submitted_by, source, gov_data_id, deleted_at
--   admin-moderation：reviewed_by, reviewed_at, reject_reason
--   gov-import：gov_data_id (UNIQUE), source
-- age_groups / facilities 採 TEXT[]，值域由應用層驗證（不設 CHECK，便於擴充）。

CREATE TABLE spots (
    id            UUID                    PRIMARY KEY DEFAULT gen_random_uuid(),
    name          VARCHAR(200)            NOT NULL,
    description   TEXT,
    category      VARCHAR(30)             NOT NULL,
    status        VARCHAR(20)             NOT NULL DEFAULT 'pending',
    spot_type     VARCHAR(20)             NOT NULL DEFAULT 'permanent',
    start_date    DATE,
    end_date      DATE,
    age_groups    TEXT[]                  NOT NULL DEFAULT '{}',
    facilities    TEXT[]                  NOT NULL DEFAULT '{}',
    location      geometry(Point, 4326)   NOT NULL,
    address       TEXT,
    submitted_by  UUID,
    source        VARCHAR(20)             NOT NULL DEFAULT 'user',
    gov_data_id   VARCHAR(255),
    reviewed_by   UUID,
    reviewed_at   TIMESTAMPTZ,
    reject_reason TEXT,
    source_url    TEXT,
    crawled_at    TIMESTAMPTZ,
    deleted_at    TIMESTAMPTZ,
    created_at    TIMESTAMPTZ             NOT NULL DEFAULT now(),
    updated_at    TIMESTAMPTZ             NOT NULL DEFAULT now(),

    CONSTRAINT fk_spots_submitted_by
        FOREIGN KEY (submitted_by) REFERENCES users (id) ON DELETE SET NULL,
    CONSTRAINT fk_spots_reviewed_by
        FOREIGN KEY (reviewed_by)  REFERENCES users (id) ON DELETE SET NULL,

    CONSTRAINT chk_spots_status    CHECK (status    IN ('pending', 'approved', 'rejected')),
    CONSTRAINT chk_spots_spot_type CHECK (spot_type IN ('permanent', 'temporary')),
    CONSTRAINT chk_spots_source    CHECK (source    IN ('user', 'gov_import')),

    CONSTRAINT chk_spots_temporal CHECK (
        (spot_type = 'temporary'
            AND start_date IS NOT NULL
            AND end_date   IS NOT NULL
            AND end_date >= start_date)
        OR
        (spot_type = 'permanent'
            AND start_date IS NULL
            AND end_date   IS NULL)
    )
);

CREATE INDEX ix_spots_location_gist
    ON spots USING GIST (location);

CREATE INDEX ix_spots_location_geog_gist
    ON spots USING GIST ((location::geography));

CREATE UNIQUE INDEX uq_spots_gov_data_id
    ON spots (gov_data_id)
    WHERE gov_data_id IS NOT NULL;

CREATE INDEX ix_spots_filter
    ON spots (status, deleted_at, spot_type, end_date);

CREATE INDEX ix_spots_category
    ON spots (category);

CREATE INDEX ix_spots_submitted_by
    ON spots (submitted_by);
