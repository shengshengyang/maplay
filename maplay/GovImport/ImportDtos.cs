using System.Text.Json;

namespace Maplay.GovImport;

/// <summary>匯入結果統計。</summary>
public record ImportResultDto(string Dataset, int Total, int Success, int Skipped, int Failed);

/// <summary>自訂欄位對應匯入請求：records + 來源欄位→系統欄位對應表。</summary>
public class CustomImportRequest
{
    public List<JsonElement> Records { get; set; } = new();
    /// <summary>key=系統欄位(name/lat/lng/category/address/govDataId)，value=來源欄位名稱。</summary>
    public Dictionary<string, string> Mapping { get; set; } = new();
    public string? DefaultCategory { get; set; }
}

public record ImportHistoryDto(
    Guid Id, string Dataset, Guid? OperatedBy, int Total, int Success, int Skipped, int Failed, DateTime CreatedAt);
