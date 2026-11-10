using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.ApplicationDbContext;
using IdentityService.Infrastructure.Persistences.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistences.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(IIdentityDbContext dbContext) : base(dbContext)
    {
    }
    /// <summary>
    /// Gets the refresh token by account identifier asynchronous.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<RefreshToken?> GetRefreshTokenByAccountIdAsync(Guid Id)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Gets the by token asynchronous.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns></returns>
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token && rt.IsRevoked != true);
    }
    /// <summary>
    /// Gets the by token include revoked asynchronous.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns></returns>
    public async Task<RefreshToken?> GetByTokenIncludeRevokedAsync(string token)
    {
        return await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }
    /// <summary>
    /// Revokes all user tokens asynchronous.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.IsRevoked != true)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.UpdatedAt = DateTimeOffset.UtcNow;
        }

        if (tokens.Any())
        {
            await _dbContext.SaveChangesAsync();
        }
    }
    /// <summary>
    /// Saves the changes asynchronous.
    /// </summary>
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}