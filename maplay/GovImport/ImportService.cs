using System.Globalization;
using System.Text.Json;
using Maplay.Common;
using Maplay.Data;
using Maplay.Data.Entities;
using Maplay.Spots;
using Microsoft.EntityFrameworkCore;

namespace Maplay.GovImport;

public interface IImportService
{
    Task<ImportResultDto> ImportToiletAsync(JsonElement payload, Guid adminId, CancellationToken ct);
    Task<ImportResultDto> ImportParkAsync(JsonElement geojson, Guid adminId, CancellationToken ct);
    Task<ImportResultDto> ImportCustomAsync(CustomImportRequest req, Guid adminId, CancellationToken ct);
    Task<PagedResult<ImportHistoryDto>> HistoryAsync(PageQuery page, CancellationToken ct);
}

public class ImportService : IImportService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ImportService> _logger;

    public ImportService(AppDbContext db, ILogger<ImportService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public Task<ImportResultDto> ImportToiletAsync(JsonElement payload, Guid adminId, CancellationToken ct)
    {
        var records = ExtractRecords(payload);
        return RunImportAsync("toilet", adminId, records.Count, ct, () =>
        {
            var spots = new List<(string govId, Spot spot)>();
            foreach (var r in records)
            {
                var govId = FirstString(r, "number", "編號", "id", "公廁編號")
                            ?? Hash(FirstString(r, "公廁名稱", "name"), FirstString(r, "經度", "longitude"), FirstString(r, "緯度", "latitude"));
                var lng = ParseDouble(FirstString(r, "經度", "longitude", "lng", "lon"));
                var lat = ParseDouble(FirstString(r, "緯度", "latitude", "lat"));
                var name = FirstString(r, "公廁名稱", "name", "場所") ?? "公共廁所";
                if (lng is null || lat is null) continue; // 無座標者略過（不算成功）

                spots.Add((govId!, new Spot
                {
                    Name = name,
                    Category = "toilet",
                    Status = SpotStatus.Approved,
                    SpotType = SpotTypes.Permanent,
                    Location = SpotsService.MakePoint(lng.Value, lat.Value),
                    Address = FirstString(r, "地址", "address"),
                    Source = SpotSources.GovImport,
                    GovDataId = govId,
                    Facilities = Array.Empty<string>(),
                    AgeGroups = Array.Empty<string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }));
            }
            return spots;
        });
    }

    public Task<ImportResultDto> ImportParkAsync(JsonElement geojson, Guid adminId, CancellationToken ct)
    {
        if (!geojson.TryGetProperty("features", out var features) || features.ValueKind != JsonValueKind.Array)
            throw AppException.BadRequest(ErrorCodes.ImportFormatInvalid, "GeoJSON 缺少 features 陣列");

        var featureList = features.EnumerateArray().ToList();
        return RunImportAsync("park", adminId, featureList.Count, ct, () =>
        {
            var spots = new List<(string, Spot)>();
            var i = 0;
            foreach (var f in featureList)
            {
                i++;
                if (!TryGetPointFromGeometry(f, out var lng, out var lat)) continue;

                var props = f.TryGetProperty("properties", out var p) ? p : default;
                var name = FirstString(props, "name", "名稱", "PARKNAME", "公園名稱") ?? $"公園 {i}";
                var govId = (f.TryGetProperty("id", out var idEl) ? idEl.ToString() : null)
                            ?? FirstString(props, "id", "編號")
                            ?? Hash(name, lng.ToString(CultureInfo.InvariantCulture), lat.ToString(CultureInfo.InvariantCulture));

                spots.Add((govId!, new Spot
                {
                    Name = name,
                    Category = "park",
                    Status = SpotStatus.Approved,
                    SpotType = SpotTypes.Permanent,
                    Location = SpotsService.MakePoint(lng, lat),
                    Address = FirstString(props, "address", "地址"),
                    Source = SpotSources.GovImport,
                    GovDataId = govId,
                    Facilities = Array.Empty<string>(),
                    AgeGroups = Array.Empty<string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }));
            }
            return spots;
        });
    }

    public Task<ImportResultDto> ImportCustomAsync(CustomImportRequest req, Guid adminId, CancellationToken ct)
    {
        string? Map(JsonElement r, string sysField) =>
            req.Mapping.TryGetValue(sysField, out var src) ? FirstString(r, src) : null;

        return RunImportAsync("custom", adminId, req.Records.Count, ct, () =>
        {
            var spots = new List<(string, Spot)>();
            var i = 0;
            foreach (var r in req.Records)
            {
                i++;
                var lng = ParseDouble(Map(r, "lng"));
                var lat = ParseDouble(Map(r, "lat"));
                if (lng is null || lat is null) continue;

                var name = Map(r, "name") ?? $"匯入景點 {i}";
                var category = Map(r, "category") ?? req.DefaultCategory ?? "park";
                var govId = Map(r, "govDataId") ?? Hash(name, lng.ToString(), lat.ToString());

                spots.Add((govId!, new Spot
                {
                    Name = name,
                    Category = category,
                    Status = SpotStatus.Approved,
                    SpotType = SpotTypes.Permanent,
                    Location = SpotsService.MakePoint(lng.Value, lat.Value),
                    Address = Map(r, "address"),
                    Source = SpotSources.GovImport,
                    GovDataId = govId,
                    Facilities = Array.Empty<string>(),
                    AgeGroups = Array.Empty<string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }));
            }
            return spots;
        });
    }

    public async Task<PagedResult<ImportHistoryDto>> HistoryAsync(PageQuery page, CancellationToken ct)
    {
        var query = _db.ImportHistory.AsQueryable();
        var total = await query.LongCountAsync(ct);
        var items = await query
            .OrderByDescending(h => h.CreatedAt)
            .Skip(page.Skip).Take(page.PageSize)
            .Select(h => new ImportHistoryDto(h.Id, h.Dataset, h.OperatedBy, h.Total, h.Success, h.Skipped, h.Failed, h.CreatedAt))
            .ToListAsync(ct);
        return PagedResult<ImportHistoryDto>.Create(items, total, page);
    }

    /// <summary>共用匯入流程：去重、單一交易、整批 rollback、寫入 import_history。</summary>
    private async Task<ImportResultDto> RunImportAsync(
        string dataset, Guid adminId, int total, CancellationToken ct,
        Func<List<(string govId, Spot spot)>> build)
    {
        var candidates = build();
        var success = 0;
        var skipped = 0;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            // 去重：已存在的 gov_data_id 跳過
            var govIds = candidates.Select(c => c.govId).Distinct().ToList();
            var existing = await _db.Spots
                .Where(s => s.GovDataId != null && govIds.Contains(s.GovDataId))
                .Select(s => s.GovDataId!)
                .ToListAsync(ct);
            var existingSet = new HashSet<string>(existing);
            var seen = new HashSet<string>();

            foreach (var (govId, spot) in candidates)
            {
                if (existingSet.Contains(govId) || !seen.Add(govId))
                {
                    skipped++;
                    continue;
                }
                _db.Spots.Add(spot);
                success++;
            }

            // total 包含無座標而未列入 candidates 者，一併計入 skipped
            skipped += total - candidates.Count;

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "匯入 {Dataset} 失敗，整批回滾", dataset);
            await WriteHistoryAsync(dataset, adminId, total, 0, 0, total, ct);
            throw AppException.Unprocessable(ErrorCodes.ImportFailed, "匯入過程發生錯誤，已整批回滾");
        }

        await WriteHistoryAsync(dataset, adminId, total, success, skipped, 0, ct);
        return new ImportResultDto(dataset, total, success, skipped, 0);
    }

    private async Task WriteHistoryAsync(string dataset, Guid adminId, int total, int success, int skipped, int failed, CancellationToken ct)
    {
        _db.ImportHistory.Add(new ImportHistory
        {
            Dataset = dataset,
            OperatedBy = adminId,
            Total = total,
            Success = success,
            Skipped = skipped,
            Failed = failed,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }

    // ---- 解析輔助 ----

    private static List<JsonElement> ExtractRecords(JsonElement payload)
    {
        if (payload.ValueKind == JsonValueKind.Array)
            return payload.EnumerateArray().ToList();
        if (payload.ValueKind == JsonValueKind.Object &&
            payload.TryGetProperty("records", out var recs) && recs.ValueKind == JsonValueKind.Array)
            return recs.EnumerateArray().ToList();
        throw AppException.BadRequest(ErrorCodes.ImportFormatInvalid, "資料格式錯誤：預期為陣列或含 records 陣列的物件");
    }

    private static string? FirstString(JsonElement obj, params string[] keys)
    {
        if (obj.ValueKind != JsonValueKind.Object) return null;
        foreach (var k in keys)
        {
            if (obj.TryGetProperty(k, out var v))
            {
                var s = v.ValueKind == JsonValueKind.String ? v.GetString() : v.ToString();
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
            }
        }
        return null;
    }

    private static double? ParseDouble(string? s) =>
        double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : null;

    private static bool TryGetPointFromGeometry(JsonElement feature, out double lng, out double lat)
    {
        lng = 0; lat = 0;
        if (!feature.TryGetProperty("geometry", out var geom) ||
            !geom.TryGetProperty("coordinates", out var coords)) return false;

        var type = geom.TryGetProperty("type", out var t) ? t.GetString() : null;
        try
        {
            switch (type)
            {
                case "Point":
                    lng = coords[0].GetDouble();
                    lat = coords[1].GetDouble();
                    return true;
                case "Polygon":
                    // 取第一個外環第一點作代表座標
                    var ring = coords[0];
                    lng = ring[0][0].GetDouble();
                    lat = ring[0][1].GetDouble();
                    return true;
                case "MultiPolygon":
                    var poly = coords[0][0];
                    lng = poly[0][0].GetDouble();
                    lat = poly[0][1].GetDouble();
                    return true;
                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private static string Hash(params string?[] parts)
    {
        var raw = string.Join("|", parts.Select(p => p ?? ""));
        var bytes = System.Security.Cryptography.SHA1.HashData(System.Text.Encoding.UTF8.GetBytes(raw));
        return "auto-" + Convert.ToHexString(bytes)[..16];
    }
}
