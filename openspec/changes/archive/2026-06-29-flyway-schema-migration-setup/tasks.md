## 1. 檢查現有遷移檔案依賴關係

- [x] 1.1 靜態分析 V1-V7 遷移檔案內容
- [x] 1.2 繪製資料表依賴關係圖 (users ← refresh_tokens, spots ← spot_images/reviews)
- [x] 1.3 驗證遷移版本號順序符合依賴關係
- [x] 1.4 檢查所有 foreign key constraints 指向的資料表皆在先前版本建立
- [x] 1.5 驗證索引與 constraint 建立順序正確

**驗證：** 所有 FK 皆指向已存在的資料表，依賴關係圖為 acyclic directed graph

## 2. 建立 Flyway 設定檔

- [x] 2.1 建立 `flyway.conf` 於專案根目錄
- [x] 2.2 配置資料庫連線資訊 (URL, user, password)
- [x] 2.3 設定遷移檔案路徑 (`flyway.locations=filesystem:db/migration`)
- [x] 2.4 啟用遷移驗證 (`flyway.validate-on-migrate=true`)
- [x] 2.5 設定基線與基礎版本 (`flyway.baseline-on-migrate=true`)
- [x] 2.6 建立 `.env` 範本檔案供環境變數覆蓋

**驗證：** `flyway info` 指令可正確讀取配置並列出待執行遷移

## 3. 執行遷移驗證

- [ ] 3.1 於本地 fresh database 執行完整遷移 (V1-V7)
- [ ] 3.2 驗證 `flyway_schema_history` 表記錄正確
- [ ] 3.3 檢查所有資料表、索引、constraint 建立成功
- [ ] 3.4 測試重複執行遷移 (idempotent，不應重複建立)
- [ ] 3.5 模擬遷移失敗情境 (如修改某遷移檔案引入錯誤)
- [ ] 3.6 驗證失敗後的正確 rollback 行為

**驗證：** `flyway migrate` 於空庫執行成功，重複執行跳過已安裝版本，失敗時停止後續遷移

## 4. Docker 整合

- [x] 4.1 更��� `docker-compose.yml` 加入 Flyway 服務
- [x] 4.2 設定服務依賴順序 (PostgreSQL → Flyway → Web API)
- [x] 4.3 配置 Flyway 容器連線至 PostgreSQL 服務
- [x] 4.4 設定 PostgreSQL healthcheck 確保就緒後才執行遷移
- [x] 4.5 測試 Docker Compose 完整啟動流程
- [x] 4.6 驗證容器重啟時遷移不被重複執行

**驗證：** `docker-compose up` 後 Web API 成功啟動且資料庫 schema 正確，重新啟動不重複執行遷移

## 5. EF Core 整合驗證

- [x] 5.1 確認 `DbContext` 設定不啟用 EF Core Migrations
- [x] 5.2 驗證 EF Core 僅用於查詢與物件對應
- [x] 5.3 確認 `OnModelCreating` 不包含 schema 修改邏輯
- [x] 5.4 測試 EF Core 可正確對應至 Flyway 建立的資料表

**驗證：** EF Core 查詢功能正常，且不嘗試管理 schema

## 6. 文件與程序建立

- [x] 6.1 撰寫「新增遷移檔案標準流程」文件
- [x] 6.2 說明遷移檔案命名規則與依賴檢查要點
- [x] 6.3 建立「遷移錯誤排查」指南
- [x] 6.4 更新開發者手冊，加入 Flyway 使用章節
- [x] 6.5 於專案 README 加入資料庫遷移設定說明

**驗證：** 文件涵蓋從新增遷移到執行驗證的完整流程，且包含常見錯誤處理