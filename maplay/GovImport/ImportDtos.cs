using System.Text.Json;

namespace Maplay.GovImport;

/// <summary>匯入結果統計資訊</summary>
public record ImportResultDto(
    /// <summary>資料集名稱</summary>
    string Dataset,
    /// <summary>總筆數</summary>
    int Total,
    /// <summary>成功筆數</summary>
    int Success,
    /// <summary>跳過筆數</summary>
    int Skipped,
    /// <summary>失敗筆數</summary>
    int Failed);

/// <summary>自訂欄位對應匯入請求</summary>
/// <remarks>包含記錄列表與來源欄位→系統欄位對應表</remarks>
public class CustomImportRequest
{
    /// <summary>資料記錄列表 (JSON 格式)</summary>
    public List<JsonElement> Records { get; set; } = new();
    /// <summary>欄位對應表 (key=系統欄位名稱, value=來源欄位名稱)</summary>
    /// <remarks>系統欄位包含：name, lat, lng, category, address, govDataId 等</remarks>
    public Dictionary<string, string> Mapping { get; set; } = new();
    /// <summary>預設景點類別</summary>
    public string? DefaultCategory { get; set; }
}

/// <summary>匯入歷史記錄資訊</summary>
public record ImportHistoryDto(
    /// <summary>記錄 ID</summary>
    Guid Id,
    /// <summary>資料集名稱</summary>
    string Dataset,
    /// <summary>操作者 ID</summary>
    Guid? OperatedBy,
    /// <summary>總筆數</summary>
    int Total,
    /// <summary>成功筆數</summary>
    int Success,
    /// <summary>跳過筆數</summary>
    int Skipped,
    /// <summary>失敗筆數</summary>
    int Failed,
    /// <summary>建立時間</summary>
    DateTime CreatedAt);
