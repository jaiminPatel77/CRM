using Crm.Application.Common.Interfaces;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Crm.Tests.Unit.Services;

public class BackupServiceTests
{
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<BackupService> _logger;
    private readonly IEmailService _emailService;
    private readonly ApplicationDbContext _dbContext;

    public BackupServiceTests()
    {
        _fileStorage = Substitute.For<IFileStorageService>();
        _logger = Substitute.For<ILogger<BackupService>>();
        _emailService = Substitute.For<IEmailService>();

        // Use SQLite InMemory for Relational support (GetDbConnection works)
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
            
        var interceptor = new Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor(
            Substitute.For<ICurrentUserService>(),
            Substitute.For<IDateTime>()
        );
        _dbContext = new ApplicationDbContext(options, interceptor);
        _dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task BackupDatabaseAsync_ShouldUploadFile_WhenSuccessful()
    {
        // Arrange
        var service = new BackupService(_dbContext, _fileStorage, _logger, _emailService);

        // Act
        await service.BackupDatabaseAsync();

        // Assert
        await _fileStorage.Received(1).UploadFileAsync(Arg.Any<Stream>(), Arg.Is<string>(s => s.StartsWith("backup-") && s.EndsWith(".bak")));
    }

    [Fact]
    public async Task BackupDatabaseAsync_ShouldSendEmail_WhenExceptionOccurs()
    {
        // Arrange
        _fileStorage.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>())
            .ThrowsAsync(new Exception("Upload failed"));

        var service = new BackupService(_dbContext, _fileStorage, _logger, _emailService);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => service.BackupDatabaseAsync());
        Assert.Equal("Upload failed", ex.Message);
        await _emailService.Received(1).SendEmailAsync(Arg.Any<string>(), "Backup Failed", Arg.Any<string>());
    }
    
    [Fact]
    public async Task CleanBackupsAsync_ShouldDeleteOldFiles_AndReturnCount()
    {
        // Arrange
        var today = DateTime.UtcNow;
        var oldDate = today.AddDays(-10);
        var recentDate = today.AddDays(-1);
        
        // Format: backup-{dbName}-yyyyMMddHHmmss.bak
        var oldFile = $"backup-TestDB-{oldDate:yyyyMMddHHmmss}.bak";
        var recentFile = $"backup-TestDB-{recentDate:yyyyMMddHHmmss}.bak";
        var invalidFile = "random-file.txt";

        _fileStorage.ListFilesAsync(Arg.Any<string>())
            .Returns(new List<string> { oldFile, recentFile, invalidFile });

        var service = new BackupService(_dbContext, _fileStorage, _logger, _emailService);
        var cutoffDate = today.AddDays(-5); // Clean older than 5 days

        // Act
        var result = await service.CleanBackupsAsync(cutoffDate);

        // Assert
        Assert.Equal(1, result); // Only oldFile
        await _fileStorage.Received(1).DeleteFileAsync(oldFile);
        await _fileStorage.DidNotReceive().DeleteFileAsync(recentFile);
        await _fileStorage.DidNotReceive().DeleteFileAsync(invalidFile);
    }
}
