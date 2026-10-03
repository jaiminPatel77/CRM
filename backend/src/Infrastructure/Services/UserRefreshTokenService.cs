using Crm.Application.Common.Interfaces;
using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Services;

public class UserRefreshTokenService : IUserRefreshTokenService
{
    private readonly IApplicationDbContext _context;

    public UserRefreshTokenService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserRefreshToken?> GetRefreshToken(string refreshToken)
    {
        var token = await _context.UserRefreshTokens
            .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken);

        if (token != null && token.ValidTill.HasValue && token.ValidTill < DateTimeOffset.UtcNow)
        {
            // Expired, delete it
            _context.UserRefreshTokens.Remove(token);
            await _context.SaveChangesAsync(CancellationToken.None);
            return null;
        }

        return token;
    }

    public async Task<UserRefreshToken> CreateRefreshToken(UserRefreshToken record)
    {
        _context.UserRefreshTokens.Add(record);
        await _context.SaveChangesAsync(CancellationToken.None);
        return record;
    }

    public async Task RemoveRefreshToken(string refreshToken)
    {
        var token = await _context.UserRefreshTokens
            .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken);
        
        if (token != null)
        {
            _context.UserRefreshTokens.Remove(token);
            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }

    public async Task RemoveExpiredTokens(long userId)
    {
        var expired = await _context.UserRefreshTokens
            .Where(t => t.UserId == userId && t.ValidTill < DateTimeOffset.UtcNow)
            .ToListAsync();

        if (expired.Any())
        {
            _context.UserRefreshTokens.RemoveRange(expired);
            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }

    public async Task<UserRefreshToken?> RefreshToken(UserRefreshToken existingToken, string newRefreshToken, DateTime? validTill)
    {
        // Delete old
        _context.UserRefreshTokens.Remove(existingToken);
        
        // Create new
        var newToken = new UserRefreshToken
        {
            UserId = existingToken.UserId,
            RefreshToken = newRefreshToken,
            DeviceId = existingToken.DeviceId,
            ValidTill = validTill,
            CreatedOn = DateTimeOffset.UtcNow,
            CreatedByIp = existingToken.CreatedByIp // Carry over IP or we could fetch new one if passed
        };

        _context.UserRefreshTokens.Add(newToken);
        await _context.SaveChangesAsync(CancellationToken.None);
        
        return newToken;
    }
}
