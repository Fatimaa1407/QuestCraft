using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Auth.Dtos;

internal static class UserDtoMapper
{
    public static UserDto ToDto(User user) => new(
        user.Id,
        user.Username,
        user.Email,
        user.Role.Name,
        user.Profile?.AvatarUrl,
        user.Profile?.Xp ?? 0,
        user.Profile?.Coins ?? 0,
        user.Profile?.Level ?? 1);
}
