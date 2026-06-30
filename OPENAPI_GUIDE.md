# Maplay API 文件使用說明

## 概述

Maplay 使用 **.NET 10 原生 OpenAPI** (Microsoft.AspNetCore.OpenApi) 來生成 API 文檔，並提供 **兩個互動式文檔介面**：Swagger UI 和 Scalar UI。

## 技術棧

- **OpenAPI 規範**: .NET 原生 Microsoft.AspNetCore.OpenApi 10.0.9
- **前端介面**: Swagger UI + Scalar UI (雙介面)
- **文檔格式**: OpenAPI v3.0 (JSON)
- **XML 註釋**: 已啟用 XML 文檔生成

## 存取 API 文檔

### 推薦：Swagger UI (穩定版本)

```bash
http://localhost:8080/swagger
```

**特點：**
- ✅ 穩定且廣泛使用
- ✅ 完整的 API 測試功能
- ✅ 優異的瀏覽器相容性
- ✅ 支援 JWT 認證測試

### 備選：Scalar UI (現代化介面)

```bash
http://localhost:8080/scalar/v1
```

**特點：**
- 🎨 現代化使用者介面
- 📱 響應式設計
- 🔍 簡潔的文檔展示

### OpenAPI JSON 端點

```bash
# 直接取得 OpenAPI JSON
http://localhost:8080/openapi/v1.json
```

### Docker 環境

```bash
# 啟動完整服務
docker-compose -f compose.yaml up -d

# 或使用分離部署
docker-compose -f compose-fix-windows.yaml up -d
docker-compose -f compose-app.yaml up -d --build
```

然後訪問：
- **Swagger UI**: http://localhost:8080/swagger
- **Scalar UI**: http://localhost:8080/scalar/v1

## API 端點總覽

目前專案包含以下主要 API 模組：

### 🔐 認證 API (`/api/auth`)
- `POST /api/auth/register` - 用戶註冊
- `POST /api/auth/login` - 用戶登入
- `POST /api/auth/refresh` - 刷新權杖
- `POST /api/auth/logout` - 用戶登出
- `GET /api/auth/me` - 獲取當前用戶資訊
- `POST /api/auth/google` - Google OAuth 登入
- `POST /api/auth/line` - LINE OAuth 登入

### 🏞️ 景點 API (`/api/spots`)
- 景點 CRUD 操作
- 地理位置查詢
- 分頁和篩選

### ⭐ 評價 API (`/api/reviews`)
- 評價 CRUD 操作
- 用戶評價管理

### 🛠️ 管理員 API (`/api/admin`)
- 景點管理功能
- 系統維護操作

### 📥 匯入 API (`/api/import`)
- 政府資料匯入功能

## 使用 Swagger UI (推薦)

### 基本操作

1. **瀏覽 API**: 頁面顯示所有可用的 API 端點
2. **展開端點**: 點擊任意端點查看詳細資訊
3. **測試 API**:
   - 點擊 "Try it out" 按鈕
   - 填寫必要參數
   - 點擊 "Execute" 執行請求
4. **認證**:
   - 點擊右上角 "Authorize" 按鈕
   - 輸入 `Bearer YOUR_TOKEN` (注意 Bearer 前綴)
   - 點擊 "Authorize" 確認

### 高級功能

- **下載文檔**: 點擊 API 文檔頂部的下載圖示
- **展開所有端點**: 點擊 "List Operations" 展開所有 API
- **搜尋功能**: 使用內建搜尋欄快速找到端點
- **回應範例**: 每個端點顯示預期的回應格式

## 使用 Scalar UI (備選)

### 基本操作

1. **瀏覽 API**: 左側面板顯示所有可用的 API 端點
2. **查看詳細資訊**: 點擊任意端點查看請求/回應格式
3. **測試 API**: 
   - 點擊 "Try it" 按鈕
   - 填寫必要參數
   - 點擊 "Send Request"
4. **認證**: 
   - 點擊 "Authorize" 按鈕
   - 輸入 JWT Token
   - 格式：`Bearer YOUR_TOKEN`

### 獲取 Token 進行測試

```bash
# 1. 註冊測試帳號
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@example.com","password":"Test1234!"}'

# 2. 登入取得 Token
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test1234!"}'

# 回應會包含 accessToken
# 3. 在 Scalar UI 中使用該 Token
```

## 程式碼中的文檔註釋

### 基本格式

```csharp
/// <summary>
/// 獲取用戶資訊
/// </summary>
/// <param name="id">用戶 ID</param>
/// <returns>用戶詳細資訊</returns>
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id, CancellationToken ct)
{
    // ...
}
```

### 回應類型註釋

```csharp
/// <response code="200">成功取得用戶資訊</response>
/// <response code="404">用戶不存在</response>
[HttpGet("{id}")]
[ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetUser(int id, CancellationToken ct)
{
    // ...
}
```

### 請求/回應模型

```csharp
/// <summary>
/// 用戶註冊請求
/// </summary>
public record RegisterRequest
{
    /// <summary>
    /// 用戶名稱
    /// </summary>
    /// <example>測試用戶</example>
    public string Username { get; init; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    /// <example>test@example.com</example>
    public string Email { get; init; }

    /// <summary>
    /// 密碼（至少 8 字元，包含大小寫字母、數字和特殊字元）
    /// </summary>
    /// <example>Test1234!</example>
    public string Password { get; init; }
}
```

