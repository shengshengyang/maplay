# gov-import Specification

## Purpose
讓管理員匯入政府開放資料（公共廁所、公園綠地等）成為景點，含去重、交易保護與匯入紀錄。

## Requirements

### Requirement: 廁所資料匯入
系統 SHALL 提供 `POST /api/admin/import/toilet`，解析 data.gov.tw 廁所資料並逐筆建立 `category=toilet`、`source=gov_import`、`status=approved` 的景點。以 `gov_data_id` 比對，已存在者 MUST 跳過並計入 skipped。

#### Scenario: 首次匯入建立景點
- **WHEN** admin 上傳含 N 筆未匯入過的廁所資料
- **THEN** 建立 N 筆 approved 景點，回傳 success=N、skipped=0

#### Scenario: 重複匯入跳過
- **GIVEN** 部分 `gov_data_id` 已存在
- **WHEN** 再次匯入同一資料集
- **THEN** 已存在者被跳過並計入 skipped，不建立重複景點

### Requirement: 公園資料匯入
系統 SHALL 提供 `POST /api/admin/import/park`，解析 GeoJSON FeatureCollection，自 geometry 取座標、自 properties 取名稱建立景點。

#### Scenario: 解析 GeoJSON 建立景點
- **WHEN** admin 上傳合法 GeoJSON FeatureCollection
- **THEN** 依各 Feature 建立對應 `category=park` 景點

### Requirement: 自訂欄位對應匯入
系統 SHALL 提供 `POST /api/admin/import/custom`，接受上傳資料與「來源欄位→系統欄位」對應表，依對應轉換後批次寫入。

#### Scenario: 依對應表轉換
- **WHEN** admin 上傳資料並提供欄位對應
- **THEN** 系統依對應將來源欄位寫入對應系統欄位後建立景點

### Requirement: 匯入交易保護與紀錄
系統 SHALL 將每次批次匯入包於單一交易，任一筆寫入失敗 MUST 整批 rollback。每次匯入 SHALL 寫入一筆 `import_history`（資料集、操作者、total/success/skipped/failed），並提供 `GET /api/admin/import/history` 查詢。

#### Scenario: 匯入失敗整批回滾
- **GIVEN** 匯入過程中某筆資料導致寫入失敗
- **WHEN** 交易提交前發生錯誤
- **THEN** 整批變更回滾，資料庫無部分寫入

#### Scenario: 匯入紀錄可查
- **WHEN** admin 查詢匯入紀錄
- **THEN** 回傳歷次匯入的資料集、時間與 success/skipped/failed 統計
