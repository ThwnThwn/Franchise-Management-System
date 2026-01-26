using AutoMapper;
using IdentityService.Application.Dtos.Auth.Response;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.Helpers;
using MediatR;

namespace IdentityService.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginRep>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtHelper _jwtHelper;
    private readonly IMapper _mapper;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        JwtHelper jwtHelper,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtHelper = jwtHelper;
        _mapper = mapper;
    }

    public async Task<LoginRep> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailOrUserNameAsync(request.Request.EmailOrUsername);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email/username or password");

        if (user.IsBlocked == true)
            throw new UnauthorizedAccessException("Account is blocked");

        if(user.Role == null)
            throw new UnauthorizedAccessException("Account's role is null");

        if (!BCrypt.Net.BCrypt.Verify(request.Request.Password, user.Password))
            throw new UnauthorizedAccessException("Wrong password");

        var accessToken = _jwtHelper.GenerateAccessToken(user.Id, user.Role.Slug);
        var (refreshTokenValue, refreshTokenExpiry) = _jwtHelper.GenerateRefreshToken(user.Id);

        await _refreshTokenRepository.AddAsync(new Domain.Entities.RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = new DateTimeOffset(refreshTokenExpiry, TimeSpan.Zero),
            IsRevoked = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        var userRep = _mapper.Map<Dtos.User.Respone.UserRep>(user);

        return new LoginRep
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            User = userRep
        };
    }
}
