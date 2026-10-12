namespace ShareStudy.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default);
    Task<string> GetPresignedUrlAsync(string storedFileName, int expiryInSeconds = 3600, CancellationToken cancellationToken = default);
}
