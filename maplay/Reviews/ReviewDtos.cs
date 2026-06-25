using System.ComponentModel.DataAnnotations;

namespace Maplay.Reviews;

public class CreateReviewRequest
{
    [Range(1, 5)] public int Rating { get; set; }
    [Range(1, 5)] public int CleanLevel { get; set; }
    public string? Content { get; set; }
    public DateOnly? VisitedAt { get; set; }
}

public class UpdateReviewRequest
{
    [Range(1, 5)] public int Rating { get; set; }
    [Range(1, 5)] public int CleanLevel { get; set; }
    public string? Content { get; set; }
    public DateOnly? VisitedAt { get; set; }
}

public record ReviewDto(
    Guid Id, Guid SpotId, Guid UserId, short Rating, short CleanLevel,
    string? Content, DateOnly? VisitedAt, DateTime CreatedAt, DateTime UpdatedAt);
