using NetTopologySuite.Geometries;

namespace Maplay.Data.Entities;

public class Spot
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = SpotStatus.Pending;
    public string SpotType { get; set; } = SpotTypes.Permanent;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string[] AgeGroups { get; set; } = Array.Empty<string>();
    public string[] Facilities { get; set; } = Array.Empty<string>();
    public Point Location { get; set; } = default!;
    public string? Address { get; set; }
    public Guid? SubmittedBy { get; set; }
    public string Source { get; set; } = SpotSources.User;
    public string? GovDataId { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectReason { get; set; }
    public string? SourceUrl { get; set; }
    public DateTime? CrawledAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<SpotImage> Images { get; set; } = new List<SpotImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}

public static class SpotStatus
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
}

public static class SpotTypes
{
    public const string Permanent = "permanent";
    public const string Temporary = "temporary";
}

public static class SpotSources
{
    public const string User = "user";
    public const string GovImport = "gov_import";
}

public static class SpotCategories
{
    public static readonly HashSet<string> All = new()
    {
        "park", "restaurant", "nursing_room", "toilet", "activity", "medical"
    };
}

public static class AgeGroupValues
{
    public static readonly HashSet<string> All = new() { "0-3", "3-7", "7-12", "12+" };
}

public static class FacilityValues
{
    public static readonly HashSet<string> All = new()
    {
        "diaper_table", "nursing_room", "stroller_accessible", "kids_toilet",
        "high_chair", "parking", "elevator", "play_area"
    };
}
