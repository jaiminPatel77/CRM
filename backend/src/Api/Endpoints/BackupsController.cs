using Asp.Versioning;
using Crm.Api.Models; // For ApiResponse
using Crm.Application.Common.Interfaces;
using Crm.Domain.Consts; // For Enums
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[Authorize(Roles = "GlobalAdministrator")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/backups")]
public class BackupsController : ControllerBase
{
    private readonly IBackupService _backupService;
    private readonly ILogger<BackupsController> _logger;

    public BackupsController(IBackupService backupService, ILogger<BackupsController> logger)
    {
        _backupService = backupService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetBackups()
    {
        try
        {
            var backups = await _backupService.GetBackupsAsync();
            return Ok(new ApiOkResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_LIST_ALL_ITEMS, backups));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiBadRequestResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_LIST_EXCEPTION, $"Failed to list backups: {ex.Message}"));
        }
    }

    [HttpGet("{fileName}/download")]
    public async Task<IActionResult> DownloadBackup(string fileName)
    {
        // Traversal check
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
        {
             return BadRequest(new ApiBadRequestResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND, "Invalid filename."));
        }

        try
        {
            var stream = await _backupService.GetBackupFileAsync(fileName);
            return File(stream, "application/octet-stream", fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new ApiBadRequestResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND, $"Backup file {fileName} not found."));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiBadRequestResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_GET_EXCEPTION, $"Download failed: {ex.Message}"));
        }
    }

    [HttpDelete("{fileName}")]
    public async Task<IActionResult> DeleteBackup(string fileName)
    {
        try
        {
            await _backupService.DeleteBackupAsync(fileName);
            return Ok(new ApiOkResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_DELETE_ITEM, $"Backup {fileName} deleted."));
        }
        catch (Exception ex)
        {
             return BadRequest(new ApiBadRequestResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_DELETE_EXCEPTION, $"Delete failed: {ex.Message}"));
        }
    }

    [HttpDelete]
    public async Task<IActionResult> CleanBackups([FromBody] BackupCleanupRequest request)
    {
        int deletedCount = 0;

        try
        {
            // 1. Delete by Age
            if (request.OlderThan.HasValue)
            {
                // This service method already returns the count of deleted files
                deletedCount += await _backupService.CleanBackupsAsync(request.OlderThan.Value);
            }

            // 2. Delete Specific Files
            if (request.FileNames != null && request.FileNames.Any())
            {
                foreach (var fileName in request.FileNames)
                {
                    // Basic sanity check
                    if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\")) continue;

                    try
                    {
                        await _backupService.DeleteBackupAsync(fileName);
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        // Log individual failures but continue attempting other deletions
                        _logger.LogWarning(ex, "Failed to delete backup file: {FileName}", fileName);
                    }
                }
            }

            return Ok(new ApiOkResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_DELETE_ITEM, $"Deleted {deletedCount} backup files."));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiBadRequestResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_DELETE_EXCEPTION, $"Cleanup failed: {ex.Message}"));
        }
    }
}

public class BackupCleanupRequest
{
    public List<string>? FileNames { get; set; }
    public DateTime? OlderThan { get; set; }
}
