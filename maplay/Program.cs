using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Maplay.Auth;
using Maplay.Common;
using Maplay.Data;
using Maplay.GovImport;
using Maplay.Reviews;
using Maplay.Spots;
using Maplay.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---- Options ----
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<OAuthProviderOptions>(builder.Configuration.GetSection("OAuth"));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

// ---- EF Core（僅查詢/對應，schema 由 Flyway 管理）----
var connString = builder.Configuration.GetConnectionString("Default")
                 ?? throw new InvalidOperationException("缺少 ConnectionStrings:Default");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(connString, npg => npg.UseNetTopologySuite())
       .UseSnakeCaseNamingConvention());

// ---- 認證 / 授權 ----
var jwtOpt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOpt.Issuer,
            ValidAudience = jwtOpt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpt.Secret)),
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async ctx =>
            {
                ctx.HandleResponse();
                var expired = ctx.AuthenticateFailure is SecurityTokenExpiredException;
                await WriteAuthError(ctx.Response, 401,
                    expired ? ErrorCodes.TokenExpired : ErrorCodes.Unauthorized,
                    expired ? "權杖已過期" : "未認證或權杖無效");
            },
            OnForbidden = async ctx =>
                await WriteAuthError(ctx.Response, 403, ErrorCodes.Forbidden, "權限不足")
        };
    });
builder.Services.AddAuthorization();

// ---- DI ----
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOAuthService, GoogleOAuthService>();
builder.Services.AddScoped<IOAuthService, LineOAuthService>();
builder.Services.AddSingleton<IObjectStorage, S3ObjectStorage>();
builder.Services.AddScoped<ISpotsService, SpotsService>();
builder.Services.AddScoped<IReviewsService, ReviewsService>();
builder.Services.AddScoped<Maplay.Admin.IAdminSpotsService, Maplay.Admin.AdminSpotsService>();
builder.Services.AddScoped<IImportService, ImportService>();

// ---- Controllers / JSON / 驗證回應 ----
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.Configure<ApiBehaviorOptions>(opt =>
{
    // 將 model binding/驗證錯誤統一為 ApiResponse VALIDATION_ERROR
    opt.InvalidModelStateResponseFactory = ctx =>
    {
        var errors = ctx.ModelState
            .Where(kv => kv.Value!.Errors.Count > 0)
            .ToDictionary(
                kv => ToCamel(kv.Key),
                kv => kv.Value!.Errors.Select(e =>
                    string.IsNullOrEmpty(e.ErrorMessage) ? "輸入無效" : e.ErrorMessage).ToArray());

        var body = ApiResponse.Fail(ErrorCodes.ValidationError, "輸入驗證失敗", new { errors });
        return new ObjectResult(body) { StatusCode = StatusCodes.Status400BadRequest };
    };
});

// ---- OpenAPI 文件配置 (僅開發環境) ----
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenApi();
}

var app = builder.Build();

