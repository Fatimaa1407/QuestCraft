using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Common.Interfaces;

public record AccessTokenResult(string Token, DateTime ExpiresAtUtc);

public record RefreshTokenResult(string Token, DateTime ExpiresAtUtc);

public interface IJwtTokenService
{
    AccessTokenResult GenerateAccessToken(User user);
    RefreshTokenResult GenerateRefreshToken();
}
