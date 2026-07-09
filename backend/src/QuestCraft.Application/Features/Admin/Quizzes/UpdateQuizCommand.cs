using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Quizzes;

public record UpdateQuizCommand(int Id, string Title, int? CategoryId, int XpReward, bool IsPublished) : ICommand<QuizListItemDto>;

public class UpdateQuizCommandValidator : AbstractValidator<UpdateQuizCommand>
{
    public UpdateQuizCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Başlıq boş ola bilməz.")
            .MaximumLength(200).WithMessage("Başlıq 200 simvoldan uzun ola bilməz.");
        RuleFor(x => x.XpReward).GreaterThanOrEqualTo(0);
    }
}

public class UpdateQuizCommandHandler : IRequestHandler<UpdateQuizCommand, QuizListItemDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateQuizCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuizListItemDto> Handle(UpdateQuizCommand request, CancellationToken cancellationToken)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Quiz), request.Id);

        string? categoryName = null;
        if (request.CategoryId is not null)
        {
            var category = await _context.ChallengeCategories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken)
                ?? throw new NotFoundException(nameof(ChallengeCategory), request.CategoryId);
            categoryName = category.Name;
        }

        quiz.Title = request.Title;
        quiz.CategoryId = request.CategoryId;
        quiz.XpReward = request.XpReward;
        quiz.IsPublished = request.IsPublished;

        await _context.SaveChangesAsync(cancellationToken);

        return new QuizListItemDto(quiz.Id, quiz.Title, categoryName, quiz.XpReward, quiz.IsPublished, quiz.Questions.Count);
    }
}
