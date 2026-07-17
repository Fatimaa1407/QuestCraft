using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Quizzes;

public record CreateQuizCommand(
    string Title, int? CategoryId, int XpReward, bool IsPublished, int RequiredLevel = 1, string? TitleEn = null, string? Tags = null)
    : ICommand<QuizListItemDto>;

public class CreateQuizCommandValidator : AbstractValidator<CreateQuizCommand>
{
    public CreateQuizCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Başlıq boş ola bilməz.")
            .MaximumLength(200).WithMessage("Başlıq 200 simvoldan uzun ola bilməz.");
        RuleFor(x => x.XpReward).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RequiredLevel).GreaterThanOrEqualTo(1).WithMessage("Tələb olunan level ən azı 1 olmalıdır.");
    }
}

public class CreateQuizCommandHandler : IRequestHandler<CreateQuizCommand, QuizListItemDto>
{
    private readonly IApplicationDbContext _context;

    public CreateQuizCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuizListItemDto> Handle(CreateQuizCommand request, CancellationToken cancellationToken)
    {
        string? categoryName = null;
        if (request.CategoryId is not null)
        {
            var category = await _context.ChallengeCategories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken)
                ?? throw new NotFoundException(nameof(ChallengeCategory), request.CategoryId);
            categoryName = category.Name;
        }

        var quiz = new Quiz
        {
            Title = request.Title,
            CategoryId = request.CategoryId,
            XpReward = request.XpReward,
            IsPublished = request.IsPublished,
            RequiredLevel = request.RequiredLevel,
            TitleEn = request.TitleEn,
            Tags = request.Tags,
        };

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync(cancellationToken);

        return new QuizListItemDto(quiz.Id, quiz.Title, categoryName, quiz.XpReward, quiz.IsPublished, 0, quiz.RequiredLevel, false, quiz.Tags);
    }
}
