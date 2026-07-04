namespace Maplay.Common;

/// <summary>統一 API 回應格式</summary>
/// <remarks>成功回應：{ success:true, data }；失敗回應：{ success:false, errorCode, message, data }</remarks>
public class ApiResponse
{
    /// <summary>請求是否成功</summary>
    public bool Success { get; init; }
    /// <summary>錯誤代碼 (失敗時)</summary>
    public string? ErrorCode { get; init; }
    /// <summary>錯誤或訊息描述</summary>
    public string? Message { get; init; }
    /// <summary>回應資料</summary>
    public object? Data { get; init; }

    /// <summary>建立成功回應</summary>
    /// <param name="data">回應資料</param>
    /// <returns>成功的 ApiResponse</returns>
    public static ApiResponse Ok(object? data = null) => new() { Success = true, Data = data };

    /// <summary>建立失敗回應</summary>
    /// <param name="errorCode">錯誤代碼</param>
    /// <param name="message">錯誤訊息</param>
    /// <param name="data">額外資料</param>
    /// <returns>失敗的 ApiResponse</returns>
    public static ApiResponse Fail(string errorCode, string message, object? data = null) =>
        new() { Success = false, ErrorCode = errorCode, Message = message, Data = data };
}
