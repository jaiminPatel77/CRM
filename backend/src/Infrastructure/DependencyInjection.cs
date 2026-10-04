using Crm.Application.Common.Interfaces;
using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Interceptors;
using Crm.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Crm.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? configuration.GetConnectionString("PostgresConnection");
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");

        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            if (useInMemory)
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            }
            else
            {
                options.UseNpgsql(connectionString, builder => 
                    builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
                           .EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null));
            }
            options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        var jwtSection = configuration.GetSection("Authentication:JwtIssuerOptions");
        services.Configure<Crm.Application.Common.Models.JwtIssuerOptions>(jwtSection);

        var secretKey = jwtSection["SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("Authentication:JwtIssuerOptions:SecretKey configuration is missing or empty.");
        }

        var signingKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
        var jwtOptions = jwtSection.Get<Crm.Application.Common.Models.JwtIssuerOptions>() ?? new Crm.Application.Common.Models.JwtIssuerOptions();
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization(options =>
        {
            foreach (var permission in Crm.Application.Common.Security.Permissions.GetAll())
            {
                options.AddPolicy(permission, policy => policy.RequireAuthenticatedUser());
            }
        });

        // Add Services
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IUserRefreshTokenService, UserRefreshTokenService>();
        services.AddScoped<IAuthService, AuthService>();
            
        services.AddIdentityCore<User>()
            .AddRoles<Role>()
            .AddSignInManager()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddHangfire((sp, config) => 
        {
            var env = sp.GetService<Microsoft.Extensions.Hosting.IHostEnvironment>();
            var isInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");
            var connStr = configuration.GetConnectionString("DefaultConnection") 
                ?? configuration.GetConnectionString("PostgresConnection");

            config.UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings();

            if (isInMemory)
            {
                if (env != null && !env.IsDevelopment() && !env.IsEnvironment("Testing"))
                {
                    throw new InvalidOperationException("Hangfire in-memory storage fallback is strictly forbidden outside Development and Testing environments.");
                }
                config.UseMemoryStorage();
            }
            else
            {
                config.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connStr));
            }
        });

        services.AddHangfireServer();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddTransient<IEmailService, SmtpEmailService>();
        services.AddTransient<IEmailSender, SmtpEmailService>();

        // Storage
        var storageType = configuration["Storage:Type"];
        if (storageType?.Equals("S3", StringComparison.OrdinalIgnoreCase) == true)
        {
            var awsOptions = configuration.GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<Amazon.S3.IAmazonS3>();
            services.AddTransient<IFileStorageService, S3StorageService>();
        }
        else if (storageType?.Equals("Azure", StringComparison.OrdinalIgnoreCase) == true)
        {
            services.AddTransient<IFileStorageService, AzureStorageService>();
        }
        else
        {
            services.AddTransient<IFileStorageService, FileSystemStorageService>();
        }
        services.AddScoped<IBackupService, BackupService>();
        services.AddHttpClient<IReCaptchaService, GoogleReCaptchaService>();

        services.AddTransient<IDateTime, DateTimeService>();
        
        var healthChecks = services.AddHealthChecks();
        if (!useInMemory)
        {
            healthChecks.AddDbContextCheck<ApplicationDbContext>();
        }
        healthChecks
            .AddDiskStorageHealthCheck(s => 
            {
                var drive = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) ? "C:\\" : "/";
                s.AddDrive(drive, 1024);
            })
            .AddPrivateMemoryHealthCheck(512 * 1024 * 1024);

        services.AddSingleton<IHealthCheckPublisher, HealthCheckPushService>();
        
        // Data Seeding
        services.AddScoped<ApplicationDbContextInitialiser>();

        return services;
    }
}
