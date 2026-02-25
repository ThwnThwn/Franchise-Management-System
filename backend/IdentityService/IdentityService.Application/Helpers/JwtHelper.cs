using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Helpers
{
    public class JwtHelper
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpirationMinutes;
        private readonly int _refreshTokenExpirationDays;

        public JwtHelper(string secretKey, string issuer, string audience, int accessTokenExpirationMinutes = 60, int refreshTokenExpirationDays = 7)
        {
            _secretKey = secretKey;
            _issuer = issuer;
            _audience = audience;
            _accessTokenExpirationMinutes = accessTokenExpirationMinutes;
            _refreshTokenExpirationDays = refreshTokenExpirationDays;
        }

        public string GenerateAccessToken(Guid userId, string roleSlug, Guid? storeId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.Role, roleSlug),
                new Claim("role_slug", roleSlug),
                new Claim("type", "access")
            };
            if (storeId != null)
            {
                claims.Add(new Claim("store_id", storeId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateForgetPasswordToken(string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, email),
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim("type", "forget-password")
        };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public (string RefreshToken, DateTime RefreshTokenExpiration) GenerateRefreshToken(Guid userId)
        {
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var refreshTokenClaims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("type", "refresh")
        };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(refreshTokenClaims),
                Expires = refreshTokenExpiration,
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var refreshToken = tokenHandler.CreateToken(tokenDescriptor);
            var refreshTokenString = tokenHandler.WriteToken(refreshToken);

            return (refreshTokenString, refreshTokenExpiration);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public (Guid? UserId, string? RoleSlug) GetUserInfoFromToken(ClaimsPrincipal principal)
        {
            var userIdValue =
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            Guid? userId = null;
            if (!string.IsNullOrEmpty(userIdValue) && Guid.TryParse(userIdValue, out var parsed))
                userId = parsed;

            var roleSlug =
                principal.FindFirst("role_slug")?.Value
                ?? principal.FindFirst(ClaimTypes.Role)?.Value;

            return (userId, roleSlug);
        }

        /// <summary>
        /// Lấy thời gian hết hạn từ JWT token (không validate, chỉ đọc)
        /// </summary>
        /// <param name="token">JWT token string</param>
        /// <returns>DateTime? - Thời gian hết hạn của token, null nếu không parse được</returns>
        public DateTime? GetTokenExpiry(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                // Read token without validation (chỉ để lấy expiry)
                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo;
            }
            catch
            {
                return null;
            }
        }
    }
}