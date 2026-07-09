using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Quizzes;

public record GetQuizByIdAdminQuery(int Id) : IQuery<QuizAdminDetailDto>;

public class GetQuizByIdAdminQueryHandler : IRequestHandler<GetQuizByIdAdminQuery, QuizAdminDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetQuizByIdAdminQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuizAdminDetailDto> Handle(GetQuizByIdAdminQuery request, CancellationToken cancellationToken)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Category)
            .Include(q => q.Questions).ThenInclude(qu => qu.Options)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Quiz), request.Id);

        var questions = quiz.Questions.Select(q => new QuestionAdminDto(
            q.Id, q.Text, q.Explanation,
            q.Options.Select(o => new QuestionOptionAdminDto(o.Id, o.Text, o.IsCorrect)).ToList())).ToList();

        return new QuizAdminDetailDto(quiz.Id, quiz.Title, quiz.CategoryId, quiz.Category?.Name, quiz.XpReward, quiz.IsPublished, questions);
    }
}
