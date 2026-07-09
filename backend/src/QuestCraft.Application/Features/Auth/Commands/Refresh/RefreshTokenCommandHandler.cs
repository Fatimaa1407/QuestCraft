using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Auth.Dtos;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Auth.Commands.Refresh;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenCommandHandler(IApplicationDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await _context.RefreshTokens
            .Include(rt => rt.User).ThenInclude(u => u.Role)
            .Include(rt => rt.User).ThenInclude(u => u.Profile)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (existingToken is null || existingToken.IsRevoked || existingToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh token etibarsızdır və ya vaxtı bitib.");
        }

        // Rotation: the used token is revoked immediately so it can never be replayed.
        existingToken.IsRevoked = true;

        var accessToken = _jwtTokenService.GenerateAccessToken(existingToken.User);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            User = existingToken.User,
            Token = newRefreshToken.Token,
            ExpiresAt = newRefreshToken.ExpiresAtUtc,
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            newRefreshToken.Token,
            newRefreshToken.ExpiresAtUtc,
            UserDtoMapper.ToDto(existingToken.User));
    }
}
