using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Crm.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Crm.Infrastructure.Services;

public class AzureStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureStorageService> _logger;

    public AzureStorageService(IConfiguration configuration, ILogger<AzureStorageService> logger)
    {
        _logger = logger;
        var connectionString = configuration["AzureStorage:ConnectionString"];
        var containerName = configuration["AzureStorage:ContainerName"] ?? "default-container";

        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists();
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            // Overwrite if exists, can be configured via BlobUploadOptions
            await blobClient.UploadAsync(fileStream, true); 

            _logger.LogInformation("File {FileName} uploaded to Azure Blob Storage", fileName);
            return fileName; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} to Azure Blob Storage", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string fileName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException("File not found in Azure Storage", fileName);
            }

            var downloadInfo = await blobClient.DownloadAsync();
            return downloadInfo.Value.Content;
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            _logger.LogError(ex, "Error downloading file {FileName} from Azure Blob Storage", fileName);
            throw;
        }
    }

    public async Task DeleteFileAsync(string fileName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            _logger.LogInformation("File {FileName} deleted from Azure Blob Storage", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileName} from Azure Blob Storage", fileName);
            throw;
        }
    }
    public async Task<List<string>> ListFilesAsync(string pattern)
    {
        try
        {
            var results = new List<string>();
            var prefix = pattern.Replace("*", ""); // Simple prefix support

            await foreach (var blobItem in _containerClient.GetBlobsAsync(prefix: string.IsNullOrEmpty(prefix) ? default : prefix))
            {
                results.Add(blobItem.Name);
            }
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing blobs in container");
            return new List<string>();
        }
    }
}
