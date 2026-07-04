using Maplay.Common;
using Maplay.Spots;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maplay.Admin;

/// <summary>管理功能 API 控制器</summary>
[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin")]
[Tags("Admin Moderation")]
public class AdminController : ControllerBase
{
    private readonly IAdminSpotsService _admin;
    private readonly ICurrentUser _current;

    public AdminController(IAdminSpotsService admin, ICurrentUser current)
    {
        _admin = admin;
        _current = current;
    }

    /// <summary>查詢待審核的景點列表</summary>
    /// <param name="page">分頁查詢參數</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回待審核景點分頁列表 (HTTP 200)，僅限管理員存取</returns>
    [HttpGet("spots/pending")]
    public async Task<IActionResult> Pending([FromQuery] PageQuery page, CancellationToken ct)
        => Ok(ApiResponse.Ok(await _admin.PendingAsync(page, ct)));

    /// <summary>核准景點</summary>
    /// <param name="id">景點 ID</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回核准狀態 (HTTP 200)，僅限管理員存取</returns>
    [HttpPut("spots/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        await _admin.ApproveAsync(id, _current.RequireUserId(), ct);
        return Ok(ApiResponse.Ok(new { id, status = "approved" }));
    }

    /// <summary>拒絕景點</summary>
    /// <param name="id">景點 ID</param>
    /// <param name="req">拒絕請求資料</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回拒絕狀態 (HTTP 200)，僅限管理員存取</returns>
    [HttpPut("spots/{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest req, CancellationToken ct)
    {
        await _admin.RejectAsync(id, req.RejectReason, _current.RequireUserId(), ct);
        return Ok(ApiResponse.Ok(new { id, status = "rejected" }));
    }

    /// <summary>編輯景點資訊</summary>
    /// <param name="id">景點 ID</param>
    /// <param name="req">更新景點請求資料</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回更新狀態 (HTTP 200)，僅限管理員存取</returns>
    [HttpPut("spots/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, [FromBody] UpdateSpotRequest req, CancellationToken ct)
    {
        await _admin.EditAsync(id, req, ct);
        return Ok(ApiResponse.Ok(new { id, updated = true }));
    }

    /// <summary>軟刪除景點</summary>
    /// <param name="id">景點 ID</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回刪除狀態 (HTTP 200)，僅限管理員存取</returns>
    [HttpDelete("spots/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _admin.SoftDeleteAsync(id, ct);
        return Ok(ApiResponse.Ok(new { id, deleted = true }));
    }

    /// <summary>恢復已刪除的景點</summary>
    /// <param name="id">景點 ID</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回恢復狀態 (HTTP 200)，僅限管理員存取</returns>
    [HttpPost("spots/{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        await _admin.RestoreAsync(id, ct);
        return Ok(ApiResponse.Ok(new { id, restored = true }));
    }

    /// <summary>查詢使用者列表</summary>
    /// <param name="page">分頁查詢參數</param>
    /// <param name="search">搜尋關鍵字</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回使用者分頁列表 (HTTP 200)，僅限管理員存取</returns>
    [HttpGet("users")]
    public async Task<IActionResult> Users([FromQuery] PageQuery page, [FromQuery] string? search, CancellationToken ct)
        => Ok(ApiResponse.Ok(await _admin.ListUsersAsync(page, search, ct)));
}
