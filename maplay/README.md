# OpenSpec 規格交付說明

本資料夾以 [OpenSpec](https://github.com/Fission-AI/OpenSpec) 慣例撰寫，作為親子資源地圖系統的規格 source of truth，供 agent 後續以 OpenSpec 工作流（propose → apply → archive）開發。

## 目錄結構

```
openspec/
├── project.md                      # 專案脈絡、技術棧、能力清單、關鍵決策
└── specs/                          # 各能力的 source-of-truth 規格
    ├── auth/spec.md                # 認證：註冊/登入/Refresh/OAuth/個資
    ├── spot-management/spec.md     # 景點：查詢/詳情/回報/圖片(物件儲存)
    ├── reviews/spec.md             # 評價與評分
    ├── admin-moderation/spec.md    # 審查/編輯/軟刪除/使用者管理
    ├── gov-import/spec.md          # 政府開放資料匯入
    └── map-navigation/spec.md      # 地圖/標記/Google Maps 跳轉/篩選
```

## 格式慣例

- 每個能力一個 `spec.md`，含 `## Purpose` 與 `## Requirements`。
- 每條需求為 `### Requirement: <名稱>`，敘述使用 `SHALL` / `MUST` 關鍵字（OpenSpec 解析依賴此關鍵字）。
- 每條需求至少一個 `#### Scenario: <名稱>`，以 `**GIVEN** / **WHEN** / **THEN**` 描述可驗證行為（對應測試案例）。

## 本次納入的兩項決策

- **圖片採物件儲存**：見 `spot-management/spec.md` 之「景點圖片上傳至物件儲存」與「圖片儲存抽象」需求。
- **景點軟刪除**：見 `admin-moderation/spec.md` 之「景點軟刪除」需求（含 restore 與評價保留）。

## 如何讓 agent 接手（建議流程）

1. 安裝並初始化（Node.js ≥ 20.19）：
   ```bash
   npm install -g @fission-ai/openspec@latest
   cd <你的專案>
   openspec init
   ```
2. 將本 `openspec/` 內容放入專案根目錄的 `openspec/`（作為既有規格基線；OpenSpec 支援 brownfield，可直接以這些 spec 為 source of truth）。
3. 之後每個功能以變更提案推進，例如：
   ```
   /opsx:propose 實作 auth 能力的帳密登入與 Refresh 輪替
   /opsx:apply
   /opsx:archive
   ```
   propose 會在 `openspec/changes/<name>/` 產生 `proposal.md`、`specs/`(delta)、`design.md`、`tasks.md`；delta 以 `ADDED / MODIFIED / REMOVED` 標記與本基線比對。

## 與其他文件的關係

- `family-map-skeleton.md`：架構骨架（目錄、Schema、Docker、路由總覽），屬技術設計參考。
- `family-map-spec.md`：人類可讀的 SA/SD 規格書（資料字典、錯誤碼、權限矩陣、QA 檢核表），與本 OpenSpec 規格互補；本資料夾為 agent 可解析的機器友善版本。

## 建議實作順序（對應能力）

auth → spot-management（查詢/回報）→ map-navigation → reviews → admin-moderation → gov-import
