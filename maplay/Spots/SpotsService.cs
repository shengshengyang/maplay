using Maplay.Common;
using Maplay.Data;
using Maplay.Data.Entities;
using Maplay.Storage;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Npgsql;

namespace Maplay.Spots;

public interface ISpotsService
{
    Task<IReadOnlyList<NearbySpotDto>> NearbyAsync(NearbyQuery q, CancellationToken ct);
    Task<SpotDetailDto> GetDetailAsync(Guid id, ICurrentUser current, CancellationToken ct);
    Task<PagedResult<SpotSummaryDto>> ActiveTempAsync(PageQuery page, CancellationToken ct);
    Task<SpotDetailDto> CreateAsync(CreateSpotRequest req, Guid userId, CancellationToken ct);
    Task<IReadOnlyList<SpotImageDto>> UploadImagesAsync(Guid spotId, IReadOnlyList<IFormFile> files, Guid userId, CancellationToken ct);
}

public class SpotsService : ISpotsService
{
    private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/webp" };
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxFileBytes = 5 * 1024 * 1024;
    private const int MaxFilesPerUpload = 5;

    private static readonly GeometryFactory Geo = new(new PrecisionModel(), 4326);

    private readonly AppDbContext _db;
    private readonly IObjectStorage _storage;

    public SpotsService(AppDbContext db, IObjectStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<IReadOnlyList<NearbySpotDto>> NearbyAsync(NearbyQuery q, CancellationToken ct)
    {
        if (q.Lat is null || q.Lng is null)
            throw AppException.Validation("lat/lng", "lat 與 lng 為必填");
        if (q.Radius > 20000)
            throw AppException.Validation("radius", "radius 不可超過 20000 公尺");
        var radius = q.Radius <= 0 ? 2000 : q.Radius;

        const string sql = @"
SELECT id, name, category, spot_type, start_date, end_date, age_groups, facilities,
       ST_Y(location) AS lat, ST_X(location) AS lng,
       ST_Distance(location::geography,
                   ST_SetSRID(ST_MakePoint(@lng, @lat), 4326)::geography) AS distance_meters
FROM spots
WHERE status = 'approved'
  AND deleted_at IS NULL
  AND (spot_type = 'permanent'
       OR (start_date <= CURRENT_DATE AND end_date >= CURRENT_DATE))
  AND ST_DWithin(location::geography,
                 ST_SetSRID(ST_MakePoint(@lng, @lat), 4326)::geography, @radius)
  AND (@category::text IS NULL OR category = @category::text)
  AND (@spotType::text IS NULL OR spot_type = @spotType::text)
  AND (@age::text IS NULL OR @age::text = ANY(age_groups))
ORDER BY distance_meters ASC
LIMIT 500;";

        var conn = (NpgsqlConnection)_db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(ct);

        var rows = new List<NearbySpotDto>();
        var spotIds = new List<Guid>();

        await using (var cmd = new NpgsqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("lat", q.Lat.Value);
            cmd.Parameters.AddWithValue("lng", q.Lng.Value);
            cmd.Parameters.AddWithValue("radius", (double)radius);
            cmd.Parameters.AddWithValue("category", (object?)q.Category ?? DBNull.Value);
            cmd.Parameters.AddWithValue("spotType", (object?)q.SpotType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("age", (object?)q.Age ?? DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var id = reader.GetGuid(reader.GetOrdinal("id"));
                spotIds.Add(id);
                rows.Add(new NearbySpotDto(
                    id,
                    reader.GetString(reader.GetOrdinal("name")),
                    reader.GetString(reader.GetOrdinal("category")),
                    reader.GetString(reader.GetOrdinal("spot_type")),
                    reader.GetDouble(reader.GetOrdinal("lat")),
                    reader.GetDouble(reader.GetOrdinal("lng")),
                    reader.GetDouble(reader.GetOrdinal("distance_meters")),
                    GetArray(reader, "age_groups"),
                    GetArray(reader, "facilities"),
                    GetDateOrNull(reader, "start_date"),
                    GetDateOrNull(reader, "end_date"),
                    null));
            }
        }

        // 補各景點封面圖（單獨查詢，避免巢狀 reader）
        var covers = await _db.SpotImages
            .Where(i => spotIds.Contains(i.SpotId) && i.IsCover)
            .ToDictionaryAsync(i => i.SpotId, i => i.Url, ct);

        return rows
            .Select(r => covers.TryGetValue(r.Id, out var url) ? r with { CoverUrl = url } : r)
            .ToList();
    }

    public async Task<SpotDetailDto> GetDetailAsync(Guid id, ICurrentUser current, CancellationToken ct)
    {
        var spot = await _db.Spots
            .Include(s => s.Images)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (spot is null)
            throw AppException.NotFound(ErrorCodes.SpotNotFound, "找不到景點");

        // 已軟刪除：對非 admin 一律 404
        if (spot.DeletedAt is not null && !current.IsAdmin)
            throw AppException.NotFound(ErrorCodes.SpotNotFound, "找不到景點");

        // 未核准：僅 admin 或回報本人可見
        if (spot.Status != SpotStatus.Approved && !current.IsAdmin && current.UserId != spot.SubmittedBy)
            throw AppException.NotFound(ErrorCodes.SpotNotFound, "找不到景點");

        var stats = await _db.Reviews
            .Where(r => r.SpotId == id)
            .GroupBy(r => 1)
            .Select(g => new ReviewStatsDto(
                g.Average(x => (double)x.Rating),
                g.Average(x => (double)x.CleanLevel),
                g.Count()))
            .FirstOrDefaultAsync(ct) ?? new ReviewStatsDto(0, 0, 0);

        var recent = await _db.Reviews
            .Where(r => r.SpotId == id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new ReviewBriefDto(
                r.Id, r.UserId, r.User!.DisplayName, r.Rating, r.CleanLevel,
                r.Content, r.VisitedAt, r.CreatedAt))
            .ToListAsync(ct);

        return ToDetail(spot, stats, recent);
    }

    public async Task<PagedResult<SpotSummaryDto>> ActiveTempAsync(PageQuery page, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = _db.Spots.Where(s =>
            s.SpotType == SpotTypes.Temporary &&
            s.Status == SpotStatus.Approved &&
            s.DeletedAt == null &&
            s.StartDate <= today && s.EndDate >= today);

        var total = await query.LongCountAsync(ct);
        var items = await query
            .OrderBy(s => s.EndDate)
            .Skip(page.Skip).Take(page.PageSize)
            .Select(s => ToSummary(s))
            .ToListAsync(ct);

        return PagedResult<SpotSummaryDto>.Create(items, total, page);
    }

    public async Task<SpotDetailDto> CreateAsync(CreateSpotRequest req, Guid userId, CancellationToken ct)
    {
        ValidateSpotInput(req.Name, req.Category, req.SpotType, req.Lat, req.Lng,
            req.AgeGroups, req.Facilities, req.StartDate, req.EndDate);

        var isTemp = req.SpotType == SpotTypes.Temporary;
        var spot = new Spot
        {
            Name = req.Name.Trim(),
            Description = req.Description,
            Category = req.Category,
            Status = SpotStatus.Pending,
            SpotType = req.SpotType,
            StartDate = isTemp ? req.StartDate : null,
            EndDate = isTemp ? req.EndDate : null,
            AgeGroups = req.AgeGroups,
            Facilities = req.Facilities,
            Location = MakePoint(req.Lng!.Value, req.Lat!.Value),
            Address = req.Address,
            SubmittedBy = userId,
            Source = SpotSources.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Spots.Add(spot);
        await _db.SaveChangesAsync(ct);

        return ToDetail(spot, new ReviewStatsDto(0, 0, 0), Array.Empty<ReviewBriefDto>());
    }

    public async Task<IReadOnlyList<SpotImageDto>> UploadImagesAsync(
        Guid spotId, IReadOnlyList<IFormFile> files, Guid userId, CancellationToken ct)
    {
        var spot = await _db.Spots.FirstOrDefaultAsync(s => s.Id == spotId && s.DeletedAt == null, ct)
                   ?? throw AppException.NotFound(ErrorCodes.SpotNotFound, "找不到景點");

        if (files.Count == 0)
            throw AppException.Validation("files", "請至少上傳一個檔案");
        if (files.Count > MaxFilesPerUpload)
            throw AppException.BadRequest(ErrorCodes.TooManyFiles, $"單次至多上傳 {MaxFilesPerUpload} 張");

        foreach (var f in files)
        {
            var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
            if (!AllowedImageTypes.Contains(f.ContentType) || !AllowedExtensions.Contains(ext))
                throw AppException.BadRequest(ErrorCodes.FileTypeInvalid, "僅允許 jpg/jpeg/png/webp");
            if (f.Length > MaxFileBytes)
                throw AppException.BadRequest(ErrorCodes.FileTooLarge, "單檔不可超過 5MB");
        }

        var hasCover = await _db.SpotImages.AnyAsync(i => i.SpotId == spotId && i.IsCover, ct);
        var images = new List<SpotImage>();

        foreach (var f in files)
        {
            var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
            await using var stream = f.OpenReadStream();
            var uploaded = await _storage.UploadAsync(stream, f.ContentType, ext, ct);

            var image = new SpotImage
            {
                Id = Guid.NewGuid(),
                SpotId = spotId,
                Url = uploaded.Url,
                ObjectKey = uploaded.ObjectKey,
                IsCover = !hasCover,           // 首張圖片設為封面
                UploadedBy = userId,
                CreatedAt = DateTime.UtcNow
            };
            hasCover = true;
            _db.SpotImages.Add(image);
            images.Add(image);
        }

        await _db.SaveChangesAsync(ct);
        return images.Select(i => new SpotImageDto(i.Id, i.Url, i.IsCover)).ToList();
    }

    // ---- 共用驗證與轉換 ----

    public static void ValidateSpotInput(
        string? name, string? category, string? spotType, double? lat, double? lng,
        string[]? ageGroups, string[]? facilities, DateOnly? startDate, DateOnly? endDate)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
            errors["name"] = new[] { "name 必填且長度 1–200" };
        if (category is not null && !SpotCategories.All.Contains(category))
            errors["category"] = new[] { "category 非合法列舉值" };
        if (spotType is not null && spotType is not (SpotTypes.Permanent or SpotTypes.Temporary))
            errors["spotType"] = new[] { "spotType 僅能為 permanent 或 temporary" };
        if (lat is null or < -90 or > 90)
            errors["lat"] = new[] { "lat 必填且介於 -90~90" };
        if (lng is null or < -180 or > 180)
            errors["lng"] = new[] { "lng 必填且介於 -180~180" };
        if (ageGroups is not null && ageGroups.Any(a => !AgeGroupValues.All.Contains(a)))
            errors["ageGroups"] = new[] { "ageGroups 含非合法列舉值（0-3/3-7/7-12/12+）" };
        if (facilities is not null && facilities.Any(f => !FacilityValues.All.Contains(f)))
            errors["facilities"] = new[] { "facilities 含非合法列舉值" };

        if (errors.Count > 0)
            throw AppException.Validation(errors);

        // 期間限定日期（與列舉錯誤分開，使用 422 TEMP_DATE_INVALID）
        if (spotType == SpotTypes.Temporary)
        {
            if (startDate is null || endDate is null)
                throw AppException.Unprocessable(ErrorCodes.TempDateInvalid, "期間限定景點需提供 startDate 與 endDate");
            if (endDate < startDate)
                throw AppException.Unprocessable(ErrorCodes.TempDateInvalid, "endDate 不可早於 startDate");
        }
    }

    public static Point MakePoint(double lng, double lat)
    {
        var p = Geo.CreatePoint(new Coordinate(lng, lat));
        return p;
    }

    private static SpotDetailDto ToDetail(Spot s, ReviewStatsDto stats, IReadOnlyList<ReviewBriefDto> recent) =>
        new(s.Id, s.Name, s.Description, s.Category, s.Status, s.SpotType,
            s.StartDate, s.EndDate, s.AgeGroups, s.Facilities,
            s.Location.Y, s.Location.X, s.Address, s.Source,
            s.Images.Select(i => new SpotImageDto(i.Id, i.Url, i.IsCover)).ToList(),
            stats, recent, s.CreatedAt);

    private static SpotSummaryDto ToSummary(Spot s) =>
        new(s.Id, s.Name, s.Category, s.Status, s.SpotType,
            s.Location.Y, s.Location.X, s.StartDate, s.EndDate, s.CreatedAt);

    private static string[] GetArray(NpgsqlDataReader r, string col)
    {
        var i = r.GetOrdinal(col);
        return r.IsDBNull(i) ? Array.Empty<string>() : (string[])r.GetValue(i);
    }

    private static DateOnly? GetDateOrNull(NpgsqlDataReader r, string col)
    {
        var i = r.GetOrdinal(col);
        return r.IsDBNull(i) ? null : DateOnly.FromDateTime(r.GetDateTime(i));
    }
}
