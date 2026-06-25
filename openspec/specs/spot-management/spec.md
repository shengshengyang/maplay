# spot-management Specification

## Purpose
管理親子景點的查詢、詳情、回報與圖片。景點分永久與期間限定，含分類、適合年齡、設施標籤與地理座標；圖片儲存於 S3 相容物件儲存。

## Requirements

### Requirement: 附近景點查詢
系統 SHALL 提供 `GET /api/spots/nearby`，依中心座標與半徑回傳已核准且未刪除的景點，並附與中心點的距離（公尺），依距離升冪排序。期間限定景點 MUST 僅在 `start_date ≤ today ≤ end_date` 時回傳。查詢 MUST 使用 PostGIS GIST 索引，禁止全表掃描。

參數：`lat`(必), `lng`(必), `radius`(預設 2000、上限 20000), `category`(選), `age`(選), `spotType`(選)。

#### Scenario: 查詢回傳含距離且排序正確
- **GIVEN** 資料庫存在多筆已核准景點
- **WHEN** 帶 lat/lng/radius 呼叫
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
系統 SHALL 提供 `GET /api/spots/{id}`，回傳景點完整資訊、圖片清單、評價統計（平均 rating、平均 clean_level、評價則數）與最新數則評價。未核准或已軟刪除的景點 MUST 對非 admin 且非回報本人回 404，以避免資訊洩漏。

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
系統 SHALL 提供 `GET /api/spots/active-temp`（分頁），回傳目前在效期內、已核准、未刪除的 temporary 景點。

#### Scenario: 僅回傳效期內活動
- **GIVEN** 多筆 temporary 景點，部分在效期內、部分已過期
- **WHEN** 呼叫該端點
- **THEN** 僅回傳效期內者

### Requirement: 回報新景點
系統 SHALL 提供 `POST /api/spots`，需登入，建立 `status=pending`、`submitted_by=當前使用者`、`source=user` 的景點。

驗證：name 必填 1–200；category、ageGroups、facilities 之值 MUST 為合法列舉；lat/lng 必填且有效。spotType=temporary 時 start_date、end_date 必填且 `end_date ≥ start_date`；spotType=permanent 時兩者 MUST 為 null。

#### Scenario: 成功回報永久景點
- **WHEN** 已登入使用者提交合法的 permanent 景點資料
- **THEN** 回 201，景點 `status=pending` 且不出現在公開查詢

#### Scenario: 期間限定日期不合法
- **WHEN** 提交 spotType=temporary 但 end_date 早於 start_date
- **THEN** 回 422 `TEMP_DATE_INVALID`

#### Scenario: 未登入回報
- **WHEN** 未帶 accessToken 呼叫
- **THEN** 回 401 `UNAUTHORIZED`

#### Scenario: 非法列舉值
- **WHEN** ageGroups 含不在 `0-3/3-7/7-12/12+` 的值
- **THEN** 回 400 `VALIDATION_ERROR`

### Requirement: 景點圖片上傳至物件儲存
系統 SHALL 提供 `POST /api/spots/{id}/images`（multipart/form-data），需登入，將圖片上傳至 S3 相容**物件儲存**並回傳可公開存取的 URL。後端 MUST 透過儲存抽象介面操作，業務碼不得直接依賴特定供應商 SDK。允許格式 jpg/jpeg/png/webp，單檔 ≤ 5MB，單次至多 5 張。

#### Scenario: 成功上傳並寫入物件儲存
- **WHEN** 已登入使用者上傳合法圖片
- **THEN** 圖片寫入物件儲存，DB 記錄其 URL 與 `uploaded_by`，回 201

#### Scenario: 首張圖片設為封面
- **GIVEN** 景點目前無圖片
- **WHEN** 上傳第一張圖片
- **THEN** 該圖片 `is_cover=true`

#### Scenario: 檔案格式不符
- **WHEN** 上傳非 jpg/jpeg/png/webp 的檔案
- **THEN** 回 400 `FILE_TYPE_INVALID`

#### Scenario: 檔案過大
- **WHEN** 上傳單檔超過 5MB
- **THEN** 回 400 `FILE_TOO_LARGE`

### Requirement: 圖片儲存抽象
系統 SHALL 以介面（如 `IObjectStorage`）抽象物件儲存操作（上傳、刪除、取得 URL），使本地開發可用相容實作（如 MinIO），正式環境可切換至雲端物件儲存而不變更業務碼。

#### Scenario: 切換供應商不影響業務碼
- **GIVEN** 圖片上傳邏輯僅依賴 `IObjectStorage`
- **WHEN** 替換為不同物件儲存實作
- **THEN** 上傳/刪除行為不變，無需修改 Controller 或 Service 業務邏輯
