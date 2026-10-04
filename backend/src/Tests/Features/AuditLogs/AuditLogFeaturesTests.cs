using Crm.Application.Common.Interfaces;
using Crm.Application.Features.AuditLogs;
using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Crm.Tests.Features.AuditLogs;

public class AuditLogFeaturesTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public AuditLogFeaturesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var interMock = Substitute.For<AuditableEntityInterceptor>(Substitute.For<ICurrentUserService>(), Substitute.For<IDateTime>());
        _context = new ApplicationDbContext(options, interMock);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetAuditLogsWithPagination_ShouldReturnLogs_WhenLogsExist()
    {
        // Arrange
        _context.AuditLogs.Add(new AuditLog { TableName = "Users", Type = "Create", PrimaryKey = "1", DateTime = DateTime.Now });
        _context.AuditLogs.Add(new AuditLog { TableName = "Projects", Type = "Update", PrimaryKey = "2", DateTime = DateTime.Now });
        await _context.SaveChangesAsync();

        var handler = new GetAuditLogsWithPaginationQueryHandler(_context);
        var query = new GetAuditLogsWithPaginationQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Data!.Data.Count());
        Assert.Contains(result.Data.Data, l => l.TableName == "Users");
        Assert.Contains(result.Data.Data, l => l.TableName == "Projects");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
