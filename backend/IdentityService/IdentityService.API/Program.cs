using DotNetEnv;
using IdentityService.API.Interceptors;
using IdentityService.API.Services;
using IdentityService.Application;
using IdentityService.Application.Abstrations;
using IdentityService.Application.Helpers;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Grpc.Services;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.ApplicationDbContext;
using IdentityService.Infrastructure.Persistences.Cache;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

// Load .env
Env.Load("../../../.env");
builder.Configuration.AddEnvironmentVariables();
// gRPC
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection(); // optional

// ----- JWT Configuration -----
var jwtSecretKey = builder.Configuration["JWT_SECRET"]
    ?? throw new InvalidOperationException("JWT_SECRET is not configured");

var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? "CFSM-be";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? "CFSM-be";

var jwtAccessTokenExpirationMinutes =
    int.Parse(builder.Configuration["JWT_ACCESS_TOKEN_EXPIRATION_MINUTES"] ?? "15");

var jwtRefreshTokenExpirationDays =
    int.Parse(builder.Configuration["JWT_REFRESH_TOKEN_EXPIRATION_DAYS"] ?? "7");

builder.Services.AddSingleton<JwtHelper>(_ =>
    new JwtHelper(jwtSecretKey, jwtIssuer, jwtAudience,
        jwtAccessTokenExpirationMinutes, jwtRefreshTokenExpirationDays));

builder.Services.AddScoped<ITokenBlacklist, TokenBlacklist>();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader))
                authHeader = context.Request.Headers["authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader))
            {
                logger.LogWarning("No Authorization metadata/header found in gRPC request");
                return Task.CompletedTask;
            }

            var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authHeader.Substring(7).Trim()
                : authHeader.Trim();

            context.Token = token;

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
            logger.LogWarning("JWT Challenge: {Error}, {ErrorDescription}",
                context.Error, context.ErrorDescription);
            return Task.CompletedTask;
        },

        OnTokenValidated = async context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

            var tokenBlacklist = context.HttpContext.RequestServices
                .GetRequiredService<ITokenBlacklist>();

            var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader))
                authHeader = context.HttpContext.Request.Headers["authorization"].ToString();

            var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authHeader.Substring(7).Trim()
                : authHeader.Trim();

            if (!string.IsNullOrEmpty(token))
            {
                var isBlacklisted = await tokenBlacklist.IsTokenBlacklistedAsync(token);
                if (isBlacklisted)
                {
                    logger.LogWarning("Token is blacklisted");
                    context.Fail("Token has been revoked");
                }
            }
        }
    };
});
builder.Services.AddAuthorization();

// PostgreSQL
var connectionString =
    "Host=localhost;Port=5432;Database=identity_db;Username=admin;Password=12345";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IAppDbContext>(sp =>
    sp.GetRequiredService<AppDbContext>());

// Redis
var redisConn =
    Environment.GetEnvironmentVariable("REDIS_CONFIGURATION")
    ?? builder.Configuration["REDIS_CONFIGURATION"]
    ?? "localhost:6379,abortConnect=false";

//For IDistributedCache (AddStackExchangeRedisCache)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConn;
    options.InstanceName = "chb_redis";
});

//For IConnectionMultiplexer (used in TokenBlacklist, etc.)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect(redisConn);
});

// Application + Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
//Interceptors
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<GrpcExceptionInterceptor>();
});
// Force HTTP/2 port
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5002, listen =>
    {
        listen.Protocols = HttpProtocols.Http2;
    });
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<AuthGrpcService>();
app.MapGrpcService<UserGrpcService>();
app.MapGrpcService<RoleGrpcService>();

app.MapGrpcReflectionService(); // optional

app.Run();
