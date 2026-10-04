using Crm.Api.Models;
using Crm.Tests.Common;
using System.Net.Http.Json;
using System.Net;
using Xunit;
using Crm.Api.Controllers; // For DTOs
using Microsoft.Extensions.DependencyInjection; // Added

namespace Crm.Tests.Integration.Controllers;

// Helper for deserialization
public class TestApiResponse<T>
{
    [System.Text.Json.Serialization.JsonPropertyName("data")]
    public T Result { get; set; } = default!;
    // Add other props if needed like IsError, Message, etc.
}

public class LogsControllerTests : IntegrationTestBase, IAsyncLifetime
{
    private readonly string _logsDir;

    public LogsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _logsDir = factory.TestLogsDir;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        // Clear the directory before each test to ensure isolation
        if (Directory.Exists(_logsDir))
        {
            foreach (var file in Directory.GetFiles(_logsDir))
            {
                try { File.Delete(file); } catch { /* Ignore cleanup errors */ }
            }
        }
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    [Fact]
    public async Task GetLogFiles_ShouldReturnEmpty_WhenNoLogsExist()
    {
        var response = await _client.GetAsync("/api/v1/logs/files");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TestApiResponse<List<LogFileInfo>>>();
        Assert.NotNull(result);
        
        // Filter out today's log because Serilog creates it automatically on startup
        var todayPrefix = $"log-{DateTime.Today:yyyyMMdd}";
        var otherFiles = result!.Result.Where(f => !f.Name.StartsWith(todayPrefix)).ToList();
        
        Assert.Empty(otherFiles);
    }

    [Fact]
    public async Task GetLogFiles_ShouldReturnFiles_WhenLogsExist()
    {
        var fileName = "log-20250101.txt";
        await File.WriteAllTextAsync(Path.Combine(_logsDir, fileName), "Test Log Content");

        var response = await _client.GetAsync("/api/v1/logs/files");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"DEBUG JSON: {json}");
        var result = await response.Content.ReadFromJsonAsync<TestApiResponse<List<LogFileInfo>>>();
        Assert.Contains(result!.Result, f => f.Name == fileName);
    }

    [Fact]
    public async Task GetLogs_ShouldReturnContent_WhenLogExists()
    {
        var date = "2025-01-01";
        var fileName = "log-20250101.txt";
        var content = "Test Content";
        await File.WriteAllTextAsync(Path.Combine(_logsDir, fileName), content);

        var response = await _client.GetAsync($"/api/v1/logs?date={date}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TestApiResponse<string>>();
        Assert.Equal(content, result!.Result);
    }

    [Fact]
    public async Task CleanupLogs_ShouldDeleteOldFiles()
    {
        var oldFile = "log-20200101.txt";
        var newFile = "log-99991231.txt"; // Use a far future date to avoid today's lock
        
        await File.WriteAllTextAsync(Path.Combine(_logsDir, oldFile), "Old");
        File.SetLastWriteTime(Path.Combine(_logsDir, oldFile), DateTime.Today.AddDays(-10));

        await File.WriteAllTextAsync(Path.Combine(_logsDir, newFile), "New");

        var request = new LogCleanupRequest { OlderThan = DateTime.Today.AddDays(-1) };

        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/v1/logs")
        {
            Content = JsonContent.Create(request)
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.False(File.Exists(Path.Combine(_logsDir, oldFile)));
        Assert.True(File.Exists(Path.Combine(_logsDir, newFile)));
    }
}
