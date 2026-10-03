namespace Crm.Application.Common.Interfaces;

public interface IBackupService
{
    Task BackupDatabaseAsync();
    Task RestoreDatabaseAsync(string backupFileName);
    Task<List<string>> GetBackupsAsync();
    Task<Stream> GetBackupFileAsync(string fileName);
    Task DeleteBackupAsync(string fileName);
    Task<int> CleanBackupsAsync(DateTime olderThan);
}
