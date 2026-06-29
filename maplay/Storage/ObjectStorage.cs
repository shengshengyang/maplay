using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace Maplay.Storage;

public class StorageOptions
{
    public string Endpoint { get; set; } = string.Empty;       // 內部存取端點（後端→MinIO）
    public string PublicEndpoint { get; set; } = string.Empty;  // 對外可存取端點（瀏覽器）
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;
    public bool ForcePathStyle { get; set; } = true;
}

public record UploadedObject(string ObjectKey, string Url);

/// <summary>物件儲存抽象：業務碼僅依賴此介面，不直接依賴特定供應商 SDK。</summary>
public interface IObjectStorage
{
    Task<UploadedObject> UploadAsync(Stream content, string contentType, string extension, CancellationToken ct);
    Task DeleteAsync(string objectKey, CancellationToken ct);
    string GetPublicUrl(string objectKey);
}

/// <summary>S3 相容實作（本機可接 MinIO，正式可接雲端物件儲存而不改業務碼）。</summary>
public class S3ObjectStorage : IObjectStorage
{
    private readonly IAmazonS3 _s3;
    private readonly StorageOptions _opt;

    public S3ObjectStorage(IOptions<StorageOptions> opt)
    {
        _opt = opt.Value;
        _s3 = new AmazonS3Client(
            _opt.AccessKey,
            _opt.SecretKey,
            new AmazonS3Config
            {
                ServiceURL = _opt.Endpoint,
                ForcePathStyle = _opt.ForcePathStyle,
                AuthenticationRegion = "us-east-1"
            });
    }

    public async Task<UploadedObject> UploadAsync(Stream content, string contentType, string extension, CancellationToken ct)
    {
        var key = $"spots/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{extension}";
        await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _opt.Bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            DisablePayloadSigning = true
        }, ct);
        return new UploadedObject(key, GetPublicUrl(key));
    }

    public Task DeleteAsync(string objectKey, CancellationToken ct) =>
        _s3.DeleteObjectAsync(new DeleteObjectRequest { BucketName = _opt.Bucket, Key = objectKey }, ct);

    public string GetPublicUrl(string objectKey)
    {
        var baseUrl = (string.IsNullOrEmpty(_opt.PublicEndpoint) ? _opt.Endpoint : _opt.PublicEndpoint)
            .TrimEnd('/');
        return _opt.ForcePathStyle
            ? $"{baseUrl}/{_opt.Bucket}/{objectKey}"
            : $"{baseUrl}/{objectKey}";
    }
}
