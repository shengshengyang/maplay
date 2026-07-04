## Context

專案目前缺乏 API 文件與測試介面，開發者與前端團隊無法快速了解 API 結構。系統基於 ASP.NET Core 8 Web API，現有完整的功能端點 (認證、景點管理、評價、管理等)，但缺乏互動式文件。專案已建立 JWT 認證機制，API 端點已有基本授權控制，需要將這些機制整合至 Swagger 測試環境中。

## Goals / Non-Goals

**Goals:**
- 提供互動式 API 文件介面 (Swagger UI)
- 自動產生符合 OpenAPI 3.0 規格
- 支援 JWT Token 認證測試
- 環境隔離：開發啟用，生產停用
- 完整的 API 說明與測試功能

**Non-Goals:**
- 不修改既有 API 行為或回應格式
- 不變更認證授權邏輯
- 不建立自動化 API 測試套件
- 不產生客戶端 SDK
- 不影響既有 API 效能

## Decisions

### Swagger 套件選擇
**決策：** 使用 Swashbuckle.AspNetCore
**理由：**
- 微軟官方推薦，與 ASP.NET Core 8 原生整合
- 自動產生 OpenAPI 3.0 規格，無需手動編寫
- 內建 Swagger UI 介面，開箱即用
- 完整支援 JWT 認證配置
- 社群活躍，文件與範例豐富

**替代方案考慮：**
- NSwag：功能更強大但配置複雜，學習曲線較陡
- 手動編寫 OpenAPI 規格：維護成本高，容易與實際 API 不同步

### JWT 認證整合策略
**決策：** 使用 Swashbuckle JWT Bearer 認證配置
**理由：**
- Swagger UI 原生支援 Bearer Token 認證
- 配置簡單，僅需在 AddSwaggerGen 中設定
- 不影響既有 JWT 驗證邏輯
- 開發者可複製實際 JWT Token 進行測試

**實作方式：**
```csharp
services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer Token 認證",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

### 環境隔離配置
**決策：** 基於 ASPNETCORE_ENVIRONMENT 環境變數控制
**理由：**
- 符合 .NET 慣例，開發/生產環境自然分離
- 配置簡單，條件編譯或條件中介層即可實現
- 生產環境完全停用，避免安全風險
- 無需額外配置檔案或環境變數

**實作方式：**
```csharp
// Program.cs
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen();
    // Swagger 配置...
}

// app.UseSwagger();
// app.UseSwaggerUI();
```

### XML 註解支援
**決策：** 啟用 XML 註解並整合至 Swagger 文件
**理由：**
- ASP.NET Core 原生支援 XML 文件註解
- 可從程式碼註解自動產生 API 說明
- 減少維護成本，文件與程式碼同步
- 提升文件完整性和可讀性

**實作步驟：**
1. 專案檔啟用 XML 文件產生
2. 控制器與模型加入 XML 註解
3. SwaggerGen 包含 XML 註解檔案

### API 端點分組策略
**決策：** 按 API 功能模組分組顯示
**理由：**
- 對應既有能力架構 (auth, spot-management, reviews, admin-moderation 等)
- 提升文件可讀性與組織性
- 便於前端開發者快速找到相關端點

**分組方式：**
```csharp
// 控制器中的 ApiExplorerSettings.GroupName
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "Authentication")]
public class AuthController : ControllerBase
```

## Risks / Trade-offs

### 風險：生產環境誤啟用 Swagger
**緩解：**
- 使用條件編譯確保生產環境不註冊 Swagger 服務
- Docker Compose 生產配置不設定相關環境變數
- CI/CD 流程中加入檢查，確保生產建置不包含 Swagger

### 風險：JWT Token 測試安全性
**緩解：**
- Swagger UI 僅在開發環境啟用
- 開發環境 Token 時限較短，降低風險
- 文件中說明 Token 使用注意事项

### 風險：效能影響
**緩解：**
- Swagger 中介層僅在開發環境載入
- 生產環境完全不載入 Swagger 相關套件
- 監控開發環境 API 回應時間，確認無顯著差異

### 取捨：XML 註解 vs JSON Schema
**取捨：** 優先使用 XML 註解，僅在複雜模型時補充 Schema 屬性
**理由：**
- XML 註解更符合 .NET 開發者習慣
- 自 Visual Studio IntelliSense 自動完成
- 減少重複定義，維護性更佳

### 取捨：Swagger UI 樣式客製化
**取捨：** 採用預設樣式，僅做基本客製化
**理由：**
- 預設樣式已足夠清晰好用
- 過度客製化增加維護成本
- 未來可依需求調整，初期保持簡單

## 部署計畫

### 開發環境部署
1. NuGet 套件安裝：`Swashbuckle.AspNetCore`
2. Program.cs 加入 Swagger 配置
3. 控制器與模型加入 XML 註解
4. 本地測試驗證功能正常

### 生產環境部署
1. 確認生產配置未啟用 Swagger
2. 部署時驗證 /swagger 端點回傳 404
3. 監控應用程式效能，確認無異常

### 回滾策略
- 移除 Swashbuckle.AspNetCore 套件引用
- 移除 Program.cs 中 Swagger 相關程式碼
- 回滾影響範圍僅限於開發體驗，無資料庫或 API 行為變更

## 待解決問題

### 無待解決問題
本設計無需額外決策，所有技術選擇已明確。主要實作步驟包含於任務清單中，可依序執行。

### 未來擴展考慮 (非本期範圍)
- API 版本控制 (如 /api/v1, /api/v2)
- 自動化 API 測試整合
- 客戶端 SDK 自動產生
- API 變更歷史追蹤