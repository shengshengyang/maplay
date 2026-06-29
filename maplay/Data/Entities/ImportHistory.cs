namespace Maplay.Data.Entities;

public class ImportHistory
{
    public Guid Id { get; set; }
    public string Dataset { get; set; } = string.Empty;
    public Guid? OperatedBy { get; set; }
    public int Total { get; set; }
    public int Success { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public DateTime CreatedAt { get; set; }
}
