using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Quizzes;

public record RestoreQuizCommand(int Id) : ICommand<QuizListItemDto>;

public class RestoreQuizCommandHandler : IRequestHandler<RestoreQuizCommand, QuizListItemDto>
{
    private readonly IApplicationDbContext _context;

    public RestoreQuizCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuizListItemDto> Handle(RestoreQuizCommand request, CancellationToken cancellationToken)
    {
        var quiz = await _context.Quizzes
            .IgnoreQueryFilters()
            .Include(q => q.Category)
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == request.Id && q.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Quiz), request.Id);

        quiz.IsDeleted = false;
        await _context.SaveChangesAsync(cancellationToken);

        return new QuizListItemDto(
            quiz.Id, quiz.Title, quiz.Category?.Name, quiz.XpReward, quiz.IsPublished, quiz.Questions.Count, quiz.RequiredLevel, false);
    }
}
