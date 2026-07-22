using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Battles;

public record CreateRoomBattleCommand(int MaxPlayers) : ICommand<BattleDto>;

public class CreateRoomBattleCommandValidator : AbstractValidator<CreateRoomBattleCommand>
{
    public CreateRoomBattleCommandValidator()
    {
        RuleFor(x => x.MaxPlayers).InclusiveBetween(2, 10).WithMessage("Otaq ölçüsü 2-10 arasında olmalıdır.");
    }
}

public class CreateRoomBattleCommandHandler : IRequestHandler<CreateRoomBattleCommand, BattleDto>
{
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O/1/I — avoids lookalike confusion

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateRoomBattleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<BattleDto> Handle(CreateRoomBattleCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var challenge = await BattlePoolSelector.PickRandomAsync(_context, cancellationToken);

        var totalTestCases = challenge.TestCases.Count + challenge.HiddenTestCases.Count;
        var joinCode = await GenerateUniqueCodeAsync(cancellationToken);

        var battle = new Battle
        {
            Mode = BattleMode.Room,
            Status = BattleStatus.Waiting,
            MaxPlayers = request.MaxPlayers,
            ChallengeId = challenge.Id,
            HostUserId = userId,
            JoinCode = joinCode,
        };
        battle.Participants.Add(new BattleParticipant { UserId = userId, TotalTestCases = totalTestCases });

        _context.Battles.Add(battle);
        await _context.SaveChangesAsync(cancellationToken);

        var saved = await _context.Battles
            .Include(b => b.Challenge)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedAvatar)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedFrame)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedTitle)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedBadge)
            .FirstAsync(b => b.Id == battle.Id, cancellationToken);

        return BattleMapper.ToDto(saved, _currentUser.IsEnglish);
    }

    private async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var code = string.Create(6, Random.Shared, (span, rng) =>
            {
                for (var i = 0; i < span.Length; i++)
                {
                    span[i] = CodeAlphabet[rng.Next(CodeAlphabet.Length)];
                }
            });

            if (!await _context.Battles.AnyAsync(b => b.JoinCode == code, cancellationToken))
            {
                return code;
            }
        }

        throw new ConflictException("Otaq kodu yaradıla bilmədi, yenidən cəhd edin.");
    }
}
