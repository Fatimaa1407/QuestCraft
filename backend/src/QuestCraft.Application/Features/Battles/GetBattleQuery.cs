using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Battles;

public record GetBattleQuery(int BattleId) : IQuery<BattleDto>;

public class GetBattleQueryHandler : IRequestHandler<GetBattleQuery, BattleDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetBattleQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<BattleDto> Handle(GetBattleQuery request, CancellationToken cancellationToken)
    {
        var battle = await _context.Battles
            .Include(b => b.Challenge)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedAvatar)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedFrame)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedTitle)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedBadge)
            .FirstOrDefaultAsync(b => b.Id == request.BattleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Battle), request.BattleId);

        return BattleMapper.ToDto(battle, _currentUser.IsEnglish);
    }
}

public record GetBattleByCodeQuery(string Code) : IQuery<BattleDto>;

public class GetBattleByCodeQueryHandler : IRequestHandler<GetBattleByCodeQuery, BattleDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetBattleByCodeQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<BattleDto> Handle(GetBattleByCodeQuery request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var battle = await _context.Battles
            .Include(b => b.Challenge)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedAvatar)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedFrame)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedTitle)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedBadge)
            .FirstOrDefaultAsync(b => b.JoinCode == code, cancellationToken)
            ?? throw new NotFoundException(nameof(Battle), request.Code);

        return BattleMapper.ToDto(battle, _currentUser.IsEnglish);
    }
}
