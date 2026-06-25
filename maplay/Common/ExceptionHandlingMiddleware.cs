using System.Text.Json;

namespace Maplay.Common;

/// <summary>集中例外處理：將 AppException 與未預期例外轉為統一 ApiResponse 失敗格式。</summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (AppException ex)
        {
            await WriteAsync(ctx, ex.StatusCode, ApiResponse.Fail(ex.ErrorCode, ex.Message, ex.Data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "未處理的例外");
            await WriteAsync(ctx, 500, ApiResponse.Fail(ErrorCodes.InternalError, "伺服器內部錯誤"));
        }
    }

    private static async Task WriteAsync(HttpContext ctx, int status, ApiResponse body)
    {
        if (ctx.Response.HasStarted) return;
        ctx.Response.Clear();
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions.Default));
    }
}

/// <summary>全域 JSON 設定：camelCase。</summary>
public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
}
