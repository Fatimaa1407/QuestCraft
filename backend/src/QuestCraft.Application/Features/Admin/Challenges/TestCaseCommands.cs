using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Challenges;

public record AddTestCaseCommand(int ChallengeId, string Input, string ExpectedOutput, int OrderIndex, bool IsHidden, int Weight)
    : ICommand<int>;

public class AddTestCaseCommandValidator : AbstractValidator<AddTestCaseCommand>
{
    public AddTestCaseCommandValidator()
    {
        RuleFor(x => x.ChallengeId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull().WithMessage("Input boş ola bilməz.");
        RuleFor(x => x.ExpectedOutput).NotNull().WithMessage("Expected output boş ola bilməz.");
        RuleFor(x => x.Weight).GreaterThan(0).WithMessage("Çəki 0-dan böyük olmalıdır.");
    }
}

public class AddTestCaseCommandHandler : IRequestHandler<AddTestCaseCommand, int>
{
    private readonly IApplicationDbContext _context;

    public AddTestCaseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(AddTestCaseCommand request, CancellationToken cancellationToken)
    {
        var challengeExists = await _context.Challenges.AnyAsync(c => c.Id == request.ChallengeId, cancellationToken);
        if (!challengeExists)
        {
            throw new NotFoundException(nameof(Challenge), request.ChallengeId);
        }

        if (request.IsHidden)
        {
            var hiddenTestCase = new HiddenTestCase
            {
                ChallengeId = request.ChallengeId,
                Input = request.Input,
                ExpectedOutput = request.ExpectedOutput,
                OrderIndex = request.OrderIndex,
                Weight = request.Weight,
            };
            _context.HiddenTestCases.Add(hiddenTestCase);
            await _context.SaveChangesAsync(cancellationToken);
            return hiddenTestCase.Id;
        }

        var testCase = new TestCase
        {
            ChallengeId = request.ChallengeId,
            Input = request.Input,
            ExpectedOutput = request.ExpectedOutput,
            OrderIndex = request.OrderIndex,
        };
        _context.TestCases.Add(testCase);
        await _context.SaveChangesAsync(cancellationToken);
        return testCase.Id;
    }
}

public record UpdateTestCaseCommand(int Id, bool IsHidden, string Input, string ExpectedOutput, int OrderIndex, int Weight)
    : ICommand<Unit>;

public class UpdateTestCaseCommandHandler : IRequestHandler<UpdateTestCaseCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateTestCaseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateTestCaseCommand request, CancellationToken cancellationToken)
    {
        if (request.IsHidden)
        {
            var hiddenTestCase = await _context.HiddenTestCases.FirstOrDefaultAsync(h => h.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(HiddenTestCase), request.Id);

            hiddenTestCase.Input = request.Input;
            hiddenTestCase.ExpectedOutput = request.ExpectedOutput;
            hiddenTestCase.OrderIndex = request.OrderIndex;
            hiddenTestCase.Weight = request.Weight;
        }
        else
        {
            var testCase = await _context.TestCases.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(TestCase), request.Id);

            testCase.Input = request.Input;
            testCase.ExpectedOutput = request.ExpectedOutput;
            testCase.OrderIndex = request.OrderIndex;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record DeleteTestCaseCommand(int Id, bool IsHidden) : ICommand<Unit>;

public class DeleteTestCaseCommandHandler : IRequestHandler<DeleteTestCaseCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteTestCaseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteTestCaseCommand request, CancellationToken cancellationToken)
    {
        if (request.IsHidden)
        {
            var hiddenTestCase = await _context.HiddenTestCases.FirstOrDefaultAsync(h => h.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(HiddenTestCase), request.Id);
            hiddenTestCase.IsDeleted = true;
        }
        else
        {
            var testCase = await _context.TestCases.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(TestCase), request.Id);
            testCase.IsDeleted = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
