using Maplay.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maplay.Spots;

/// <summary>景點管理 API 控制器</summary>
[ApiController]
[Route("api/spots")]
[Tags("Spot Management")]
public class SpotsController : ControllerBase
{
    private readonly ISpotsService _spots;
    private readonly ICurrentUser _current;

    public SpotsController(ISpotsService spots, ICurrentUser current)
    {
        _spots = spots;
        _current = current;
    }

    /// <summary>查詢附近的景點</summary>
    /// <param name="q">附近查詢參數</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回附近景點列表 (HTTP 200)</returns>
    [AllowAnonymous]
    [HttpGet("nearby")]
    public async Task<IActionResult> Nearby([FromQuery] NearbyQuery q, CancellationToken ct)
    {
        var items = await _spots.NearbyAsync(q, ct);
        return Ok(ApiResponse.Ok(items));
    }

    /// <summary>查詢當前活躍的景點 (臨時列表)</summary>
    /// <param name="page">分頁查詢參數</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回活躍景點分頁列表 (HTTP 200)</returns>
    [AllowAnonymous]
    [HttpGet("active-temp")]
    public async Task<IActionResult> ActiveTemp([FromQuery] PageQuery page, CancellationToken ct)
    {
        var result = await _spots.ActiveTempAsync(page, ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>取得景點詳細資訊</summary>
    /// <param name="id">景點 ID</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回景點詳細資訊 (HTTP 200)，景點不存在時返回 404</returns>
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
    {
        var detail = await _spots.GetDetailAsync(id, _current, ct);
        return Ok(ApiResponse.Ok(detail));
    }

    /// <summary>建立新景點</summary>
    /// <param name="req">建立景點請求資料</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回新建景點資訊 (HTTP 201)，未認證時返回 401</returns>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSpotRequest req, CancellationToken ct)
    {
        var spot = await _spots.CreateAsync(req, _current.RequireUserId(), ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse.Ok(spot));
    }

    /// <summary>上傳景點圖片</summary>
    /// <param name="id">景點 ID</param>
    /// <param name="files">圖片檔案列表 (最大 30MB)</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回上傳的圖片資訊 (HTTP 201)，未認證時返回 401</returns>
    [Authorize]
    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<IActionResult> UploadImages(Guid id, [FromForm] List<IFormFile> files, CancellationToken ct)
    {
        var result = await _spots.UploadImagesAsync(id, files, _current.RequireUserId(), ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse.Ok(result));
    }
}
