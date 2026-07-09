using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Admin.Quizzes;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Quizzes;

public record GetQuizForAttemptQuery(int Id) : IQuery<QuizAttemptViewDto>;

public class GetQuizForAttemptQueryHandler : IRequestHandler<GetQuizForAttemptQuery, QuizAttemptViewDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetQuizForAttemptQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<QuizAttemptViewDto> Handle(GetQuizForAttemptQuery request, CancellationToken cancellationToken)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions).ThenInclude(qu => qu.Options)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Quiz), request.Id);

        if (!quiz.IsPublished && _currentUser.Role != "Admin")
        {
            throw new NotFoundException(nameof(Quiz), request.Id);
        }

        var questions = quiz.Questions.Select(q => new QuestionDto(
            q.Id, q.Text, q.Options.Select(o => new QuestionOptionDto(o.Id, o.Text)).ToList())).ToList();

        return new QuizAttemptViewDto(quiz.Id, quiz.Title, quiz.XpReward, questions);
    }
}