// ---- 啟動時驗證 Flyway 已完成 schema migration（整合於啟動流程，缺漏即失敗）----
await SchemaGuard.EnsureMigratedAsync(app);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Swagger UI - 添加 JWT 認證支持
    app.MapGet("/swagger", async context =>
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync("""
            <!DOCTYPE html>
            <html>
            <head>
                <title>親子資源地圖系統 API Documentation</title>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <meta name="description" content="親子資源地圖系統 API 文件 - 提供景點查詢、評價管理、使用者認證等功能的互動式 API 測試介面" />
                <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui.css">
                <style>
                    html { box-sizing: border-box; overflow-y: scroll; }
                    *, *:before, *:after { box-sizing: inherit; }
                    body {
                        margin: 0;
                        padding: 0;
                        font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        min-height: 100vh;
                    }
                    .header {
                        background: rgba(255, 255, 255, 0.95);
                        padding: 20px 0;
                        box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                        text-align: center;
                    }
                    .header h1 {
                        margin: 0;
                        color: #333;
                        font-size: 28px;
                        font-weight: 600;
                    }
                    .header p {
                        margin: 8px 0 0;
                        color: #666;
                        font-size: 16px;
                    }
                    .header .badge {
                        display: inline-block;
                        background: #61affe;
                        color: white;
                        padding: 4px 12px;
                        border-radius: 12px;
                        font-size: 12px;
                        margin-left: 8px;
                    }
                    #swagger-ui {
                        max-width: 1460px;
                        margin: 20px auto;
                        padding: 20px;
                        background: white;
                        border-radius: 8px;
                        box-shadow: 0 4px 20px rgba(0,0,0,0.1);
                    }
                    .token-controls {
                        position: fixed;
                        top: 10px;
                        right: 10px;
                        z-index: 9999;
                        background: white;
                        padding: 15px;
                        border: 1px solid #e1e1e1;
                        border-radius: 8px;
                        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                    }
                    .token-controls h4 {
                        margin: 0 0 10px;
                        color: #333;
                        font-size: 14px;
                        font-weight: 600;
                    }
                    .token-controls button {
                        margin: 0 5px;
                        padding: 8px 16px;
                        cursor: pointer;
                        background: #61affe;
                        color: white;
                        border: none;
                        border-radius: 6px;
                        font-size: 14px;
                        transition: all 0.2s;
                    }
                    .token-controls button:hover {
                        background: #4e9afe;
                        transform: translateY(-1px);
                    }
                    .token-controls .clear-btn {
                        background: #ff6b6b;
                    }
                    .token-controls .clear-btn:hover {
                        background: #ee5a5a;
                    }
                    .info {
                        margin: 20px 0;
                        padding: 15px;
                        background: #f8f9fa;
                        border-left: 4px solid #61affe;
                        border-radius: 4px;
                    }
                </style>
            </head>
            <body>
                <div class="header">
                    <h1>🗺️ 親子資源地圖系統 API <span class="badge">v1.0</span></h1>
                    <p>互動式 API 測試介面 - 支援景點查詢、評價管理、使用者認證等功能</p>
                </div>

                <div class="token-controls">
                    <h4>🔐 JWT 認證</h4>
                    <button onclick="setToken()">🔑 設定 Token</button>
                    <button class="clear-btn" onclick="clearToken()">🗑️ 清除</button>
                </div>

                <div class="info">
                    <strong>💡 使用說明：</strong>
                    <ul style="margin: 10px 0; padding-left: 20px;">
                        <li>點擊端點可查看詳細說明和參數</li>
                        <li>使用「Try it out」按鈕測試 API</li>
                        <li>需要認證的端點請先設定 JWT Token</li>
                        <li>開發環境僅供測試使用，生產環境已停用</li>
                    </ul>
                </div>

                <div id="swagger-ui"></div>

                <script src="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui-bundle.js" crossorigin="anonymous"></script>
                <script src="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui-standalone-preset.js" crossorigin="anonymous"></script>
                <script>
                    function setToken() {
                        const token = prompt('請輸入您的 JWT Token (不包含 Bearer 前綴):');
                        if (token) {
                            localStorage.setItem('swagger_jwt_token', token);
                            alert('✅ Token 已設定！重新載入頁面後生效。');
                            location.reload();
                        }
                    }

                    function clearToken() {
                        localStorage.removeItem('swagger_jwt_token');
                        alert('🗑️ Token 已清除！重新載入頁面後生效。');
                        location.reload();
                    }

                    window.onload = function() {
                        const token = localStorage.getItem('swagger_jwt_token');
                        SwaggerUIBundle({
                            url: '/openapi/v1.json',
                            dom_id: '#swagger-ui',
                            deepLinking: true,
                            presets: [SwaggerUIBundle.presets.apis, SwaggerUIBundle.StandalonePreset],
                            plugins: [SwaggerUIBundle.plugins.DownloadUrl],
                            layout: "BaseLayout",
                            defaultModelsExpandDepth: 1,
                            defaultModelExpandDepth: 1,
                            tryItOutEnabled: true,
                            persistAuthorization: true,
                            requestInterceptor: (request) => {
                                if (token && request.url.includes('/api/')) {
                                    request.headers.Authorization = 'Bearer ' + token;
                                }
                                return request;
                            },
                            responseInterceptor: (response) => {
                                return response;
                            },
                            validatorUrl: null,
                            docExpansion: 'list',
                            filter: true,
                            showRequestDuration: true,
                            displayOperationId: false,
                            displayRequestDuration: true
                        });

                        // 自訂標題
                        setTimeout(() => {
                            const titleElement = document.querySelector('.topbar');
                            if (titleElement) {
                                titleElement.style.display = 'none';
                            }
                        }, 100);
                    };
                </script>
            </body>
            </html>
            """);
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// ---- 區域函式 ----

static string ToCamel(string s) =>
    string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s[1..];

static async Task WriteAuthError(HttpResponse resp, int status, string code, string message)
{
    if (resp.HasStarted) return;
    resp.StatusCode = status;
    resp.ContentType = "application/json; charset=utf-8";
    await resp.WriteAsync(JsonSerializer.Serialize(ApiResponse.Fail(code, message), Maplay.Common.JsonOptions.Default));
}

/// <summary>啟動時確認 Flyway schema_history 至少完成預期 migration，否則中止啟動。</summary>
static class SchemaGuard
{
    private const int ExpectedMigrations = 7;

    public static async Task EnsureMigratedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SchemaGuard");

        try
        {
            var applied = await db.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*)::int AS \"Value\" FROM flyway_schema_history WHERE success = true")
                .FirstOrDefaultAsync();

            if (applied < ExpectedMigrations)
                throw new InvalidOperationException(
                    $"Flyway migration 未完成（已套用 {applied}，需 ≥ {ExpectedMigrations}）。請先執行 flyway migrate。");

            logger.LogInformation("Schema 檢查通過：已套用 {Applied} 筆 Flyway migration", applied);
        }
        catch (Exception ex) when (ex is Npgsql.PostgresException or Npgsql.NpgsqlException)
        {
            throw new InvalidOperationException(
                "無法讀取 flyway_schema_history，資料庫尚未由 Flyway 初始化。", ex);
        }
    }
}
