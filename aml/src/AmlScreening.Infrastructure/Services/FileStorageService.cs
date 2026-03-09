using System.Collections.Concurrent;
using AmlScreening.Application.Interfaces;
using AmlScreening.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace AmlScreening.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png" };
    private static readonly ConcurrentDictionary<string, string> ContentTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png"
    };

    private readonly FileStorageOptions _options;

    public FileStorageService(IOptions<FileStorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string contentType, string subFolder, CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            throw new ArgumentException("Only PDF, JPG, PNG are allowed.", nameof(fileName));

        var basePath = _options.BasePath;
        if (string.IsNullOrWhiteSpace(basePath))
            basePath = Path.Combine(Path.GetTempPath(), "AmlDocuments");
        var dir = Path.Combine(basePath, subFolder);
        Directory.CreateDirectory(dir);
        var storedFileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(dir, storedFileName);
        var relativePath = Path.Combine(subFolder, storedFileName).Replace('\\', '/');
        await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await content.CopyToAsync(fs, cancellationToken);
        }
        return relativePath;
    }

    public Task<Stream?> GetAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || relativePath.IndexOf("..", StringComparison.Ordinal) >= 0)
            return Task.FromResult<Stream?>(null);
        var basePath = string.IsNullOrWhiteSpace(_options.BasePath) ? Path.Combine(Path.GetTempPath(), "AmlDocuments") : _options.BasePath;
        var fullPath = Path.Combine(basePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);
        try
        {
            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult<Stream?>(stream);
        }
        catch
        {
            return Task.FromResult<Stream?>(null);
        }
    }

    public string GetContentTypeFromFileName(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return ContentTypeMap.TryGetValue(ext ?? "", out var ct) ? ct : "application/octet-stream";
    }
}
