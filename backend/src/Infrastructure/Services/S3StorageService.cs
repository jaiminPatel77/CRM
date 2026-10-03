using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Crm.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Crm.Infrastructure.Services;

public class S3StorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(IAmazonS3 s3Client, IConfiguration configuration, ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:BucketName"] ?? "default-bucket";
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        try
        {
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = fileName,
                BucketName = _bucketName,
                CannedACL = S3CannedACL.Private
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            _logger.LogInformation("File {FileName} uploaded to S3 bucket {Bucket}", fileName, _bucketName);
            return fileName;
        }
        catch (AmazonS3Exception e)
        {
            _logger.LogError(e, "Error encountered on server. Message:'{Message}' when writing an object", e.Message);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unknown encountered on server. Message:'{Message}' when writing an object", e.Message);
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string fileName)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName
            };

            var response = await _s3Client.GetObjectAsync(request);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception e)
        {
            _logger.LogError(e, "Error encountered on server. Message:'{Message}' when reading an object", e.Message);
            throw new FileNotFoundException("File not found in S3", fileName, e);
        }
    }

    public async Task DeleteFileAsync(string fileName)
    {
        try
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName
            };

            await _s3Client.DeleteObjectAsync(deleteObjectRequest);
            _logger.LogInformation("File {FileName} deleted from S3 bucket {Bucket}", fileName, _bucketName);
        }
        catch (AmazonS3Exception e)
        {
            _logger.LogError(e, "Error encountered on server. Message:'{Message}' when deleting an object", e.Message);
            throw;
        }
    }

    public async Task<List<string>> ListFilesAsync(string pattern)
    {
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
            };

            var prefix = pattern.Replace("*", "");
            if (!string.IsNullOrEmpty(prefix))
            {
                request.Prefix = prefix;
            }

            var response = await _s3Client.ListObjectsV2Async(request);
            
            return response.S3Objects.Select(o => o.Key).ToList();
        }
        catch (AmazonS3Exception e)
        {
            _logger.LogError(e, "Error listing objects in bucket {Bucket}", _bucketName);
            return new List<string>();
        }
    }
}
