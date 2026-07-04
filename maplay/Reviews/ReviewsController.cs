using Maplay.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maplay.Reviews;

/// <summary>評價管理 API 控制器</summary>
[ApiController]
[Authorize]
[Route("api/spots/{spotId:guid}/reviews")]
[Tags("Reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewsService _reviews;
    private readonly ICurrentUser _current;

    public ReviewsController(IReviewsService reviews, ICurrentUser current)
    {
        _reviews = reviews;
        _current = current;
    }

    /// <summary>建立景點評價</summary>
    /// <param name="spotId">景點 ID</param>
    /// <param name="req">建立評價請求資料</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回新建評價資訊 (HTTP 201)，未認證時返回 401</returns>
    [HttpPost]
    public async Task<IActionResult> Create(Guid spotId, [FromBody] CreateReviewRequest req, CancellationToken ct)
    {
        var review = await _reviews.CreateAsync(spotId, req, _current.RequireUserId(), ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse.Ok(review));
    }

    /// <summary>更新景點評價</summary>
    /// <param name="spotId">景點 ID</param>
    /// <param name="reviewId">評價 ID</param>
    /// <param name="req">更新評價請求資料</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>成功時返回更新後的評價資訊 (HTTP 200)，未認證時返回 401</returns>
    [HttpPut("{reviewId:guid}")]
    public async Task<IActionResult> Update(Guid spotId, Guid reviewId, [FromBody] UpdateReviewRequest req, CancellationToken ct)
    {
        var review = await _reviews.UpdateAsync(spotId, reviewId, req, _current.RequireUserId(), ct);
        return Ok(ApiResponse.Ok(review));
    }
}
