using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Auth.Dtos;

namespace QuestCraft.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string Username, string FirstName, string LastName, string Email, string Password) : ICommand<UserDto>;
