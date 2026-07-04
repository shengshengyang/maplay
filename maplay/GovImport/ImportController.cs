using System.Text.Json;
using Maplay.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maplay.GovImport;

/// <summary>政府資料匯入 API 控制器</summary>
[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin/import")]
[Tags("Admin Moderation")]
public class ImportController : ControllerBase
{
    private readonly IImportService _import;
    private readonly ICurrentUser _current;

    public ImportController(IImportService import, ICurrentUser current)
    {
        _import = import;
        _current = current;
    }

    /// <summary>匯入廁所資料</summary>
    /// <param name="payload">政府開放資料 JSON 格式</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回匯入結果 (HTTP 200)，僅限管理員存取</returns>
    [HttpPost("toilet")]
    public async Task<IActionResult> Toilet([FromBody] JsonElement payload, CancellationToken ct)
    {
        var result = await _import.ImportToiletAsync(payload, _current.RequireUserId(), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>匯入公園資料</summary>
    /// <param name="geojson">公園 GeoJSON 格式資料</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回匯入結果 (HTTP 200)，僅限管理員存取</returns>
    [HttpPost("park")]
    public async Task<IActionResult> Park([FromBody] JsonElement geojson, CancellationToken ct)
    {
        var result = await _import.ImportParkAsync(geojson, _current.RequireUserId(), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>匯入自訂資料</summary>
    /// <param name="req">自訂匯入請求資料</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回匯入結果 (HTTP 200)，僅限管理員存取</returns>
    [HttpPost("custom")]
    public async Task<IActionResult> Custom([FromBody] CustomImportRequest req, CancellationToken ct)
    {
        var result = await _import.ImportCustomAsync(req, _current.RequireUserId(), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>查詢匯入歷史記錄</summary>
    /// <param name="page">分頁查詢參數</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回匯入歷史分頁列表 (HTTP 200)，僅限管理員存取</returns>
    [HttpGet("history")]
    public async Task<IActionResult> History([FromQuery] PageQuery page, CancellationToken ct)
        => Ok(ApiResponse.Ok(await _import.HistoryAsync(page, ct)));
}
