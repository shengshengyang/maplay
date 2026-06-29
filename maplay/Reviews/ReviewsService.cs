using Maplay.Common;
using Maplay.Data;
using Maplay.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maplay.Reviews;

public interface IReviewsService
{
    Task<ReviewDto> CreateAsync(Guid spotId, CreateReviewRequest req, Guid userId, CancellationToken ct);
    Task<ReviewDto> UpdateAsync(Guid spotId, Guid reviewId, UpdateReviewRequest req, Guid userId, CancellationToken ct);
}

public class ReviewsService : IReviewsService
{
    private readonly AppDbContext _db;

    public ReviewsService(AppDbContext db) => _db = db;

    public async Task<ReviewDto> CreateAsync(Guid spotId, CreateReviewRequest req, Guid userId, CancellationToken ct)
    {
        ValidateRange(req.Rating, req.CleanLevel);

        var spot = await _db.Spots.FirstOrDefaultAsync(s => s.Id == spotId && s.DeletedAt == null, ct)
                   ?? throw AppException.NotFound(ErrorCodes.SpotNotFound, "找不到景點");

        if (spot.Status != SpotStatus.Approved)
            throw AppException.Unprocessable(ErrorCodes.SpotNotApproved, "景點尚未核准，無法評價");

        var duplicate = await _db.Reviews.AnyAsync(r => r.SpotId == spotId && r.UserId == userId, ct);
        if (duplicate)
            throw AppException.Conflict(ErrorCodes.ReviewDuplicate, "您已評價過此景點");

        var review = new Review
        {
            SpotId = spotId,
            UserId = userId,
            Rating = (short)req.Rating,
            CleanLevel = (short)req.CleanLevel,
            Content = req.Content,
            VisitedAt = req.VisitedAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync(ct);
        return ToDto(review);
    }

    public async Task<ReviewDto> UpdateAsync(Guid spotId, Guid reviewId, UpdateReviewRequest req, Guid userId, CancellationToken ct)
    {
        ValidateRange(req.Rating, req.CleanLevel);

        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.SpotId == spotId, ct)
                     ?? throw AppException.NotFound(ErrorCodes.ReviewNotFound, "找不到評價");

        if (review.UserId != userId)
            throw AppException.Forbidden("僅能修改自己的評價");

        review.Rating = (short)req.Rating;
        review.CleanLevel = (short)req.CleanLevel;
        review.Content = req.Content;
        review.VisitedAt = req.VisitedAt;
        review.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return ToDto(review);
    }

    private static void ValidateRange(int rating, int cleanLevel)
    {
        var errors = new Dictionary<string, string[]>();
        if (rating is < 1 or > 5) errors["rating"] = new[] { "rating 需為 1–5" };
        if (cleanLevel is < 1 or > 5) errors["cleanLevel"] = new[] { "cleanLevel 需為 1–5" };
        if (errors.Count > 0) throw AppException.Validation(errors);
    }

    private static ReviewDto ToDto(Review r) =>
        new(r.Id, r.SpotId, r.UserId, r.Rating, r.CleanLevel, r.Content, r.VisitedAt, r.CreatedAt, r.UpdatedAt);
}
