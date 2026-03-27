using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using IdentityService.Application.Helpers;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace IdentityService.Application.Tests.Features.Jwt
{
    public class JwtHelperTest
    {
        private readonly JwtHelper _jwtHelper;
        private const string SecretKey = "super_secret_key_must_be_long_enough_for_hmacsha256";
        private const string Issuer = "IdentityService";
        private const string Audience = "IdentityClient";

        public JwtHelperTest()
        {
            // Khởi tạo với các giá trị giả định
            _jwtHelper = new JwtHelper(SecretKey, Issuer, Audience, 60, 7);
        }

        [Fact]
        public void GenerateAccessToken_ShouldReturnValidToken()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleSlug = "admin";
            var storeId = Guid.NewGuid();

            // Act
            var token = _jwtHelper.GenerateAccessToken(userId, roleSlug, storeId);

            // Assert
            token.Should().NotBeNullOrEmpty();
            var principal = _jwtHelper.ValidateToken(token);
            principal.Should().NotBeNull();
            principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(userId.ToString());
            principal.FindFirst("role_slug")?.Value.Should().Be(roleSlug);
            principal.FindFirst("store_id")?.Value.Should().Be(storeId.ToString());
        }

        [Fact]
        public void GenerateForgetPasswordToken_ShouldReturnValidToken()
        {
            // Arrange
            var email = "test@example.com";

            // Act
            var token = _jwtHelper.GenerateForgetPasswordToken(email);

            // Assert
            token.Should().NotBeNullOrEmpty();
            var principal = _jwtHelper.ValidateToken(token);
            principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(email);
            principal.FindFirst("type")?.Value.Should().Be("forget-password");
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnTokenAndExpiration()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var (token, expiration) = _jwtHelper.GenerateRefreshToken(userId);

            // Assert
            token.Should().NotBeNullOrEmpty();
            expiration.Should().BeAfter(DateTime.UtcNow);

            var principal = _jwtHelper.ValidateToken(token);
            principal.Should().NotBeNull();
            principal!.FindFirst("type")?.Value.Should().Be("refresh");
        }

        [Fact]
        public void ValidateToken_ShouldReturnNull_WhenTokenIsInvalid()
        {
            // Act
            var result = _jwtHelper.ValidateToken("invalid.token.here");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetUserInfoFromToken_ShouldReturnCorrectData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleSlug = "customer";
            var token = _jwtHelper.GenerateAccessToken(userId, roleSlug, null);
            var principal = _jwtHelper.ValidateToken(token);

            // Act
            var (extractedUserId, extractedRoleSlug) = _jwtHelper.GetUserInfoFromToken(principal!);

            // Assert
            extractedUserId.Should().Be(userId);
            extractedRoleSlug.Should().Be(roleSlug);
        }

        [Fact]
        public void GetTokenExpiry_ShouldReturnCorrectDateTime()
        {
            // Arrange
            var token = _jwtHelper.GenerateAccessToken(Guid.NewGuid(), "user", null);

            // Act
            var expiry = _jwtHelper.GetTokenExpiry(token);

            // Assert
            expiry.Should().NotBeNull();
            expiry.Value.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public void GetTokenExpiry_ShouldReturnNull_WhenTokenIsMalformed()
        {
            // Act
            var expiry = _jwtHelper.GetTokenExpiry("not-a-jwt");

            // Assert
            expiry.Should().BeNull();
        }
    }
}