## API 規範特性

### 統一回應格式

所有 API 端點都遵循統一的回應格式：

```json
{
  "success": true,
  "data": { ... },
  "error": null
}
```

或錯誤回應：

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "輸入驗證失敗",
    "details": { ... }
  }
}
```

### 錯誤碼定義

- `UNAUTHORIZED` - 未認證 (401)
- `TOKEN_EXPIRED` - 權杖已過期 (401)
- `FORBIDDEN` - 權限不足 (403)
- `NOT_FOUND` - 資源不存在 (404)
- `VALIDATION_ERROR` - 輸入驗證失敗 (400)
- `NOT_IMPLEMENTED` - 功能未實作 (501)

### JWT 認證

```bash
# 在 Authorization header 中包含 Token
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

# Token 會在 15 分鐘後過期
# Refresh Token 存儲在 HttpOnly Cookie 中
```

## 環境設定

### 開發環境

OpenAPI 文檔僅在開發環境中啟用（由 `app.Environment.IsDevelopment()` 控制）。

### 生產環境

生產環境不會公開 OpenAPI 文檔端點，如需在生產環境啟用：

```csharp
// Program.cs
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapGet("/scalar/{version}", async context => { ... });
}
```

## 進階使用

### 自訂 OpenAPI 文檔

```csharp
// 在 Program.cs 中自訂 OpenAPI 設定
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<CustomDocumentTransformer>();
});

// 自訂文檔轉換器
class CustomDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new()
        {
            Title = "親子資源地圖系統 API",
            Version = "v1",
            Description = "提供親子景點、評價和管理功能的 API"
        };
        return Task.CompletedTask;
    }
}
```

### API 版本控制

如需支援多版本 API：

```csharp
// 在 Controller 中指定版本
[ApiController]
[Route("api/v1/[controller]")]
[ApiVersion("1.0")]
public class SpotsController : ControllerBase
{
    // ...
}
```

## 故障排除

### 常見問題

1. **無法訪問 Scalar UI**
   - 確認應用程式正在運行：`docker ps`
   - 檢查端口 8080 是否正常：`netstat -an | findstr 8080`
   - 查看應用程式日誌：`docker logs maplay-maplay-1`

2. **OpenAPI JSON 為空**
   - 確認在開發環境運行
   - 檢查是否正確引用了 `Microsoft.AspNetCore.OpenApi` 套件

3. **文檔註釋未顯示**
   - 確認專案檔案中啟用了 `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
   - 檢查 XML 註釋格式是否正確

### 調試技巧

```bash
# 查看 OpenAPI 端點是否正確映射
curl http://localhost:8080/openapi/v1.json | jq .

# 檢查應用程式日誌中的錯誤
docker logs maplay-maplay-1 | findstr OpenAPI

# 進入容器檢查檔案系統
docker exec -it maplay-maplay-1 ls -la /app
```

## 介面選擇建議

### 使用 Swagger UI 當：
- 🎯 需要穩定且經過驗證的介面
- 🔧 需要完整的 API 測試功能
- 🌐 使用較舊的瀏覽器版本
- 📚 需要豐富的文檔和範例

### 使用 Scalar UI 當：
- 🎨 喜歡現代化的介面設計
- 📱 需要更好的移動端體驗
- 🔍 專注於 API 文檔瀏覽而非測試
- ⚡ 需要更快的載入速度

### 故障排除

如果 Scalar UI 無法載入（CDN 問題）：
1. **使用 Swagger UI**: 訪問 `http://localhost:8080/swagger`
2. **檢查網路連接**: 確認能訪問外部 CDN
3. **直接使用 OpenAPI JSON**: `http://localhost:8080/openapi/v1.json`

## 相關資源

- [.NET OpenAPI 文檔](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi)
- [Swagger UI 官方網站](https://swagger.io/tools/swagger-ui/)
- [Scalar UI 官方網站](https://scalar.com/)
- [OpenAPI 規範](https://swagger.io/specification/)
- [.NET 10 發行說明](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)

## 最佳實踐

1. **保持文檔更新**: 每次修改 API 時更新 XML 註釋
2. **提供範例**: 使用 `<example>` 標籤提供請求/回應範例
3. **明確錯誤碼**: 為所有錯誤回應定義清晰的錯誤碼和訊息
4. **測試 API**: 使用 Scalar UI 在開發過程中測試所有端點
5. **版本控制**: 為重大變更使用 API 版本控制

## 快速指令參考

```bash
# 啟動 API 服務
docker-compose -f compose-app.yaml up -d --build

# 查看 API 日誌
docker logs maplay-maplay-1 -f

# 重啟 API 服務
docker restart maplay-maplay-1

# 測試 API 連接
curl http://localhost:8080/openapi/v1.json | jq .

# 進入容器
docker exec -it maplay-maplay-1 /bin/bash
```

---

**注意**: API 文檔功能僅在開發環境中啟用。生產環境部署時請確保敏感資訊不會通過文檔端點洩漏。
