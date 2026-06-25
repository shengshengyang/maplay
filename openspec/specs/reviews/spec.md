# reviews Specification

## Purpose
管理使用者對景點的評價與評分，含整體評分（rating）與乾淨程度（clean_level），每位使用者每個景點至多一則評價。

## Requirements

### Requirement: 新增評價
系統 SHALL 提供 `POST /api/spots/{id}/reviews`，需登入。rating 與 clean_level MUST 為 1–5 整數；目標景點 MUST 為已核准狀態。同一使用者對同一景點重複評價時 MUST 回 409。

#### Scenario: 成功新增評價
- **WHEN** 已登入使用者對已核准景點提交 rating、cleanLevel(1–5)、content、visitedAt
- **THEN** 回 201 並建立評價

#### Scenario: 重複評價
- **GIVEN** 使用者已對該景點留過評價
- **WHEN** 再次提交評價
- **THEN** 回 409 `REVIEW_DUPLICATE`

#### Scenario: 評分超出範圍
- **WHEN** rating 或 cleanLevel 不在 1–5
- **THEN** 回 400 `VALIDATION_ERROR`

#### Scenario: 對未核准景點評價
- **WHEN** 目標景點 `status` 非 approved
- **THEN** 回 422 `SPOT_NOT_APPROVED`

### Requirement: 修改自己的評價
系統 SHALL 提供 `PUT /api/spots/{id}/reviews/{reviewId}`，使用者 MUST 僅能修改自己的評價，否則回 403。

#### Scenario: 修改本人評價
- **WHEN** 評價作者修改自己的評價內容或評分
- **THEN** 回 200 並更新

#### Scenario: 修改他人評價
- **WHEN** 非作者嘗試修改他人評價
- **THEN** 回 403 `FORBIDDEN`

### Requirement: 評價統計彙整
系統 SHALL 於景點詳情提供評價統計：平均 rating、平均 clean_level、評價總則數，統計 MUST 排除已軟刪除景點之外的有效評價。

#### Scenario: 詳情含評價統計
- **GIVEN** 一景點有多則評價
- **WHEN** 查詢景點詳情
- **THEN** 回傳平均 rating、平均 clean_level 與總則數
