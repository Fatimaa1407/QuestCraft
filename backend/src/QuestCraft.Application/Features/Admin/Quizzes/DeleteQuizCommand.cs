using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Quizzes;

public record DeleteQuizCommand(int Id) : ICommand<Unit>;

public class DeleteQuizCommandHandler : IRequestHandler<DeleteQuizCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteQuizCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteQuizCommand request, CancellationToken cancellationToken)
    {
        var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Quiz), request.Id);

        quiz.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
