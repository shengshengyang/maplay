using System.ComponentModel.DataAnnotations;

namespace Maplay.Admin;

public record PendingSpotDto(
    Guid Id,
    string Name,
    string Category,
    string SpotType,
    double Lat,
    double Lng,
    Guid? SubmittedBy,
    string? ReporterName,
    DateTime CreatedAt);

public class RejectRequest
{
    [Required(AllowEmptyStrings = false)]
    public string RejectReason { get; set; } = string.Empty;
}

public record AdminUserDto(
    Guid Id,
    string? Email,
    string DisplayName,
    string Role,
    string Provider,
    DateTime CreatedAt);
