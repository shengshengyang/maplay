# Maplay API 文檔使用說明 (已修正)

## ✅ 問題已解決！

經過搜尋 .NET 10 OpenAPI 配置相關文件，現在提供了兩個可用的 API 文檔介面：

## 🌐 可用的 API 文檔介面

### 1. Swagger UI (經典穩定版)
```
http://localhost:8080/swagger
```
**特點：**
- ✅ 經過驗證的穩定介面
- ✅ 完整的 API 測試功能
- ✅ JWT 認證測試：點擊 "Authorize" 按鈕
- ✅ 優異的瀏覽器相容性
- ✅ 豐富的文檔和範例

### 2. Scalar UI (現代化介面)
```
http://localhost:8080/scalar
```
**特點：**
- 🎨 現代化使用者介面
- 📱 響應式設計
- 🔍 簡潔的文檔展示
- ⚡ 更快的載入速度

### 3. OpenAPI JSON
```
http://localhost:8080/openapi/v1.json
```
**特點：**
- 📄 機器讀取格式
- 🔄 可用於生成客戶端 SDK
- 🛠️ 整合到其他工具

## 🔧 修復過程

### 遇到的問題
1. **Scalar UI 空白頁面** - CDN 資源載入失敗
2. **Swagger UI 錯誤** - "No layout defined for 'StandaloneLayout'"

### 解決方案
1. **移除套件衝突** - 移除 Swashbuckle.AspNetCore，改用純前端實現
2. **修正佈局配置** - 將 Swagger UI 從 "StandaloneLayout" 改為 "BaseLayout"
3. **簡化實現** - 使用 `app.MapGet()` 直接提供 HTML 介面

## 🚀 快速開始

### 測試腳本
```powershell
# 自動開啟兩個介面
.\test-api-working.ps1
```

### 手動測試
```powershell
# 開啟 Swagger UI
Start-Process http://localhost:8080/swagger

# 開啟 Scalar UI
Start-Process http://localhost:8080/scalar

# 測試 OpenAPI JSON
Invoke-WebRequest http://localhost:8080/openapi/v1.json
```

### API 測試範例
```bash
# 1. 註冊測試帳號
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@example.com","password":"Test1234!"}'

# 2. 登入取得 Token
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test1234!"}'

# 3. 使用 Token 測試認證 API
curl -X GET http://localhost:8080/api/auth/me \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## 📚 參考資源

### 官方文件
- [ASP.NET Core OpenAPI 文檔](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/using-openapi-documents?view=aspnetcore-10.0) - Microsoft Learn
- [.NET 10 OpenAPI 增強功能](https://medium.com/@sidharth.cp34/openapi-swagger-enhancements-in-asp-net-core-10-the-complete-2025-guide-2fa6da93a7fb) - Medium 完整指南
- [Scalar ASP.NET Core 整合](https://scalar.com/products/api-references/integrations/aspnetcore/integration) - 官方文檔

### 技術文章
- [.NET 10 的 OpenAPI 新功能](https://servicestack.net/posts/openapi-net10) - ServiceStack
- [Swagger 替代方案比較](https://nikolatech.net/blogs/swagger-alternatives-openapi-dotnet) - NikolaTech
- [為什麼在 .NET 10 中選擇 Scalar](https://medium.com/@robmason777/with-net-10-now-is-a-good-time-to-try-scalar-instead-of-swagger-86ebda7fa23c) - Medium

### 套件資源
- [Scalar.AspNetCore 1.2.10](https://www.nuget.org/packages/Scalar.AspNetCore/1.2.10) - NuGet 套件
- [Microsoft.AspNetCore.OpenApi 10.0.9](https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi/10.0.9) - NuGet 套件

## 💡 使用建議

### 選擇介面

**開發測試**: 使用 **Swagger UI** (http://localhost:8080/swagger)
- 更穩定和成熟
- 完整的測試功能
- 豐富的錯誤訊息

**文檔展示**: 使用 **Scalar UI** (http://localhost:8080/scalar)
- 更現代化的介面
- 更好的使用者體驗
- 適合展示給外部開發者

**機器整合**: 使用 **OpenAPI JSON** (http://localhost:8080/openapi/v1.json)
- SDK 生成
- 自動化測試
- API 整合

### 認證測試

**Swagger UI:**
1. 點擊右上角 "Authorize" 按鈕
2. 輸入 `Bearer YOUR_TOKEN` (注意 Bearer 前綴)
3. 點擊 "Authorize" 確認
4. 測試需要認證的端點

**Scalar UI:**
1. 點擊頁面右上角的鎖圖示
2. 輸入 `Bearer YOUR_TOKEN`
3. 點擊 "Set value"
4. 測試需要認證的端點

## 🐛 故障排除

### 如果介面無法載入

**Scalar UI 空白:**
1. 檢查瀏覽器控制台錯誤訊息
2. 確認能訪問 CDN (cdn.jsdelivr.net)
3. 嘗試使用 Swagger UI 作為備選

**Swagger UI 錯誤:**
1. 確認應用程式正在運行
2. 檢查 OpenAPI JSON 是否可用
3. 清除瀏覽器快取

**認證無法使用:**
1. 確認 Token 格式正確 (`Bearer YOUR_TOKEN`)
2. 檢查 Token 是否過期
3. 重新登入取得新 Token

## 📝 版本資訊

- **.NET**: 10.0
- **Microsoft.AspNetCore.OpenApi**: 10.0.9
- **專案**: Maplay 親子資源地圖系統
- **最後更新**: 2025-06-30

---

**Sources:**
- [Microsoft Learn - Use OpenAPI documents](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/using-openapi-documents?view=aspnetcore-10.0)
- [Scalar Official Documentation](https://scalar.com/products/api-references/integrations/aspnetcore/integration)
- [.NET 10 OpenAPI Guide](https://medium.com/@sidharth.cp34/openapi-swagger-enhancements-in-asp-net-core-10-the-complete-2025-guide-2fa6da93a7fb)
- [ServiceStack .NET 10 OpenAPI](https://servicestack.net/posts/openapi-net10)
