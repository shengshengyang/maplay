using System.ComponentModel.DataAnnotations;

namespace Maplay.Spots;

/// <summary>��近景點查詢參數</summary>
public class NearbyQuery
{
    /// <summary>緯度</summary>
    [Required] public double? Lat { get; set; }
    /// <summary>經度</summary>
    [Required] public double? Lng { get; set; }
    /// <summary>搜尋半徑 (公尺，預設 2000)</summary>
    public int Radius { get; set; } = 2000;
    /// <summary>景點類別篩選</summary>
    public string? Category { get; set; }
    /// <summary>適用年齡篩選</summary>
    public string? Age { get; set; }
    /// <summary>景點類型篩選</summary>
    public string? SpotType { get; set; }
}

/// <summary>附近景點資訊</summary>
public record NearbySpotDto(
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
    /// <summary>距離 (公尺)</summary>
    double DistanceMeters,
    /// <summary>適用年齡群組</summary>
    string[] AgeGroups,
    /// <summary>設施列表</summary>
    string[] Facilities,
    /// <summary>開始日期</summary>
    DateOnly? StartDate,
    /// <summary>結束日期</summary>
    DateOnly? EndDate,
    /// <summary>封面圖片 URL</summary>
    string? CoverUrl);

/// <summary>景點圖片資訊</summary>
public record SpotImageDto(
    /// <summary>圖片 ID</summary>
    Guid Id,
    /// <summary>圖片 URL</summary>
    string Url,
    /// <summary>是否為封面圖</summary>
    bool IsCover);

/// <summary>評價統計資訊</summary>
public record ReviewStatsDto(
    /// <summary>平均評分</summary>
    double AvgRating,
    /// <summary>平均整潔度評分</summary>
    double AvgCleanLevel,
    /// <summary>評價數量</summary>
    int ReviewCount);

/// <summary>評價簡要資訊</summary>
public record ReviewBriefDto(
    /// <summary>評價 ID</summary>
    Guid Id,
    /// <summary>使用者 ID</summary>
    Guid UserId,
    /// <summary>使用者顯示名稱</summary>
    string DisplayName,
    /// <summary>評分 (1-5)</summary>
    short Rating,
    /// <summary>整潔度評分 (1-5)</summary>
    short CleanLevel,
    /// <summary>評價內容</summary>
    string? Content,
    /// <summary>拜訪日期</summary>
    DateOnly? VisitedAt,
    /// <summary>建立時間</summary>
    DateTime CreatedAt);

/// <summary>景點詳細資訊</summary>
public record SpotDetailDto(
    /// <summary>景點 ID</summary>
    Guid Id,
    /// <summary>景點名稱</summary>
    string Name,
    /// <summary>景點描述</summary>
    string? Description,
    /// <summary>景點類別</summary>
    string Category,
    /// <summary>景點狀態</summary>
    string Status,
    /// <summary>景點類型</summary>
    string SpotType,
    /// <summary>開始日期</summary>
    DateOnly? StartDate,
    /// <summary>結束日期</summary>
    DateOnly? EndDate,
    /// <summary>適用年齡群組</summary>
    string[] AgeGroups,
    /// <summary>設施列表</summary>
    string[] Facilities,
    /// <summary>緯度</summary>
    double Lat,
    /// <summary>經度</summary>
    double Lng,
    /// <summary>地址</summary>
    string? Address,
    /// <summary>資料來源</summary>
    string Source,
    /// <summary>圖片列表</summary>
    IReadOnlyList<SpotImageDto> Images,
    /// <summary>評價統計</summary>
    ReviewStatsDto Stats,
    /// <summary>最近評價列表</summary>
    IReadOnlyList<ReviewBriefDto> RecentReviews,
    /// <summary>建立時間</summary>
    DateTime CreatedAt);

/// <summary>景點摘要資訊</summary>
public record SpotSummaryDto(
    /// <summary>景點 ID</summary>
    Guid Id,
    /// <summary>景點名稱</summary>
    string Name,
    /// <summary>景點類別</summary>
    string Category,
    /// <summary>景點狀態</summary>
    string Status,
    /// <summary>景點類型</summary>
    string SpotType,
    /// <summary>緯度</summary>
    double Lat,
    /// <summary>經度</summary>
    double Lng,
    /// <summary>開始日期</summary>
    DateOnly? StartDate,
    /// <summary>結束日期</summary>
    DateOnly? EndDate,
    /// <summary>建立時間</summary>
    DateTime CreatedAt);

/// <summary>建立景點請求</summary>
public class CreateSpotRequest
{
    /// <summary>景點名稱 (1-200 字元)</summary>
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    /// <summary>景點描述</summary>
    public string? Description { get; set; }
    /// <summary>景點類別</summary>
    [Required] public string Category { get; set; } = string.Empty;
    /// <summary>景點類型 (預設: permanent)</summary>
    public string SpotType { get; set; } = "permanent";
    /// <summary>緯度</summary>
    [Required] public double? Lat { get; set; }
    /// <summary>經度</summary>
    [Required] public double? Lng { get; set; }
    /// <summary>開始日期</summary>
    public DateOnly? StartDate { get; set; }
    /// <summary>結束日期</summary>
    public DateOnly? EndDate { get; set; }
    /// <summary>適用年齡群組</summary>
    public string[] AgeGroups { get; set; } = Array.Empty<string>();
    /// <summary>設施列表</summary>
    public string[] Facilities { get; set; } = Array.Empty<string>();
    /// <summary>地址</summary>
    public string? Address { get; set; }
}

/// <summary>更新景點請求</summary>
public class UpdateSpotRequest
{
    /// <summary>景點名稱 (選填)</summary>
    public string? Name { get; set; }
    /// <summary>景點描述 (選填)</summary>
    public string? Description { get; set; }
    /// <summary>景點類別 (選填)</summary>
    public string? Category { get; set; }
    /// <summary>景點類型 (選填)</summary>
    public string? SpotType { get; set; }
    /// <summary>緯度 (選填)</summary>
    public double? Lat { get; set; }
    /// <summary>經度 (選填)</summary>
    public double? Lng { get; set; }
    /// <summary>開始日期 (選填)</summary>
    public DateOnly? StartDate { get; set; }
    /// <summary>結束日期 (選填)</summary>
    public DateOnly? EndDate { get; set; }
    /// <summary>適用年齡群組 (選填)</summary>
    public string[]? AgeGroups { get; set; }
    /// <summary>設施列表 (選填)</summary>
    public string[]? Facilities { get; set; }
    /// <summary>地址 (選填)</summary>
    public string? Address { get; set; }
}
