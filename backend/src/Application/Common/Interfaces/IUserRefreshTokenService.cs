using Crm.Domain.Entities;

namespace Crm.Application.Common.Interfaces;

public interface IUserRefreshTokenService
{
    Task<UserRefreshToken?> GetRefreshToken(string refreshToken);
    Task<UserRefreshToken> CreateRefreshToken(UserRefreshToken record);
    Task RemoveRefreshToken(string refreshToken);
    Task RemoveExpiredTokens(long userId);
    Task<UserRefreshToken?> RefreshToken(UserRefreshToken existingToken, string newRefreshToken, DateTime? validTill);
}
