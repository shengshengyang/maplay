# user-favorites Specification

## Purpose
TBD - created by archiving change add-public-spots-and-user-favorites. Update Purpose after archive.
## Requirements
### Requirement: 添加收藏
系統 SHALL 提供 `POST /api/favorites`，需登入，讓用戶收藏指定景點。若該景點已被收藏，MUST 回 409 `ALREADY_FAVORITED`；若景點不存在或已刪除，MUST 回 404 `SPOT_NOT_FOUND`。

請求體：`{ "spotId": "bigint" }`。

#### Scenario: 成功添加收藏
- **GIVEN** 資料庫存在一筆有效景點
- **WHEN** 已登入用戶收藏該景點
- **THEN** 回 201，收藏記錄建立成功，`created_at` 設為當前時間

#### Scenario: 重複收藏同一景點
- **GIVEN** 用戶已收藏該景點
- **WHEN** 再次收藏相同景點
- **THEN** 回 409 `ALREADY_FAVORITED`

#### Scenario: 收藏不存在的景點
- **WHEN** 收藏不存在的 spotId
- **THEN** 回 404 `SPOT_NOT_FOUND`

#### Scenario: 收藏已刪除的景點
- **GIVEN** 景點 `deleted_at` 非空
- **WHEN** 嘗試收藏該景點
- **THEN** 回 404 `SPOT_NOT_FOUND`

#### Scenario: 未登入添加收藏
- **WHEN** 未帶 accessToken 呼叫
- **THEN** 回 401 `UNAUTHORIZED`

### Requirement: 取消收藏
系統 SHALL 提供 `DELETE /api/favorites/{spotId}`，需登入，讓用戶取消收藏指定景點。若該景點未被收藏，MUST 回 404 `FAVORITE_NOT_FOUND`。

#### Scenario: 成功取消收藏
- **GIVEN** 用戶已收藏該景點
- **WHEN** 呼叫取消收藏
- **THEN** 回 204，收藏記錄刪除成功

#### Scenario: 取消未收藏的景點
- **GIVEN** 用戶未收藏該景點
- **WHEN** 嘗試取消收藏
- **THEN** 回 404 `FAVORITE_NOT_FOUND`

#### Scenario: 未登入取消收藏
- **WHEN** 未帶 accessToken 呼叫
- **THEN** 回 401 `UNAUTHORIZED`

### Requirement: 個人收藏列表
系統 SHALL 提供 `GET /api/favorites`，需登入，回傳當前用戶的所有收藏記錄，按 `created_at` 降冪排序。查詢 MUST 支援分頁（page、page Size，預設 pageSize=20、上限=100）。

回傳格式：每筆記錄包含景點基本信息（id、name、category、lat、lng、thumbnailUrl）與收藏時間 `created_at`。

#### Scenario: 取得個人收藏列表
- **GIVEN** 用戶有多筆收藏記錄
- **WHEN** 呼叫 GET /api/favorites
- **THEN** 回 200，收藏按時間倒序排列，包含景點基本信息

#### Scenario: 分頁查詢收藏列表
- **GIVEN** 用戶有超過 20 筆收藏記錄
- **WHEN** 使用 page=1、pageSize=20 查詢
- **THEN** 回傳前 20 筆收藏，總數正確

#### Scenario: 空收藏列表
- **GIVEN** 用戶從未收藏任何景點
- **WHEN** 查詢收藏列表
- **THEN** 回 200，收藏陣列為空

#### Scenario: 收藏列表包含已刪除景點
- **GIVEN** 用戶收藏的某景點後來被軟刪除
- **WHEN** 查詢收藏列表
- **THEN** 該景點仍出現在列表中（歷史記錄保留）

#### Scenario: 未登入查詢收藏列表
- **WHEN** 未帶 accessToken 呼叫
- **THEN** 回 401 `UNAUTHORIZED`

### Requirement: 檢查收藏狀態
系統 SHALL 提供 `GET /api/favorites/check/{spotId}`，需登入，讓用戶快速檢查是否已收藏指定景點，無需載入完整收藏列表。

#### Scenario: 景點已被收藏
- **GIVEN** 用戶已收藏該景點
- **WHEN** 檢查收藏狀態
- **THEN** 回 200，`{ "isFavorited": true }`

#### Scenario: 景點未被收藏
- **GIVEN** 用戶未收藏該景點
- **WHEN** 檢查收藏狀態
- **THEN** 回 200，`{ "isFavorited": false }`

#### Scenario: 檢查不存在的景點
- **WHEN** 檢查不存在的 spotId
- **THEN** 回 404 `SPOT_NOT_FOUND`

#### Scenario: 未登入檢查收藏狀態
- **WHEN** 未帶 accessToken 呼叫
- **THEN** 回 401 `UNAUTHORIZED`

### Requirement: 級聯刪除處理
系統 MUST 在用戶刪除時自動刪除該用戶的所有收藏記錄，在景點刪除時自動刪除該景點的所有收藏記錄，利用資料庫 `ON DELETE CASCADE` 約束確保引用完整性。

#### Scenario: 用戶刪除時清理收藏記錄
- **GIVEN** 用戶有多筆收藏記錄
- **WHEN** 該用戶被刪除
- **THEN** 所有相關收藏記錄自動刪除

#### Scenario: 景點刪除時清理收藏記錄
- **GIVEN** 某景點被��位用戶收藏
- **WHEN** 該景點被刪除
- **THEN** 所有相關收藏記錄自動刪除

### Requirement: 防止重複收藏
系統 MUST 使用複合唯一索引 `(user_id, spot_id)` 確保同一用戶不能重複收藏同一景點，違反時回 409 `ALREADY_FAVORITED`。

#### Scenario: 資料庫層級防重複
- **GIVEN** user_favorites 表有複合唯一索引
- **WHEN** 嘗試插入重複的 (user_id, spot_id) 組合
- **THEN** 資料庫拋出唯一性約束違例，轉為 409 錯誤

### Requirement: 收藏記錄創建時間
系統 MUST 在創建收藏記錄時自動設置 `created_at` 為當前時間戳，用於排序和追蹤收藏時間。

#### Scenario: 新收藏記錄包含時間戳
- **WHEN** 用戶添加新收藏
- **THEN** 記錄的 `created_at` 為當前 UTC 時間

#### Scenario: 收藏列表按時間排序
- **GIVEN** 用戶在不同時間收藏多個景點
- **WHEN** 查詢收藏列表
- **THEN** 結果按 `created_at` 降冪排列（最新的在前）

