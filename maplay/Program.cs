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

    // 使用 Scalar UI 作為 API 文件介面
    app.MapGet("/scalar/{version}", async context =>
    {
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync("""
            <!DOCTYPE html>
            <html>
            <head>
                <title>親子資源地圖系統 API</title>
                <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@scalar/dist/latest/style.css">
            </head>
            <body>
                <script id="api-reference" data-url="/openapi/v1.json"></script>
                <script src="https://cdn.jsdelivr.net/npm/@scalar/dist/latest/browser.js"></script>
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
