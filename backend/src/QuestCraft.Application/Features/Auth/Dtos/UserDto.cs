namespace QuestCraft.Application.Features.Auth.Dtos;

public record UserDto(
    int Id,
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string? AvatarUrl,
    int Xp,
    int Coins,
    int Level);
