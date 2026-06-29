using System.ComponentModel.DataAnnotations;

namespace Maplay.Spots;

public class NearbyQuery
{
    [Required] public double? Lat { get; set; }
    [Required] public double? Lng { get; set; }
    public int Radius { get; set; } = 2000;
    public string? Category { get; set; }
    public string? Age { get; set; }
    public string? SpotType { get; set; }
}

public record NearbySpotDto(
    Guid Id,
    string Name,
    string Category,
    string SpotType,
    double Lat,
    double Lng,
    double DistanceMeters,
    string[] AgeGroups,
    string[] Facilities,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? CoverUrl);

public record SpotImageDto(Guid Id, string Url, bool IsCover);

public record ReviewStatsDto(double AvgRating, double AvgCleanLevel, int ReviewCount);

public record ReviewBriefDto(
    Guid Id, Guid UserId, string DisplayName, short Rating, short CleanLevel,
    string? Content, DateOnly? VisitedAt, DateTime CreatedAt);

public record SpotDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string Category,
    string Status,
    string SpotType,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string[] AgeGroups,
    string[] Facilities,
    double Lat,
    double Lng,
    string? Address,
    string Source,
    IReadOnlyList<SpotImageDto> Images,
    ReviewStatsDto Stats,
    IReadOnlyList<ReviewBriefDto> RecentReviews,
    DateTime CreatedAt);

public record SpotSummaryDto(
    Guid Id, string Name, string Category, string Status, string SpotType,
    double Lat, double Lng, DateOnly? StartDate, DateOnly? EndDate, DateTime CreatedAt);

public class CreateSpotRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required] public string Category { get; set; } = string.Empty;
    public string SpotType { get; set; } = "permanent";
    [Required] public double? Lat { get; set; }
    [Required] public double? Lng { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string[] AgeGroups { get; set; } = Array.Empty<string>();
    public string[] Facilities { get; set; } = Array.Empty<string>();
    public string? Address { get; set; }
}

public class UpdateSpotRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? SpotType { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string[]? AgeGroups { get; set; }
    public string[]? Facilities { get; set; }
    public string? Address { get; set; }
}
