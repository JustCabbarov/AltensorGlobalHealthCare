using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using AltensorGlobalHealthCare.Application.Common.Exceptions;
using AltensorGlobalHealthCare.Application.Interfaces.Services;

namespace AltensorGlobalHealthCare.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LocalFileStorageService> _logger;

    private static readonly HashSet<string> AllowedMedicalExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".dcm", ".zip", ".rar", ".doc", ".docx"
    };

    private static readonly HashSet<string> AllowedPublicExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".svg"
    };

    public LocalFileStorageService(IWebHostEnvironment env, ILogger<LocalFileStorageService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string> SavePublicFileAsync(Stream fileStream, string fileName, string subFolder, CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedPublicExtensions.Contains(ext))
        {
            throw new FileValidationException($"İcazə verilməyən fayl formatı: {ext}. Yalnız şəkil faylları qəbul edilir.");
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var folderPath = Path.Combine(webRoot, "uploads", subFolder);
        Directory.CreateDirectory(folderPath);

        var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folderPath, uniqueFileName);

        using (var destStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(destStream, cancellationToken);
        }

        return $"/uploads/{subFolder}/{uniqueFileName}".Replace("\\", "/");
    }

    public async Task<string> SaveSecureMedicalDocumentAsync(Stream fileStream, string fileName, Guid inquiryId, CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedMedicalExtensions.Contains(ext))
        {
            throw new FileValidationException($"İcazə verilməyən tibbi sənəd formatı: {ext}. Yalnız PDF, şəkil, DICOM və ya arxiv faylları yükləyə bilərsiniz.");
        }

        // App_Data is strictly OUTSIDE wwwroot
        var baseDir = Path.Combine(_env.ContentRootPath, "App_Data", "medical-docs", inquiryId.ToString());
        Directory.CreateDirectory(baseDir);

        var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(baseDir, uniqueFileName);

        using (var destStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(destStream, cancellationToken);
        }

        // Returns relative internal path
        return Path.Combine("App_Data", "medical-docs", inquiryId.ToString(), uniqueFileName);
    }

    public (Stream FileStream, string ContentType, string FileName)? GetSecureMedicalDocument(string relativePath)
    {
        var fullPath = Path.Combine(_env.ContentRootPath, relativePath);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        var fileName = Path.GetFileName(fullPath);
        var ext = Path.GetExtension(fullPath).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".dcm" => "application/dicom",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return (stream, contentType, fileName);
    }

    public void DeleteFile(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        try
        {
            var fullPath = relativePath.StartsWith("/uploads")
                ? Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), relativePath.TrimStart('/'))
                : Path.Combine(_env.ContentRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Fayl silinərkən xəta: {Path}. {Message}", relativePath, ex.Message);
        }
    }
}

