using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Auth.Dtos;

namespace QuestCraft.Application.Features.Auth.Commands.Login;

public record LoginCommand(string EmailOrUsername, string Password) : ICommand<AuthResponseDto>;
