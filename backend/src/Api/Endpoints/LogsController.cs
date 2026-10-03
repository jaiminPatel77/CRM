using Asp.Versioning;
using Crm.Api.Models; // For ApiResponse
using Crm.Domain.Consts; // For Enums
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Text;

namespace Crm.Api.Controllers;

[Authorize(Roles = "GlobalAdministrator")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/logs")]
public class LogsController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    internal const int CommonListItem = 1000;

    public LogsController(IWebHostEnvironment env, IConfiguration config)
    {
        _env = env;
        _config = config;
    }

    private string GetLogDirectory()
    {
        var configPath = _config["Serilog:WriteTo:1:Args:path"];
        if (!string.IsNullOrEmpty(configPath))
        {
            // Serilog path is often Like "App_Data/Logs/log-.txt", we need the directory
            var directory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(directory))
            {
                return Path.IsPathRooted(directory) 
                    ? directory 
                    : Path.Combine(_env.ContentRootPath, directory);
            }
        }
        return Path.Combine(_env.ContentRootPath, "App_Data", "Logs");
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] string? date = null)
    {
        var targetDate = DateTime.Today;
        if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsedDate))
        {
            targetDate = parsedDate;
        }

        var fileName = $"log-{targetDate:yyyyMMdd}.txt";
        var logPath = Path.Combine(GetLogDirectory(), fileName);

        if (!System.IO.File.Exists(logPath))
        {
            return NotFound(new ApiBadRequestResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND, $"Log file not found for date: {targetDate:yyyy-MM-dd}"));
        }

        try
        {
            using (var fileStream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream, Encoding.UTF8))
            {
                var content = await reader.ReadToEndAsync();
                return Ok(new ApiOkResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_GET_ITEM, content));
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiBadRequestResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_GET_EXCEPTION, $"Error reading logs: {ex.Message}"));
        }
    }

    [HttpGet("files")]
    public IActionResult GetLogFiles()
    {
        var logDir = GetLogDirectory();
        if (!Directory.Exists(logDir))
        {
             return Ok(new ApiOkResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_LIST_ALL_ITEMS, new List<LogFileInfo>()));
        }

        var files = Directory.GetFiles(logDir, "log-*.txt")
                             .Select(f => new FileInfo(f))
                             .OrderByDescending(f => f.CreationTime)
                             .Select(f => new LogFileInfo
                             {
                                 Name = f.Name,
                                 SizeBytes = f.Length,
                                 LastModified = f.LastWriteTime
                             })
                             .ToList();

        return Ok(new ApiOkResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_LIST_ALL_ITEMS, files));
    }

    [HttpPost("export")]
    public async Task<IActionResult> ExportLogs([FromBody] LogExportRequest request)
    {
        if (request.FileNames == null || !request.FileNames.Any())
        {
             return BadRequest(new ApiBadRequestResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND, "No files selected."));
        }

        var logDir = GetLogDirectory();
        var memoryStream = new MemoryStream();

        try
        {
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var fileName in request.FileNames)
                {
                    if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\")) continue;

                    var filePath = Path.Combine(logDir, fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        var entry = archive.CreateEntry(fileName);
                        using (var entryStream = entry.Open())
                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            await fileStream.CopyToAsync(entryStream);
                        }
                    }
                }
            }
            
            memoryStream.Position = 0;
            return File(memoryStream, "application/zip", $"logs-export-{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
        }
        catch (Exception ex)
        {
             return BadRequest(new ApiBadRequestResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_GET_EXCEPTION, $"Export failed: {ex.Message}"));
        }
    }

    [HttpDelete]
    public IActionResult CleanupLogs([FromBody] LogCleanupRequest request)
    {
        var logDir = GetLogDirectory();
        if (!Directory.Exists(logDir)) return Ok(new ApiOkResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_DELETE_ITEM, "No logs directory found."));

        int deletedCount = 0;

        try
        {
            if (request.OlderThan.HasValue)
            {
                var cutoff = request.OlderThan.Value;
                var files = Directory.GetFiles(logDir, "log-*.txt")
                                     .Select(f => new FileInfo(f))
                                     .Where(f => f.LastWriteTime < cutoff);
                
                foreach (var file in files)
                {
                    file.Delete();
                    deletedCount++;
                }
            }

            if (request.FileNames != null && request.FileNames.Any())
            {
                foreach (var fileName in request.FileNames)
                {
                    if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\")) continue;

                    var filePath = Path.Combine(logDir, fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                        deletedCount++;
                    }
                }
            }

            return Ok(new ApiOkResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_DELETE_ITEM, $"Deleted {deletedCount} log files."));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiBadRequestResponse(EnumEntityType.SETTING, EnumEntityEvents.COMMON_DELETE_EXCEPTION, $"Cleanup failed: {ex.Message}"));
        }
    }
}

public class LogFileInfo
{
    public string Name { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime LastModified { get; set; }
}

public class LogExportRequest
{
    public List<string> FileNames { get; set; } = new();
}

public class LogCleanupRequest
{
    public List<string>? FileNames { get; set; }
    public DateTime? OlderThan { get; set; }
}
