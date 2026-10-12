using Microsoft.AspNetCore.Http;
using ShareStudy.Application.Common.Exceptions;

namespace ShareStudy.Application.Common;

public static class FileValidationHelper
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".zip", ".rar"
    };

    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50MB

    private static readonly Dictionary<string, List<byte[]>> MagicBytes = new()
    {
        { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } }, // %PDF
        { ".zip", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } }, // PK..
        { ".docx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
        { ".pptx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
        { ".xlsx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
        { ".rar", new List<byte[]> { new byte[] { 0x52, 0x61, 0x72, 0x21 } } }, // Rar!
        { ".doc", new List<byte[]> { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
        { ".ppt", new List<byte[]> { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
        { ".xls", new List<byte[]> { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, new byte[] { 0x50, 0x4B, 0x03, 0x04 } } }
    };

    public static async Task ValidateFileAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new BadRequestException("Vui lòng đính kèm file tài liệu.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new BadRequestException("Dung lượng file vượt quá giới hạn cho phép (tối đa 50MB).");
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            throw new BadRequestException($"Định dạng file '{ext}' không được hỗ trợ. Các định dạng cho phép: pdf, doc, docx, ppt, pptx, xls, xlsx, zip, rar.");
        }

        // Check magic bytes signature
        using var stream = file.OpenReadStream();
        var header = new byte[8];
        var bytesRead = await stream.ReadAsync(header, 0, header.Length, cancellationToken);
        stream.Position = 0; // Reset position

        if (bytesRead < 4)
        {
            throw new BadRequestException("File bị lỗi hoặc nội dung không hợp lệ.");
        }

        if (MagicBytes.TryGetValue(ext, out var validSignatures))
        {
            bool isValidSignature = validSignatures.Any(sig => header.Take(sig.Length).SequenceEqual(sig));
            if (!isValidSignature)
            {
                throw new BadRequestException($"Nội dung file không đúng với định dạng mở rộng '{ext}'. Vui lòng kiểm tra lại file.");
            }
        }
    }
}
