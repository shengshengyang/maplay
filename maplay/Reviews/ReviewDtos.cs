using System.ComponentModel.DataAnnotations;

namespace Maplay.Reviews;

/// <summary>建立評價請求</summary>
public class CreateReviewRequest
{
    /// <summary>評分 (1-5)</summary>
    [Range(1, 5)] public int Rating { get; set; }
    /// <summary>整潔度評分 (1-5)</summary>
    [Range(1, 5)] public int CleanLevel { get; set; }
    /// <summary>評價內容</summary>
    public string? Content { get; set; }
    /// <summary>拜訪日期</summary>
    public DateOnly? VisitedAt { get; set; }
}

/// <summary>更新評價請求</summary>
public class UpdateReviewRequest
{
    /// <summary>評分 (1-5)</summary>
    [Range(1, 5)] public int Rating { get; set; }
    /// <summary>整潔度評分 (1-5)</summary>
    [Range(1, 5)] public int CleanLevel { get; set; }
    /// <summary>評價內容</summary>
    public string? Content { get; set; }
    /// <summary>拜訪日期</summary>
    public DateOnly? VisitedAt { get; set; }
}

/// <summary>評價資訊</summary>
public record ReviewDto(
    /// <summary>評價 ID</summary>
    Guid Id,
    /// <summary>景點 ID</summary>
    Guid SpotId,
    /// <summary>使用者 ID</summary>
    Guid UserId,
    /// <summary>評分 (1-5)</summary>
    short Rating,
    /// <summary>整潔度評分 (1-5)</summary>
    short CleanLevel,
    /// <summary>評價內容</summary>
    string? Content,
    /// <summary>拜訪日期</summary>
    DateOnly? VisitedAt,
    /// <summary>建立時間</summary>
    DateTime CreatedAt,
    /// <summary>更新時間</summary>
    DateTime UpdatedAt);
