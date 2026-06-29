namespace Maplay.Data.Entities;

public class Review
{
    public Guid Id { get; set; }
    public Guid SpotId { get; set; }
    public Guid UserId { get; set; }
    public short Rating { get; set; }
    public short CleanLevel { get; set; }
    public string? Content { get; set; }
    public DateOnly? VisitedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Spot? Spot { get; set; }
    public User? User { get; set; }
}
