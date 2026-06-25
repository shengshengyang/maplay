namespace Maplay.Common;

/// <summary>統一回應外殼。成功：{ success:true, data }；失敗：{ success:false, errorCode, message, data }。</summary>
public class ApiResponse
{
    public bool Success { get; init; }
    public string? ErrorCode { get; init; }
    public string? Message { get; init; }
    public object? Data { get; init; }

    public static ApiResponse Ok(object? data = null) => new() { Success = true, Data = data };

    public static ApiResponse Fail(string errorCode, string message, object? data = null) =>
        new() { Success = false, ErrorCode = errorCode, Message = message, Data = data };
}
