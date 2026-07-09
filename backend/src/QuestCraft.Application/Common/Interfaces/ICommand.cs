using MediatR;

namespace QuestCraft.Application.Common.Interfaces;

public interface ICommand<TResponse> : IRequest<TResponse>
{
}

public interface IQuery<TResponse> : IRequest<TResponse>
{
}
