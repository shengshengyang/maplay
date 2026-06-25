namespace Maplay.Data.Entities;

public class SpotImage
{
    public Guid Id { get; set; }
    public Guid SpotId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public bool IsCover { get; set; }
    public Guid? UploadedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public Spot? Spot { get; set; }
}
