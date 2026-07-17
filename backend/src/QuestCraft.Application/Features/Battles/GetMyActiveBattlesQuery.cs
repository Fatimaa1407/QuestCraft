using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Battles;

public record GetMyActiveBattlesQuery : IQuery<List<BattleSummaryDto>>;

public class GetMyActiveBattlesQueryHandler : IRequestHandler<GetMyActiveBattlesQuery, List<BattleSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyActiveBattlesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public Task<List<BattleSummaryDto>> Handle(GetMyActiveBattlesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        return _context.Battles
            .Include(b => b.Challenge)
            .Include(b => b.Participants)
            .Where(b => (b.Status == BattleStatus.Waiting || b.Status == BattleStatus.InProgress)
                && (b.HostUserId == userId || b.InvitedUserId == userId || b.Participants.Any(p => p.UserId == userId)))
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BattleSummaryDto(
                b.Id, b.Mode.ToString(), b.Status.ToString(), b.Challenge.Title, b.Participants.Count, b.MaxPlayers, b.JoinCode, b.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}

public record GetOpenRoomsQuery : IQuery<List<BattleSummaryDto>>;

public class GetOpenRoomsQueryHandler : IRequestHandler<GetOpenRoomsQuery, List<BattleSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOpenRoomsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<BattleSummaryDto>> Handle(GetOpenRoomsQuery request, CancellationToken cancellationToken) =>
        _context.Battles
            .Include(b => b.Challenge)
            .Include(b => b.Participants)
            .Where(b => b.Mode == BattleMode.Room && b.Status == BattleStatus.Waiting)
            .OrderByDescending(b => b.CreatedAt)
            .Take(30)
            .Select(b => new BattleSummaryDto(
                b.Id, b.Mode.ToString(), b.Status.ToString(), b.Challenge.Title, b.Participants.Count, b.MaxPlayers, b.JoinCode, b.CreatedAt))
            .ToListAsync(cancellationToken);
}
