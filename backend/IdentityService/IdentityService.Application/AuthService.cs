using AutoMapper;
using IdentityService.Application.Abstrations;
using IdentityService.Application.AppExceptions;
using IdentityService.Application.Constants;
using IdentityService.Application.Dtos.Auth.Request;
using IdentityService.Application.Dtos.Auth.Response;
using IdentityService.Application.Helpers;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IdentityService.Application
{

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly IMapper _mapper;
        private readonly ITokenBlacklist _tokenBlacklist;

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IRefreshTokenRepository refreshTokenRepository,
            JwtHelper jwtHelper,
            IMapper mapper,
            ITokenBlacklist tokenBlacklist)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtHelper = jwtHelper;
            _mapper = mapper;
            _tokenBlacklist = tokenBlacklist;
        }

        public async Task<LoginRep> LoginAsync(LoginReq request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password");

            if (user.IsBlocked == true)
                throw new UnauthorizedAccessException("Account is blocked");

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                throw new UnauthorizedAccessException("Invalid email or password");

            // Generate tokens
            var roleName = user.Role?.Name;
            var accessToken = _jwtHelper.GenerateAccessToken(user.Id);
            var (refreshTokenValue, refreshTokenExpiry) = _jwtHelper.GenerateRefreshToken(user.Id);

            // Save refresh token
            var refreshToken = new RefreshToken
            {
                Token = refreshTokenValue,
                UserId = user.Id,
                IssuedAt = DateTimeOffset.UtcNow,
                ExpiresAt = new DateTimeOffset(refreshTokenExpiry, TimeSpan.Zero),
                IsRevoked = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshToken);

            var userRep = _mapper.Map<Application.Dtos.User.Respone.UserRep>(user);

            return new LoginRep
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                User = userRep
            };
        }

        public async Task<bool> LogoutAsync(string refreshToken, string? accessToken = null)
        {
            // Validate JWT refresh token 
            var principal = _jwtHelper.ValidateToken(refreshToken);
            if (principal == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? principal.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new UnauthorizedAccessException("Invalid token format");
            }

            // Check token type : refToken
            var tokenType = principal.FindFirst("type")?.Value;
            if (tokenType != "refresh")
            {
                throw new UnauthorizedAccessException("Token is not a refresh token");
            }

            // Get token from database
            var token = await _refreshTokenRepository.GetByTokenIncludeRevokedAsync(refreshToken);
            if (token == null)
            {
                throw new UnauthorizedAccessException("Refresh token not found");
            }

            if (token.UserId != userId)
            {
                throw new UnauthorizedAccessException("Token does not belong to user");
            }

            // Check token is already revoked
            if (token.IsRevoked == true)
            {
                // Token đã revoked, nhưng vẫn có thể blacklist access token nếu có
                if (!string.IsNullOrEmpty(accessToken))
                {
                    await BlacklistAccessTokenIfValid(accessToken);
                }
                return false;
            }

            // Check token expired
            if (token.ExpiresAt < DateTimeOffset.UtcNow)
            {
                token.IsRevoked = true;
                token.UpdatedAt = DateTimeOffset.UtcNow;
                await _refreshTokenRepository.SaveChangesAsync();

                // Vẫn có thể blacklist access token nếu có
                if (!string.IsNullOrEmpty(accessToken))
                {
                    await BlacklistAccessTokenIfValid(accessToken);
                }
                return false;
            }

            // Revoke the refresh token
            token.IsRevoked = true;
            token.UpdatedAt = DateTimeOffset.UtcNow;
            await _refreshTokenRepository.SaveChangesAsync();

            // Blacklist access token nếu có
            if (!string.IsNullOrEmpty(accessToken))
            {
                await BlacklistAccessTokenIfValid(accessToken);
            }

            return true;
        }

        public async Task<RefreshTokenRep> RefreshTokenAsync(string refreshToken)
        {
            var principal = _jwtHelper.ValidateToken(refreshToken);
            if (principal == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? principal.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new UnauthorizedAccessException("Invalid token format");
            }


            var tokenType = principal.FindFirst("type")?.Value;
            if (tokenType != "refresh")
            {
                throw new UnauthorizedAccessException("Token is not a refresh token");
            }


            var token = await _refreshTokenRepository.GetByTokenIncludeRevokedAsync(refreshToken);
            if (token == null)
            {
                throw new UnauthorizedAccessException("Refresh token not found");
            }

            // kiểm tra xem token đã bị revoked chưa 
            if (token.IsRevoked == true)
            {
                // nếu phát hiện token bị reuse -> m cúc liền 
                await _refreshTokenRepository.RevokeAllUserTokensAsync(userId);
                throw new UnauthorizedAccessException("Refresh token has been revoked. All tokens have been invalidated for security.");
            }

            if (token.UserId != userId)
            {
                throw new UnauthorizedAccessException("Token does not belong to user");
            }

            // double check
            if (token.IsRevoked == true)
            {
                throw new UnauthorizedAccessException("Refresh token has been revoked");
            }

            if (token.ExpiresAt < DateTimeOffset.UtcNow)
            {
                token.IsRevoked = true;
                token.UpdatedAt = DateTimeOffset.UtcNow;
                await _refreshTokenRepository.SaveChangesAsync();
                throw new UnauthorizedAccessException("Refresh token has expired");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            if (user.IsBlocked == true)
            {
                throw new UnauthorizedAccessException("Account is blocked");
            }

            // revoke Ref Token cũ - Rotation
            token.IsRevoked = true;
            token.UpdatedAt = DateTimeOffset.UtcNow;
            await _refreshTokenRepository.SaveChangesAsync();

            // Gen new token
            var newAccessToken = _jwtHelper.GenerateAccessToken(userId);
            var (newRefreshTokenValue, newRefreshTokenExpiry) = _jwtHelper.GenerateRefreshToken(userId);

            var newRefreshToken = new RefreshToken
            {
                Token = newRefreshTokenValue,
                UserId = userId,
                IssuedAt = DateTimeOffset.UtcNow,
                ExpiresAt = new DateTimeOffset(newRefreshTokenExpiry, TimeSpan.Zero),
                IsRevoked = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _refreshTokenRepository.AddAsync(newRefreshToken);

            // Return new access token + new refresh token (rotated)
            return new RefreshTokenRep
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenValue
            };
        }

        private async Task BlacklistAccessTokenIfValid(string accessToken)
        {
            try
            {
                // Lấy expiry time từ token
                var tokenExpiry = _jwtHelper.GetTokenExpiry(accessToken);
                if (tokenExpiry.HasValue && tokenExpiry.Value > DateTime.UtcNow)
                {
                    // Chỉ blacklist nếu token chưa hết hạn
                    await _tokenBlacklist.AddToBlacklistAsync(
                        accessToken,
                        new DateTimeOffset(tokenExpiry.Value)
                    );
                }
            }
            catch
            {
                // Nếu không parse được token, bỏ qua (token có thể invalid)
            }
        }

        public async Task<RegisterRep> RegisterAsync(RegisterReq request)
        {
            // check mail
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new ConflictDuplicateException($"Email '{request.Email}' already exists");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            Role? role = await _roleRepository.GetByNameAsync("User");
            if (role == null)
            {
                throw new ForbidenException(Message.RoleMessage.NotFound);
            }
            var user = new User
            {
                Email = request.Email,
                Password = hashedPassword,
                Phone = request.Phone,
                Address = request.Address,
                RoleId = role.Id, // Set default role as User
                IsBlocked = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _userRepository.AddWithRoleAsync(user);

            return new RegisterRep();
        }
    }
}
