using Maplay.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maplay.Spots;

[ApiController]
[Route("api/spots")]
public class SpotsController : ControllerBase
{
    private readonly ISpotsService _spots;
    private readonly ICurrentUser _current;

    public SpotsController(ISpotsService spots, ICurrentUser current)
    {
        _spots = spots;
        _current = current;
    }

    [AllowAnonymous]
    [HttpGet("nearby")]
    public async Task<IActionResult> Nearby([FromQuery] NearbyQuery q, CancellationToken ct)
    {
        var items = await _spots.NearbyAsync(q, ct);
        return Ok(ApiResponse.Ok(items));
    }

    [AllowAnonymous]
    [HttpGet("active-temp")]
    public async Task<IActionResult> ActiveTemp([FromQuery] PageQuery page, CancellationToken ct)
    {
        var result = await _spots.ActiveTempAsync(page, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
    {
        var detail = await _spots.GetDetailAsync(id, _current, ct);
        return Ok(ApiResponse.Ok(detail));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSpotRequest req, CancellationToken ct)
    {
        var spot = await _spots.CreateAsync(req, _current.RequireUserId(), ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse.Ok(spot));
    }

    [Authorize]
    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<IActionResult> UploadImages(Guid id, [FromForm] List<IFormFile> files, CancellationToken ct)
    {
        var result = await _spots.UploadImagesAsync(id, files, _current.RequireUserId(), ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse.Ok(result));
    }
}
