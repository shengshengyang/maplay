using Maplay.Common;
using Maplay.Spots;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maplay.Admin;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminSpotsService _admin;
    private readonly ICurrentUser _current;

    public AdminController(IAdminSpotsService admin, ICurrentUser current)
    {
        _admin = admin;
        _current = current;
    }

    [HttpGet("spots/pending")]
    public async Task<IActionResult> Pending([FromQuery] PageQuery page, CancellationToken ct)
        => Ok(ApiResponse.Ok(await _admin.PendingAsync(page, ct)));

    [HttpPut("spots/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        await _admin.ApproveAsync(id, _current.RequireUserId(), ct);
        return Ok(ApiResponse.Ok(new { id, status = "approved" }));
    }

    [HttpPut("spots/{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest req, CancellationToken ct)
    {
        await _admin.RejectAsync(id, req.RejectReason, _current.RequireUserId(), ct);
        return Ok(ApiResponse.Ok(new { id, status = "rejected" }));
    }

    [HttpPut("spots/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, [FromBody] UpdateSpotRequest req, CancellationToken ct)
    {
        await _admin.EditAsync(id, req, ct);
        return Ok(ApiResponse.Ok(new { id, updated = true }));
    }

    [HttpDelete("spots/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _admin.SoftDeleteAsync(id, ct);
        return Ok(ApiResponse.Ok(new { id, deleted = true }));
    }

    [HttpPost("spots/{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        await _admin.RestoreAsync(id, ct);
        return Ok(ApiResponse.Ok(new { id, restored = true }));
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users([FromQuery] PageQuery page, [FromQuery] string? search, CancellationToken ct)
        => Ok(ApiResponse.Ok(await _admin.ListUsersAsync(page, search, ct)));
}
