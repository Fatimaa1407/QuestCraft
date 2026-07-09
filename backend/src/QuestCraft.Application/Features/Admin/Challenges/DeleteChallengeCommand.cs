using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Challenges;

public record DeleteChallengeCommand(int Id) : ICommand<Unit>;

public class DeleteChallengeCommandHandler : IRequestHandler<DeleteChallengeCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteChallengeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteChallengeCommand request, CancellationToken cancellationToken)
    {
        var challenge = await _context.Challenges.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Challenge), request.Id);

        challenge.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
