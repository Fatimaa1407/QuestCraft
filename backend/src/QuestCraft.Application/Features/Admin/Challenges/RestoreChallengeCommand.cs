using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Challenges;

public record RestoreChallengeCommand(int Id) : ICommand<Unit>;

public class RestoreChallengeCommandHandler : IRequestHandler<RestoreChallengeCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public RestoreChallengeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RestoreChallengeCommand request, CancellationToken cancellationToken)
    {
        var challenge = await _context.Challenges
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Challenge), request.Id);

        challenge.IsDeleted = false;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
