using Maplay.Common;
using Maplay.Data;
using Maplay.Data.Entities;
using Maplay.Spots;
using Microsoft.EntityFrameworkCore;

namespace Maplay.Admin;

public interface IAdminSpotsService
{
    Task<PagedResult<PendingSpotDto>> PendingAsync(PageQuery page, CancellationToken ct);
    Task ApproveAsync(Guid id, Guid adminId, CancellationToken ct);
    Task RejectAsync(Guid id, string reason, Guid adminId, CancellationToken ct);
    Task EditAsync(Guid id, UpdateSpotRequest req, CancellationToken ct);
    Task SoftDeleteAsync(Guid id, CancellationToken ct);
    Task RestoreAsync(Guid id, CancellationToken ct);
    Task<PagedResult<AdminUserDto>> ListUsersAsync(PageQuery page, string? search, CancellationToken ct);
}

public class AdminSpotsService : IAdminSpotsService
{
    private readonly AppDbContext _db;

    public AdminSpotsService(AppDbContext db) => _db = db;

    public async Task<PagedResult<PendingSpotDto>> PendingAsync(PageQuery page, CancellationToken ct)
    {
        var query = _db.Spots
            .Where(s => s.Status == SpotStatus.Pending && s.DeletedAt == null);

        var total = await query.LongCountAsync(ct);
        var items = await query
            .OrderBy(s => s.CreatedAt)
            .Skip(page.Skip).Take(page.PageSize)
            .Select(s => new PendingSpotDto(
                s.Id, s.Name, s.Category, s.SpotType,
                s.Location.Y, s.Location.X,
                s.SubmittedBy,
                _db.Users.Where(u => u.Id == s.SubmittedBy).Select(u => u.DisplayName).FirstOrDefault(),
                s.CreatedAt))
            .ToListAsync(ct);

        return PagedResult<PendingSpotDto>.Create(items, total, page);
    }

    public async Task ApproveAsync(Guid id, Guid adminId, CancellationToken ct)
    {
        var spot = await RequireSpot(id, ct);
        spot.Status = SpotStatus.Approved;
        spot.ReviewedBy = adminId;
        spot.ReviewedAt = DateTime.UtcNow;
        spot.RejectReason = null;
        spot.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RejectAsync(Guid id, string reason, Guid adminId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw AppException.Validation("rejectReason", "rejectReason 為必填");

        var spot = await RequireSpot(id, ct);
        spot.Status = SpotStatus.Rejected;
        spot.RejectReason = reason.Trim();
        spot.ReviewedBy = adminId;
        spot.ReviewedAt = DateTime.UtcNow;
        spot.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task EditAsync(Guid id, UpdateSpotRequest req, CancellationToken ct)
    {
        var spot = await RequireSpot(id, ct);

        var newSpotType = req.SpotType ?? spot.SpotType;
        var newStart = req.StartDate ?? (newSpotType == SpotTypes.Temporary ? spot.StartDate : null);
        var newEnd = req.EndDate ?? (newSpotType == SpotTypes.Temporary ? spot.EndDate : null);

        SpotsService.ValidateSpotInput(
            req.Name ?? spot.Name,
            req.Category ?? spot.Category,
            newSpotType,
            req.Lat ?? spot.Location.Y,
            req.Lng ?? spot.Location.X,
            req.AgeGroups ?? spot.AgeGroups,
            req.Facilities ?? spot.Facilities,
            newStart, newEnd);

        if (req.Name is not null) spot.Name = req.Name.Trim();
        if (req.Description is not null) spot.Description = req.Description;
        if (req.Category is not null) spot.Category = req.Category;
        if (req.AgeGroups is not null) spot.AgeGroups = req.AgeGroups;
        if (req.Facilities is not null) spot.Facilities = req.Facilities;
        if (req.Address is not null) spot.Address = req.Address;
        if (req.SpotType is not null) spot.SpotType = req.SpotType;

        spot.SpotType = newSpotType;
        spot.StartDate = newSpotType == SpotTypes.Temporary ? newStart : null;
        spot.EndDate = newSpotType == SpotTypes.Temporary ? newEnd : null;

        if (req.Lat is not null || req.Lng is not null)
            spot.Location = SpotsService.MakePoint(req.Lng ?? spot.Location.X, req.Lat ?? spot.Location.Y);

        spot.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct)
    {
        var spot = await RequireSpot(id, ct);
        spot.DeletedAt = DateTime.UtcNow;
        spot.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RestoreAsync(Guid id, CancellationToken ct)
    {
        var spot = await _db.Spots.FirstOrDefaultAsync(s => s.Id == id, ct)
                   ?? throw AppException.NotFound(ErrorCodes.SpotNotFound, "找不到景點");
        spot.DeletedAt = null;
        spot.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<AdminUserDto>> ListUsersAsync(PageQuery page, string? search, CancellationToken ct)
    {
        var query = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = $"%{search.Trim()}%";
            query = query.Where(u =>
                (u.Email != null && EF.Functions.ILike(u.Email, kw)) ||
                EF.Functions.ILike(u.DisplayName, kw));
        }

        var total = await query.LongCountAsync(ct);
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip(page.Skip).Take(page.PageSize)
            .Select(u => new AdminUserDto(u.Id, u.Email, u.DisplayName, u.Role, u.Provider, u.CreatedAt))
            .ToListAsync(ct);

        return PagedResult<AdminUserDto>.Create(items, total, page);
    }

    private async Task<Spot> RequireSpot(Guid id, CancellationToken ct) =>
        await _db.Spots.FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, ct)
        ?? throw AppException.NotFound(ErrorCodes.SpotNotFound, "找不到景點");
}
