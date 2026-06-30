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
                <title>親子資源地圖系統 API - Swagger UI</title>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui.css">
                <style>
                    html { box-sizing: border-box; overflow-y: scroll; }
                    *, *:before, *:after { box-sizing: inherit; }
                    body { margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; }
                    #swagger-ui { max-width: 1460px; margin: 0 auto; padding: 20px; }
                    .token-controls {
                        position: fixed;
                        top: 10px;
                        right: 10px;
                        z-index: 9999;
                        background: white;
                        padding: 10px;
                        border: 1px solid #ccc;
                        border-radius: 4px;
                        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                    }
                    .token-controls button {
                        margin: 0 5px;
                        padding: 8px 15px;
                        cursor: pointer;
                        background: #61affe;
                        color: white;
                        border: none;
                        border-radius: 3px;
                        font-size: 14px;
                    }
                    .token-controls button:hover {
                        background: #4e9afe;
                    }
                    .token-controls .clear-btn {
                        background: #ff6b6b;
                    }
                    .token-controls .clear-btn:hover {
                        background: #ee5a5a;
                    }
                </style>
            </head>
            <body>
                <div class="token-controls">
                    <button onclick="setToken()">🔑 設定 Token</button>
                    <button class="clear-btn" onclick="clearToken()">🗑️ 清除 Token</button>
                </div>
                <div id="swagger-ui"></div>
                <script src="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui-bundle.js" crossorigin="anonymous"></script>
                <script src="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui-standalone-preset.js" crossorigin="anonymous"></script>
                <script>
                    function setToken() {
                        const token = prompt('請輸入您的 JWT Token:');
                        if (token) {
                            localStorage.setItem('swagger_jwt_token', token);
                            alert('Token 已設定！重新載入頁面後生效。');
                            location.reload();
                        }
                    }

                    function clearToken() {
                        localStorage.removeItem('swagger_jwt_token');
                        alert('Token 已清除！重新載入頁面後生效。');
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
                            docExpansion: 'list'
                        });
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
