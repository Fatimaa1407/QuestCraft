using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Quizzes;

public record QuestionOptionInput(string Text, bool IsCorrect, string? TextEn = null);

public record AddQuestionCommand(
    int QuizId, string Text, string? Explanation, List<QuestionOptionInput> Options,
    string? TextEn = null, string? ExplanationEn = null) : ICommand<int>;

public class AddQuestionCommandValidator : AbstractValidator<AddQuestionCommand>
{
    public AddQuestionCommandValidator()
    {
        RuleFor(x => x.QuizId).GreaterThan(0);
        RuleFor(x => x.Text).NotEmpty().WithMessage("Sual mətni boş ola bilməz.");
        RuleFor(x => x.Options).Must(o => o.Count >= 2).WithMessage("Ən azı 2 seçim olmalıdır.");
        RuleFor(x => x.Options).Must(o => o.Count(x => x.IsCorrect) == 1)
            .WithMessage("Düzgün olaraq yalnız bir seçim işarələnməlidir.");
    }
}

public class AddQuestionCommandHandler : IRequestHandler<AddQuestionCommand, int>
{
    private readonly IApplicationDbContext _context;

    public AddQuestionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(AddQuestionCommand request, CancellationToken cancellationToken)
    {
        var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == request.QuizId, cancellationToken)
            ?? throw new NotFoundException(nameof(Quiz), request.QuizId);

        var normalizedText = request.Text.Trim().ToLower();
        var duplicateLevel = await _context.Questions
            .Where(q => q.Text.ToLower() == normalizedText && q.Quiz.RequiredLevel != quiz.RequiredLevel)
            .Select(q => (int?)q.Quiz.RequiredLevel)
            .FirstOrDefaultAsync(cancellationToken);
        if (duplicateLevel is not null)
        {
            throw new ConflictException($"Bu sual artıq Level {duplicateLevel}-dəki bir testdə istifadə olunub. Hər sual yalnız bir səviyyədə istifadə oluna bilər.");
        }

        var question = new Question
        {
            QuizId = request.QuizId,
            Text = request.Text,
            Explanation = request.Explanation,
            TextEn = request.TextEn,
            ExplanationEn = request.ExplanationEn,
            Options = request.Options.Select(o => new QuestionOption { Text = o.Text, IsCorrect = o.IsCorrect, TextEn = o.TextEn }).ToList(),
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync(cancellationToken);

        return question.Id;
    }
}

public record UpdateQuestionCommand(
    int Id, string Text, string? Explanation, List<QuestionOptionInput> Options,
    string? TextEn = null, string? ExplanationEn = null) : ICommand<Unit>;

public class UpdateQuestionCommandValidator : AbstractValidator<UpdateQuestionCommand>
{
    public UpdateQuestionCommandValidator()
    {
        RuleFor(x => x.Text).NotEmpty().WithMessage("Sual mətni boş ola bilməz.");
        RuleFor(x => x.Options).Must(o => o.Count >= 2).WithMessage("Ən azı 2 seçim olmalıdır.");
        RuleFor(x => x.Options).Must(o => o.Count(x => x.IsCorrect) == 1)
            .WithMessage("Düzgün olaraq yalnız bir seçim işarələnməlidir.");
    }
}

public class UpdateQuestionCommandHandler : IRequestHandler<UpdateQuestionCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateQuestionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _context.Questions
            .Include(q => q.Options)
            .Include(q => q.Quiz)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Question), request.Id);

        var normalizedText = request.Text.Trim().ToLower();
        var duplicateLevel = await _context.Questions
            .Where(q => q.Id != request.Id && q.Text.ToLower() == normalizedText && q.Quiz.RequiredLevel != question.Quiz.RequiredLevel)
            .Select(q => (int?)q.Quiz.RequiredLevel)
            .FirstOrDefaultAsync(cancellationToken);
        if (duplicateLevel is not null)
        {
            throw new ConflictException($"Bu sual artıq Level {duplicateLevel}-dəki bir testdə istifadə olunub. Hər sual yalnız bir səviyyədə istifadə oluna bilər.");
        }

        question.Text = request.Text;
        question.Explanation = request.Explanation;
        question.TextEn = request.TextEn;
        question.ExplanationEn = request.ExplanationEn;

        // Soft-delete the old options rather than hard-removing them: past QuizAttemptAnswer rows
        // reference them via a Restrict FK, so a real DELETE would fail once anyone has attempted the quiz.
        foreach (var option in question.Options)
        {
            option.IsDeleted = true;
        }

        foreach (var input in request.Options)
        {
            question.Options.Add(new QuestionOption { Text = input.Text, IsCorrect = input.IsCorrect, TextEn = input.TextEn });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record DeleteQuestionCommand(int Id) : ICommand<Unit>;

public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteQuestionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Question), request.Id);

        question.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
