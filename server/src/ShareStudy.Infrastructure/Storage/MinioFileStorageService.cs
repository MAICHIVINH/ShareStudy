using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using ShareStudy.Application.Interfaces;

namespace ShareStudy.Infrastructure.Storage;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;

    public MinioFileStorageService(IMinioClient minioClient, IConfiguration configuration)
    {
        _minioClient = minioClient;
        var bucket = configuration["Minio:BucketName"];
        _bucketName = !string.IsNullOrWhiteSpace(bucket) ? bucket : (configuration["MINIO_BUCKET_NAME"] ?? "documents");
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var bucketExistsArgs = new BucketExistsArgs().WithBucket(_bucketName);
        bool found = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);
        if (!found)
        {
            var makeBucketArgs = new MakeBucketArgs().WithBucket(_bucketName);
            await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);

            try
            {
                var policyJson = $@"{{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {{
                            ""Effect"": ""Allow"",
                            ""Principal"": ""*"",
                            ""Action"": [""s3:GetObject""],
                            ""Resource"": [""arn:aws:s3:::{_bucketName}/*""]
                        }}
                    ]
                }}";
                var setPolicyArgs = new SetPolicyArgs().WithBucket(_bucketName).WithPolicy(policyJson);
                await _minioClient.SetPolicyAsync(setPolicyArgs, cancellationToken);
            }
            catch
            {
                // Ignore policy set errors if MinIO is configured with restricted admin policies
            }
        }
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var objectName = $"{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid()}{ext}";

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

        return objectName;
    }

    public async Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storedFileName)) return;

        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(storedFileName);

        await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);
    }

    public async Task<string> GetPresignedUrlAsync(string storedFileName, int expiryInSeconds = 3600, CancellationToken cancellationToken = default)
    {
        var presignedArgs = new PresignedGetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(storedFileName)
            .WithExpiry(expiryInSeconds);

        return await _minioClient.PresignedGetObjectAsync(presignedArgs);
    }
}
