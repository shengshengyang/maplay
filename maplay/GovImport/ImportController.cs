using System.Text.Json;
using Maplay.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maplay.GovImport;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin/import")]
public class ImportController : ControllerBase
{
    private readonly IImportService _import;
    private readonly ICurrentUser _current;

    public ImportController(IImportService import, ICurrentUser current)
    {
        _import = import;
        _current = current;
    }

    [HttpPost("toilet")]
    public async Task<IActionResult> Toilet([FromBody] JsonElement payload, CancellationToken ct)
    {
        var result = await _import.ImportToiletAsync(payload, _current.RequireUserId(), ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost("park")]
    public async Task<IActionResult> Park([FromBody] JsonElement geojson, CancellationToken ct)
    {
        var result = await _import.ImportParkAsync(geojson, _current.RequireUserId(), ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost("custom")]
    public async Task<IActionResult> Custom([FromBody] CustomImportRequest req, CancellationToken ct)
    {
        var result = await _import.ImportCustomAsync(req, _current.RequireUserId(), ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("history")]
    public async Task<IActionResult> History([FromQuery] PageQuery page, CancellationToken ct)
        => Ok(ApiResponse.Ok(await _import.HistoryAsync(page, ct)));
}
