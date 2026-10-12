using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;

namespace ShareStudy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StorageController : ControllerBase
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;

    public StorageController(IMinioClient minioClient, IConfiguration configuration)
    {
        _minioClient = minioClient;
        _bucketName = configuration["Minio:BucketName"] ?? "documents";
    }

    [HttpGet("{*objectName}")]
    public async Task<IActionResult> GetFile(string objectName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return BadRequest("Object name is required.");
        }

        try
        {
            var memoryStream = new MemoryStream();
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream));

            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName);

            var stat = await _minioClient.StatObjectAsync(statObjectArgs, cancellationToken);
            await _minioClient.GetObjectAsync(getObjectArgs, cancellationToken);

            memoryStream.Position = 0;
            var contentType = stat.ContentType;
            if (string.IsNullOrWhiteSpace(contentType) || contentType == "application/octet-stream")
            {
                var ext = Path.GetExtension(objectName).ToLowerInvariant();
                contentType = ext switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    ".gif" => "image/gif",
                    ".pdf" => "application/pdf",
                    _ => "application/octet-stream"
                };
            }

            return File(memoryStream, contentType);
        }
        catch (Exception)
        {
            return NotFound("File không tồn tại trên hệ thống lưu trữ MinIO.");
        }
    }
}
