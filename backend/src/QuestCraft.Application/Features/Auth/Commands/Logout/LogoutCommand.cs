using MediatR;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : ICommand<Unit>;
