using Crm.Domain.Consts;
using Crm.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Crm.Infrastructure.Persistence;

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public ApplicationDbContextInitialiser(
        ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (_context.Database.IsRelational())
            {
                await _context.Database.EnsureCreatedAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default Roles
        var adminRoleName = "GlobalAdministrator";

        // Ensure Admin Role
        if (_roleManager.Roles.All(r => r.Name != adminRoleName))
        {
            await _roleManager.CreateAsync(new Role 
            { 
                Name = adminRoleName, 
                Description = "Global Administrator with full access",
                RoleType = EnumRoleType.GlobalAdministrator,
                CreatedOn = DateTimeOffset.UtcNow,
                ModifiedOn = DateTimeOffset.UtcNow
            });
        }

        // Users
        var developerEmail = "developer@crm.local";
        var adminEmail = "admin@crm.local";
        var defaultPassword = "Test@123";

        // 1. Developer User
        if (_userManager.Users.All(u => u.UserName != developerEmail))
        {
            var developer = new User
            {
                UserName = developerEmail,
                Email = developerEmail,
                FullName = "Developer",
                UserType = EnumUserType.GlobalAdministrator,
                Status = EnumUserStatus.Created,
                CreatedOn = DateTimeOffset.UtcNow,
                ModifiedOn = DateTimeOffset.UtcNow,
                EmailConfirmed = true
            };

            await _userManager.CreateAsync(developer, defaultPassword);
            await _userManager.AddToRoleAsync(developer, adminRoleName);
        }

        // 2. Admin User
        if (_userManager.Users.All(u => u.UserName != adminEmail))
        {
            var admin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrator",
                UserType = EnumUserType.GlobalAdministrator,
                Status = EnumUserStatus.Created,
                CreatedOn = DateTimeOffset.UtcNow,
                ModifiedOn = DateTimeOffset.UtcNow,
                EmailConfirmed = true
            };

            await _userManager.CreateAsync(admin, defaultPassword);
            await _userManager.AddToRoleAsync(admin, adminRoleName);
        }
    }
}
