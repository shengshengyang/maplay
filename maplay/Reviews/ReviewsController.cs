using Maplay.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maplay.Reviews;

[ApiController]
[Authorize]
[Route("api/spots/{spotId:guid}/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewsService _reviews;
    private readonly ICurrentUser _current;

    public ReviewsController(IReviewsService reviews, ICurrentUser current)
    {
        _reviews = reviews;
        _current = current;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid spotId, [FromBody] CreateReviewRequest req, CancellationToken ct)
    {
        var review = await _reviews.CreateAsync(spotId, req, _current.RequireUserId(), ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse.Ok(review));
    }

    [HttpPut("{reviewId:guid}")]
    public async Task<IActionResult> Update(Guid spotId, Guid reviewId, [FromBody] UpdateReviewRequest req, CancellationToken ct)
    {
        var review = await _reviews.UpdateAsync(spotId, reviewId, req, _current.RequireUserId(), ct);
        return Ok(ApiResponse.Ok(review));
    }
}
