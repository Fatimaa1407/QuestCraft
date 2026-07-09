using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Auth.Dtos;

namespace QuestCraft.Application.Features.Auth.Commands.Refresh;

public record RefreshTokenCommand(string RefreshToken) : ICommand<AuthResponseDto>;
