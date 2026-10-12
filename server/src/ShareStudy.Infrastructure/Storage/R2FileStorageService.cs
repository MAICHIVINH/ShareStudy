using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using ShareStudy.Application.Interfaces;

namespace ShareStudy.Infrastructure.Storage;

public class R2FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string? _publicDomain;

    public R2FileStorageService(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["CloudflareR2:BucketName"] 
                      ?? configuration["R2:BucketName"] 
                      ?? "sharestudy-documents";

        _publicDomain = configuration["CloudflareR2:PublicDomain"] 
                       ?? configuration["R2:PublicDomain"];
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var objectKey = $"{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid()}{ext}";

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            InputStream = stream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
        return objectKey;
    }

    public async Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storedFileName)) return;

        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = storedFileName
        };

        await _s3Client.DeleteObjectAsync(request, cancellationToken);
    }

    public async Task<string> GetPresignedUrlAsync(string storedFileName, int expiryInSeconds = 3600, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_publicDomain))
        {
            var baseUrl = _publicDomain.TrimEnd('/');
            return $"{baseUrl}/{storedFileName}";
        }

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = storedFileName,
            Expires = DateTime.UtcNow.AddSeconds(expiryInSeconds)
        };

        return await Task.FromResult(_s3Client.GetPreSignedURL(request));
    }
}
