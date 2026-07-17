using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Auth.Dtos;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == request.EmailOrUsername || u.Username == request.EmailOrUsername, cancellationToken);

        // Same generic message whether the identifier doesn't exist or the password is wrong — avoids leaking which one.
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Email/istifadəçi adı və ya şifrə yanlışdır.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("Hesabınız deaktiv edilib.");
        }

        user.LastLoginAt = DateTime.UtcNow;

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            User = user,
            Token = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAtUtc,
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshToken.Token,
            refreshToken.ExpiresAtUtc,
            UserDtoMapper.ToDto(user));
    }
}
