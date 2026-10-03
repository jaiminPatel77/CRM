using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Respawn;
using Respawn.Graph;
using System.Data.Common;

namespace Crm.Tests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? _postgreSqlContainer;
    private Respawner? _respawner;
    private DbConnection? _dbConnection;
    private bool _useDocker = false;

    public string DefaultUserId { get; set; } = "1";
    public string TestLogsDir { get; } = Path.Combine(AppContext.BaseDirectory, "TestLogs", Guid.NewGuid().ToString());

    public async Task InitializeAsync()
    {
        if (!Directory.Exists(TestLogsDir))
        {
            Directory.CreateDirectory(TestLogsDir);
        }

        try 
        {
            _postgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("crm_db_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();
            await _postgreSqlContainer.StartAsync();
            _useDocker = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Docker PostgreSQL not available, falling back to InMemory. Error: " + ex.Message);
            _useDocker = false;
        }
    }

    public async Task RespawnerResetAsync()
    {
        if (_useDocker && _dbConnection != null && _respawner != null)
        {
            await _respawner.ResetAsync(_dbConnection);
        }
    }

    public new async Task DisposeAsync()
    {
        if (_dbConnection != null) await _dbConnection.DisposeAsync();
        if (_postgreSqlContainer != null) await _postgreSqlContainer.DisposeAsync();

        try
        {
            if (Directory.Exists(TestLogsDir))
            {
                Directory.Delete(TestLogsDir, true);
            }
        }
        catch
        {
            // Best effort cleanup
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testConfig = new Dictionary<string, string?>();
            testConfig["Serilog:WriteTo:1:Args:path"] = Path.Combine(TestLogsDir, "log-.txt");

            if (_useDocker && _postgreSqlContainer != null)
            {
                testConfig["DatabaseProvider"] = "PostgreSQL";
                testConfig["ConnectionStrings:DefaultConnection"] = _postgreSqlContainer.GetConnectionString();
                testConfig["ConnectionStrings:PostgresConnection"] = _postgreSqlContainer.GetConnectionString();
            }
            else
            {
                testConfig["UseInMemoryDatabase"] = "true";
            }

            config.AddInMemoryCollection(testConfig);
        });

        builder.ConfigureTestServices(services =>
        {
            if (!_useDocker)
            {
                // Remove existing DbContext
                var descriptors = services.Where(d => d.ServiceType.Name.Contains("DbContextOptions") || d.ServiceType == typeof(ApplicationDbContext)).ToList();
                foreach (var d in descriptors)
                {
                    services.Remove(d);
                }

                // Fallback to InMemory
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                    options.UseInMemoryDatabase("IntegrationTestsDb");
                });
            }

            // Replace Auth with Test Auth
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
                "Test", options => { });
            
            services.PostConfigure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options => 
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            });

            // Initialize DB and Respawn
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                try 
                {
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                    
                    if (_useDocker)
                    {
                        db.Database.Migrate();
                        _dbConnection = db.Database.GetDbConnection();
                        _dbConnection.Open();

                        var respawnerOptions = new RespawnerOptions
                        {
                            DbAdapter = DbAdapter.Postgres,
                            SchemasToInclude = new[] { "public" }
                        };

                        _respawner = Respawner.CreateAsync(_dbConnection, respawnerOptions).GetAwaiter().GetResult();
                    }
                    else
                    {
                        db.Database.EnsureCreated();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DB Initialization Error: {ex.Message}");
                }
            }
        });
    }
}
