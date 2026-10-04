using System.Threading.RateLimiting;
using Asp.Versioning;
using Crm.Api.Middleware;
using Crm.Application;
using Crm.Infrastructure;
using Crm.Infrastructure.Persistence;
using Hangfire;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services to the container.

// Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Add Layer Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHybridCache();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

// Controllers & OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi(); 

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// Native ASP.NET Core Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    });
});

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Crm.Api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("Crm")
        .AddConsoleExporter());

// Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = new[] { "application/json", "application/xml", "text/json", "text/xml" };
});

// API Response Caching
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.With(c => true).Cache());
});

var app = builder.Build();

// Input Sanitization
app.UseInputSanitization();

// Response Compression
app.UseResponseCompression();

// Output Caching
app.UseOutputCache();

// 2. Configure the HTTP request pipeline.

// Initialise Database
using (var scope = app.Services.CreateScope())
{
    var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
    await initialiser.InitialiseAsync();
    await initialiser.SeedAsync();
}

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("AllowDocumentation"))
{
    app.UseOpenApiSecurity();
    app.MapOpenApi();
    
    app.MapScalarApiReference(options =>
    {
        options.Title = "CRM API";
        options.Theme = ScalarTheme.BluePlanet;
#pragma warning disable CS0618
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecurityScheme = "Bearer"
        };
#pragma warning restore CS0618
    });
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Global Exception Handler
app.UseGlobalExceptionHandler();

// Security Headers
app.UseSecurityHeaders();

// Built-in Rate Limiter Middleware
app.UseRateLimiter();

app.UseAuthentication();
app.UseMiddleware<Crm.Api.Middleware.TenantResolutionMiddleware>();
app.UseAuthorization();

// Hangfire Dashboard (Secured)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new [] { new Crm.Infrastructure.BackgroundJobs.HangfireAuthorizationFilter() }
});

if (!app.Environment.IsDevelopment())
{
    RecurringJob.AddOrUpdate<Crm.Application.Common.Interfaces.IBackupService>(
        "DailyDatabaseBackup", 
        service => service.BackupDatabaseAsync(), 
        Cron.Daily);
}
else
{
    RecurringJob.RemoveIfExists("DailyDatabaseBackup");
}

app.MapHealthChecks("/health");
app.MapControllers();

app.MapGet("/", () => "CRM API Running!");

app.Run();

public partial class Program { }
