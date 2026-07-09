using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Quizzes;

public record QuestionOptionInput(string Text, bool IsCorrect);

public record AddQuestionCommand(int QuizId, string Text, string? Explanation, List<QuestionOptionInput> Options) : ICommand<int>;

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
        var quizExists = await _context.Quizzes.AnyAsync(q => q.Id == request.QuizId, cancellationToken);
        if (!quizExists)
        {
            throw new NotFoundException(nameof(Quiz), request.QuizId);
        }

        var question = new Question
        {
            QuizId = request.QuizId,
            Text = request.Text,
            Explanation = request.Explanation,
            Options = request.Options.Select(o => new QuestionOption { Text = o.Text, IsCorrect = o.IsCorrect }).ToList(),
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync(cancellationToken);

        return question.Id;
    }
}

public record UpdateQuestionCommand(int Id, string Text, string? Explanation, List<QuestionOptionInput> Options) : ICommand<Unit>;

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
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Question), request.Id);

        question.Text = request.Text;
        question.Explanation = request.Explanation;

        // Soft-delete the old options rather than hard-removing them: past QuizAttemptAnswer rows
        // reference them via a Restrict FK, so a real DELETE would fail once anyone has attempted the quiz.
        foreach (var option in question.Options)
        {
            option.IsDeleted = true;
        }

        foreach (var input in request.Options)
        {
            question.Options.Add(new QuestionOption { Text = input.Text, IsCorrect = input.IsCorrect });
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
