using Crm.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace Crm.Tests.Unit.Services;

public class FileSystemStorageServiceTests
{
    private readonly ILogger<FileSystemStorageService> _logger;
    private readonly IConfiguration _configuration;

    public FileSystemStorageServiceTests()
    {
        _logger = Substitute.For<ILogger<FileSystemStorageService>>();
        _configuration = Substitute.For<IConfiguration>();
        // Setup configuration to return a path
        _configuration["Storage:Folder"].Returns("App_Data/Backups");
    }

    [Fact]
    public async Task UploadFileAsync_ShouldCreateFile()
    {
        // Arrange
        var service = new FileSystemStorageService(_configuration, _logger);
        var fileName = $"test_upload_{Guid.NewGuid()}.txt";
        var content = "test content";
        
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        // Act
        await service.UploadFileAsync(stream, fileName);

        // Assert
        var path = Path.Combine("App_Data", "Backups", fileName);
        Assert.True(File.Exists(path));
        
        // Cleanup
        if (File.Exists(path)) File.Delete(path);
    }
}
