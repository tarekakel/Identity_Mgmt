namespace AmlScreening.Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>Saves file content and returns the stored relative path (e.g. customerId/guid.ext).</summary>
    Task<string> SaveAsync(Stream content, string fileName, string contentType, string subFolder, CancellationToken cancellationToken = default);

    /// <summary>Opens the file for reading; returns null if not found.</summary>
    Task<Stream?> GetAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>Gets content type from file extension for download.</summary>
    string GetContentTypeFromFileName(string fileName);
}
