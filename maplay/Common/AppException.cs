namespace Maplay.Common;

/// <summary>業務例外，由中介層統一轉為 ApiResponse 失敗回應。</summary>
public class AppException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public object? Data { get; }

    public AppException(int statusCode, string errorCode, string message, object? data = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Data = data;
    }

    public static AppException BadRequest(string errorCode, string message, object? data = null)
        => new(400, errorCode, message, data);

    public static AppException Unauthorized(string message = "未認證或權杖無效", string errorCode = ErrorCodes.Unauthorized)
        => new(401, errorCode, message);

    public static AppException Forbidden(string message = "權限不足")
        => new(403, ErrorCodes.Forbidden, message);

    public static AppException NotFound(string errorCode, string message)
        => new(404, errorCode, message);

    public static AppException Conflict(string errorCode, string message)
        => new(409, errorCode, message);

    public static AppException Unprocessable(string errorCode, string message, object? data = null)
        => new(422, errorCode, message, data);

    /// <summary>欄位驗證錯誤（400 VALIDATION_ERROR，data.errors 為欄位→訊息）。</summary>
    public static AppException Validation(IDictionary<string, string[]> errors, string message = "輸入驗證失敗")
        => new(400, ErrorCodes.ValidationError, message, new { errors });

    public static AppException Validation(string field, string error)
        => Validation(new Dictionary<string, string[]> { [field] = new[] { error } });
}
