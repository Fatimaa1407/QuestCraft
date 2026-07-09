using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Auth.Dtos;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            throw new ConflictException("Bu email artıq istifadə olunur.");
        }

        if (await _context.Users.AnyAsync(u => u.Username == request.Username, cancellationToken))
        {
            throw new ConflictException("Bu istifadəçi adı artıq istifadə olunur.");
        }

        // Public register endpoint always assigns the Student role — RoleId is never client-supplied to prevent privilege escalation.
        var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student", cancellationToken)
            ?? throw new NotFoundException(nameof(Role), "Student");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            RoleId = studentRole.Id,
            Role = studentRole,
            Profile = new UserProfile(),
            Statistics = new UserStatistics(),
            Streak = new Streak(),
        };

        _context.Users.Add(user);

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
