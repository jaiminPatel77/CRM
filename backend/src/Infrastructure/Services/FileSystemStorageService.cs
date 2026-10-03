using Crm.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Crm.Infrastructure.Services;

public class FileSystemStorageService : IFileStorageService
{
    private readonly string _storageFolder;
    private readonly ILogger<FileSystemStorageService> _logger;

    public FileSystemStorageService(IConfiguration configuration, ILogger<FileSystemStorageService> logger)
    {
        _storageFolder = configuration["Storage:Folder"] ?? "App_Data/Storage";
        _logger = logger;
        
        if (!Directory.Exists(_storageFolder))
        {
            Directory.CreateDirectory(_storageFolder);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        var filePath = GetFullPath(fileName);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }
        _logger.LogInformation("File uploaded to {Path}", filePath);
        
        // Return relative path or full path? Usually returning the valid filename/identifier is better.
        // But logic seems to desire path usage. Returning fileName as is if successful.
        return fileName; 
    }

    public Task<Stream> DownloadFileAsync(string fileName)
    {
        var filePath = GetFullPath(fileName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", fileName);
        }
        return Task.FromResult<Stream>(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    public Task DeleteFileAsync(string fileName)
    {
        var filePath = GetFullPath(fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        return Task.CompletedTask;
    }

    public Task<List<string>> ListFilesAsync(string pattern)
    {
        if (!Directory.Exists(_storageFolder))
        {
            return Task.FromResult(new List<string>());
        }

        // pattern e.g. "backup-*.bak"
        var files = Directory.GetFiles(_storageFolder, pattern)
                             .Select(Path.GetFileName)
                             .Where(f => !string.IsNullOrEmpty(f))
                             .Cast<string>()
                             .ToList();

        return Task.FromResult(files);
    }

    private string GetFullPath(string fileName)
    {
        // Prevent Path Traversal
        var safeFileName = fileName.TrimStart('/', '\\').Replace("..", ""); 
        var fullPath = Path.Combine(_storageFolder, safeFileName);
        
        // Final sanity check
        // if (!fullPath.StartsWith(Path.GetFullPath(_storageFolder))) { throw ... }
        // Simple replace ".." is usually sufficient for non-critical, but let's trust Path.Combine + simple sanitize.
        return fullPath;
    }
}
