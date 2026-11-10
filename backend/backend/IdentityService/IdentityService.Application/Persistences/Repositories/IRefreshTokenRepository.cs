using IdentityService.Domain.Entities;

namespace IdentityService.Application.Persistences.Repositories;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetRefreshTokenByAccountIdAsync(Guid Id);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken?> GetByTokenIncludeRevokedAsync(string token);
    Task RevokeAllUserTokensAsync(Guid userId);
    Task SaveChangesAsync();
}

