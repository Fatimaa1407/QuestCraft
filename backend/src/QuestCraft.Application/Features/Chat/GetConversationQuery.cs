using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Common.Models;

namespace QuestCraft.Application.Features.Chat;

public record GetConversationQuery(int WithUserId, int Page, int PageSize) : IQuery<PagedResult<ChatMessageDto>>;

public class GetConversationQueryHandler : IRequestHandler<GetConversationQuery, PagedResult<ChatMessageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetConversationQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public Task<PagedResult<ChatMessageDto>> Handle(GetConversationQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var query = _context.ChatMessages
            .Where(m => (m.SenderId == userId && m.RecipientId == request.WithUserId)
                || (m.SenderId == request.WithUserId && m.RecipientId == userId))
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new ChatMessageDto(m.Id, m.SenderId, m.RecipientId, m.Content, m.CreatedAt, m.IsRead));

        return PagedResult<ChatMessageDto>.CreateAsync(query, request.Page, request.PageSize, cancellationToken);
    }
}
