## Context

專案採用 Flyway 作為資料庫 schema 版本控制工具，目前已建立 V1-V7 共 7 個遷移檔案。現有遷移涵蓋：
- V1: 啟用 PostGIS 與 pgcrypto extensions
- V2: 建立使用者表 (users)
- V3: 建立更新權杖表 (refresh_tokens，FK → users)
- V4: 建立景點表 (spots，FK → users)
- V5: 建立景點圖片表 (spot_images，FK → spots, users)
- V6: 建立評價表 (reviews，FK → spots, users)
- V7: 建立匯入歷史表 (import_history，FK → users)

技術棧約束：Schema 擁有權歸 Flyway，EF Core 僅用於查詢與物件對應，嚴禁啟用或執行 EF Core Migrations。

## Goals / Non-Goals

**Goals:**
- 驗證現有 Flyway 遷移檔案的依賴關係正確性
- 確認遷移執行順序符合 foreign key 依賴
- 建立 Flyway 設定檔最佳實踐
- 提供遷移執行與驗證程序文件

**Non-Goals:**
- 不修改現有 schema 欄位定義或資料表結構
- 不新增額外的資料表或欄位
- 不變更 EF Core 與 Flyway 的權責劃分
- 不涉及資料遷移或轉換邏輯

## Decisions

### 遷移順序驗證策略
**決策：** 依賴關係靜態分析 + 執行時驗證
**理由：**
- 靜態分析可提前發現顯式依賴錯誤 (如 FK 指向不存在的資料表)
- 執行時驗證可確保實際部署時 schema 狀態正確
- 較全人工檢查更可靠，較全自動化更靈活

**替代方案考慮：**
- 僅執行時驗證：缺點是錯誤發現較晚，部署時才失敗
- 僅靜態分析：缺點是無法檢測跨 schema 或隱含依賴

### Flyway 設定檔結構
**決策：** 使用 `flyway.conf` 配置檔 + 環境變數覆蓋
**理由：**
- `flyway.conf` 提供預設配置，便於版本控制
- 環境變數允許不同環境 (dev/staging/prod) 差異化配置
- 符合 12-factor app 原則

**關鍵配置項：**
```conf
flyway.url=jdbc:postgresql://localhost:5432/maplay
flyway.user=postgres
flyway.password=postgres
flyway.locations=filesystem:db/migration
flyway.baseline-on-migrate=true
flyway.validate-on-migrate=true
```

### 遷移檔案命名慣例
**決策：** 嚴��遵循 Flyway 版本化命名規則
**理由：**
- `V{version}__{description}.sql` 格式確保執行順序可預測
- 版本號使用遞增整數 (V1, V2, V3) 較時間戳記更易於 review
- 描述使用 snake_case 便於閱讀與檔案管理

### Docker 整合策略
**決策：** Docker Compose 啟動順序：PostgreSQL → Flyway → Web API
**理由：**
- 確保資料庫服務完全就緒後才執行遷移
- 遷移完成後 Web API 才啟動，避免 schema 不一致
- 使用 `depends_on` 與 `healthcheck` 確保服務依賴順序

## Risks / Trade-offs

**風險：** 現有遷移順序可能存在隱藏依賴問題
**緩解：** 執行完整遷移測試，確認所有環境 (本地/Docker) 可重現執行

**風險：** 不同環境可能有 schema drift (未通過 Flyway 的修改)
**緩解：** 啟用 `flyway.validate-on-migrate=true`，執行前驗證已安裝版本與遷移檔案一致性

**取捨：** 強制所有 schema 變更通過 Flyway 可能增加開發流程複雜度
**理由：** 相較於靈活性，schema 一致性與可追溯性更重要，特別在團隊協作環境

**取捨：** 遷移檔案使用 DDL 而非程式化遷移 (如 C# migrations)
**理由：** SQL 直接對應 DB 操作，更直觀且便於 DBA review，且避免與 EF Core 混淆