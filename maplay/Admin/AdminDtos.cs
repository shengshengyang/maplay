using System.ComponentModel.DataAnnotations;

namespace Maplay.Admin;

/// <summary>待審核景點資訊</summary>
public record PendingSpotDto(
    /// <summary>景點 ID</summary>
    Guid Id,
    /// <summary>景點名稱</summary>
    string Name,
    /// <summary>景點類別</summary>
    string Category,
    /// <summary>景點類型</summary>
    string SpotType,
    /// <summary>緯度</summary>
    double Lat,
    /// <summary>經度</summary>
    double Lng,
    /// <summary>提交者 ID</summary>
    Guid? SubmittedBy,
    /// <summary>回報者姓名</summary>
    string? ReporterName,
    /// <summary>建立時間</summary>
    DateTime CreatedAt);

/// <summary>拒絕景點請求</summary>
public class RejectRequest
{
    /// <summary>拒絕原因 (必填，不可為空字串)</summary>
    [Required(AllowEmptyStrings = false)]
    public string RejectReason { get; set; } = string.Empty;
}

/// <summary>管理員使用者資訊</summary>
public record AdminUserDto(
    /// <summary>使用者 ID</summary>
    Guid Id,
    /// <summary>電子郵件</summary>
    string? Email,
    /// <summary>顯示名稱</summary>
    string DisplayName,
    /// <summary>角色</summary>
    string Role,
    /// <summary>認證提供者</summary>
    string Provider,
    /// <summary>建立時間</summary>
    DateTime CreatedAt);
