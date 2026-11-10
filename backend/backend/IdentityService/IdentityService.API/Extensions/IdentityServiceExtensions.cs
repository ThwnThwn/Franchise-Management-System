using System.Text;
using IdentityService.Application.Helpers;
using IdentityService.Application.Persistences.Caches;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.API.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddJwtIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET is not configured");
        var issuer = configuration["JWT_ISSUER"] ?? "CFSM-be";
        var audience = configuration["JWT_AUDIENCE"] ?? "CFSM-be";

        services.AddSingleton(new JwtHelper(
            secretKey, issuer, audience,
            int.Parse(configuration["JWT_ACCESS_TOKEN_EXPIRATION_MINUTES"] ?? "15"),
            int.Parse(configuration["JWT_REFRESH_TOKEN_EXPIRATION_DAYS"] ?? "7")
        ));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = CreateJwtEvents();
            });

        return services;
    }

    private static JwtBearerEvents CreateJwtEvents() => new()
    {
        OnMessageReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault()
                          ?? context.Request.Headers["authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader))
            {
                logger.LogWarning("No Authorization metadata/header found in gRPC request");
                return Task.CompletedTask;
            }

            context.Token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authHeader[7..].Trim() : authHeader.Trim();

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "JWT Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT Challenge: {Error}, {ErrorDescription}", context.Error, context.ErrorDescription);
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var tokenBlacklist = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklist>();

            var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()
                          ?? context.HttpContext.Request.Headers["authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader))
            {
                var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authHeader[7..].Trim() : authHeader.Trim();

                if (await tokenBlacklist.IsTokenBlacklistedAsync(token))
                {
                    logger.LogWarning("Token is blacklisted");
                    context.Fail("Token has been revoked");
                }
            }
        }
    };
}