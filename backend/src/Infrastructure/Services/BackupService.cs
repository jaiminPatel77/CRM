using Crm.Application.Common.Interfaces;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Crm.Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<BackupService> _logger;
    private readonly IEmailService _emailService;

    public BackupService(
        ApplicationDbContext context,
        IFileStorageService fileStorage,
        ILogger<BackupService> logger,
        IEmailService emailService)
    {
        _context = context;
        _fileStorage = fileStorage;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task BackupDatabaseAsync()
    {
        _logger.LogInformation("Starting Database Backup...");

        try
        {
            var dbName = _context.Database.GetDbConnection().Database;
            var fileName = $"backup-{dbName}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8]}.bak";

            // In a real scenario, we would execute "BACKUP DATABASE [db] TO DISK = '...'" 
            // OR use a tool to generate a BACPAC.
            // For this template, we simulate the text backup creation to demonstrate the generic flow.
            
            var connectionString = _context.Database.GetConnectionString();
            var backupContent = $"Simulated Backup of {dbName} at {DateTime.UtcNow}\nConnection: {connectionString}";
            
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(backupContent)))
            {
                await _fileStorage.UploadFileAsync(stream, fileName);
            }

            _logger.LogInformation("Backup uploaded successfully: {FileName}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup failed");
            try 
            {
                await _emailService.SendEmailAsync("admin@example.com", "Backup Failed", $"Error: {ex.Message}");
            }
            catch (Exception emailEx)
            {
                 _logger.LogWarning(emailEx, "Failed to send error notification email");
            }
            throw;
        }
    }

    public async Task RestoreDatabaseAsync(string backupFileName)
    {
        _logger.LogInformation("Starting Database Restore from {FileName}...", backupFileName);

        try
        {
            // 1. Download Backup File
            using (var stream = await _fileStorage.DownloadFileAsync(backupFileName))
            {
                // Simulate processing/restoring
                _logger.LogInformation("Downloaded backup file. Size: {Size} bytes", stream.Length);
            }

            // 2. Perform Restore
            _logger.LogWarning("SIMULATION: Database corrupted? No, just testing restore logic. Restore simulated successfully.");

            await _emailService.SendEmailAsync("admin@example.com", "Database Restored", $"Database was restored from {backupFileName} successfully (Simulated).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Restore failed");
            await _emailService.SendEmailAsync("admin@example.com", "Restore Failed", $"Error: {ex.Message}");
            throw;
        }
    }

    public async Task<List<string>> GetBackupsAsync()
    {
        return await _fileStorage.ListFilesAsync("backup-*");
    }

    public Task<Stream> GetBackupFileAsync(string fileName)
    {
        return _fileStorage.DownloadFileAsync(fileName);
    }

    public async Task DeleteBackupAsync(string fileName)
    {
        await _fileStorage.DeleteFileAsync(fileName);
        _logger.LogInformation("Backup {FileName} deleted.", fileName);
    }

    public async Task<int> CleanBackupsAsync(DateTime olderThan)
    {
        var backups = await GetBackupsAsync();
        int deletedCount = 0;

        foreach (var backup in backups)
        {
            // Expected format: backup-{dbName}-yyyyMMddHHmmss.bak
            var parts = backup.Split('-');
            if (parts.Length >= 3)
            {
                var datePart = parts.Last().Replace(".bak", "");
                if (DateTime.TryParseExact(datePart, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var date))
                {
                    if (date < olderThan)
                    {
                        await DeleteBackupAsync(backup);
                        deletedCount++;
                    }
                }
            }
        }
        _logger.LogInformation("Cleaned up {Count} old backups.", deletedCount);
        return deletedCount;
    }
}
