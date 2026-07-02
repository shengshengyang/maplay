# spot-management Specification (Delta)

## Purpose
調整整體景點查詢 API 的權限策略，將查詢類端點改為公開訪問，提升用戶體驗和平台可發現性。

## MODIFIED Requirements

### Requirement: 附近景點查詢
系統 SHALL 提供 `GET /api/spots/nearby`，**允許匿名訪問**，依中心座標與半徑回傳已核准且未刪除的景點，並附與中心點的距離（公尺），依距離升冪排序。期間限定景點 MUST 僅在 `start_date ≤ today ≤ end_date` ���回傳。查詢 MUST 使用 PostGIS GIST 索引，禁止全表掃描。

參數：`lat`(必), `lng`(必), `radius`(預設 2000、上限 20000), `category`(選), `age`(選), `spotType`(選)。

**變更說明：** 移除認證要求，未登入用戶也可查詢附近景點。

#### Scenario: 匿名用戶查詢附近景點
- **GIVEN** 資料庫存在多筆已核准景點
- **WHEN** 未登入用戶帶 lat/lng/radius 呼叫
- **THEN** 回傳景點陣列，每筆含 `distanceMeters`，並依距離由近至遠排序

#### Scenario: 查詢回傳含距離且排序正確
- **GIVEN** 資料庫存在多筆已核准景點
- **WHEN** 帶 lat/lng/radius 呼��
- **THEN** 回傳景點陣列，每筆含 `distanceMeters`，並依距離由近至遠排序

#### Scenario: 過期期間限定景點不顯示
- **GIVEN** 一筆 `end_date` 早於今日的 temporary 景點
- **WHEN** 查詢其所在範圍
- **THEN** 結果不包含該景點

#### Scenario: 未核准景點不顯示
- **GIVEN** 一筆 `status=pending` 的景點
- **WHEN** 查詢其所在範圍
- **THEN** 結果不包含該景點

#### Scenario: 座標缺漏
- **WHEN** 未提供 lat 或 lng
- **THEN** 回 400 `VALIDATION_ERROR`

#### Scenario: 半徑超過上限
- **WHEN** radius 大於 20000
- **THEN** 回 400 `VALIDATION_ERROR`

### Requirement: 景點詳情
系統 SHALL 提供 `GET /api/spots/{id}`，**允許匿名訪問已核准景點**，回傳景點完整資訊、圖片清單、評價統計（平均 rating、平均 clean_level、評價則數）與最新數則評價。未核准或已軟刪除的景點 MUST 對非 admin 且非回報本人回 404，以避免資訊洩漏。

**變更說明：** 已核准景點對匿名用戶開放，未核准景點仍保持訪問控制。

#### Scenario: 匿名用戶查詢已核准景點詳情
- **WHEN** 未登入用戶查詢已核准景點
- **THEN** 回 200 與完整詳情（含圖片與評價統計）

#### Scenario: 取得已核准景點詳情
- **WHEN** 以任意角色查詢已核准景點
- **THEN** 回 200 與完整詳情（含圖片與評價統計）

#### Scenario: 一般使用者查詢他人 pending 景點
- **GIVEN** 一筆他人回報且 `status=pending` 的景點
- **WHEN** 以非該回報者、非 admin 的身分查詢
- **THEN** 回 404 `SPOT_NOT_FOUND`

#### Scenario: 查詢已軟刪除景點
- **GIVEN** 一筆 `deleted_at` 非空的景點
- **WHEN** 以一般使用者查詢
- **THEN** 回 404 `SPOT_NOT_FOUND`

### Requirement: 進行中期間限定景點
系統 SHALL 提供 `GET /api/spots/active-temp`（分頁），**允許匿名訪問**，回傳目前在效期內、已核准、未刪除的 temporary 景點。

**變更說明：** 移除認證要求，未登入用戶也可查詢進行中的期間限定活動。

#### Scenario: 匿名用戶查詢進行中活動
- **GIVEN** 多筆 temporary 景點，部分在效期內
- **WHEN** 未��入用戶呼叫該端點
- **THEN** 僅回傳效期內者

#### Scenario: 僅回傳效期內活動
- **GIVEN** 多筆 temporary 景點，部分在效期內、部��已過期
- **WHEN** 呼叫該端點
- **THEN** 僅回傳效期內者

## REMOVED Requirements

無移除需求。

## RENAMED Requirements

無重命名需求。

## Notes

- **權限調整範圍：** 僅調整查詢類端點（GET），操作類端點（POST、PUT、DELETE）仍需認證
- **數據安全：** 公開訪問的景點數據已過濾敏感信息（管理標記、審查狀態等）
- **向後相容：** 已認證用戶的行為不變，可選擇登入獲得完整功能
- **前端適配：** 前端需調整認證守衛，允許未登入用戶訪問景點相關頁面
