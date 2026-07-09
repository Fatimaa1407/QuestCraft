using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Submissions;

// A query, not a command: nothing is persisted, so it stays outside the transactional pipeline.
public record RunChallengeQuery(int ChallengeId, string SourceCode) : IQuery<RunResultDto>;

public class RunChallengeQueryValidator : AbstractValidator<RunChallengeQuery>
{
    public RunChallengeQueryValidator()
    {
        RuleFor(x => x.ChallengeId).GreaterThan(0);
        RuleFor(x => x.SourceCode).NotEmpty().WithMessage("Kod boş ola bilməz.")
            .MaximumLength(50_000).WithMessage("Kod 50.000 simvoldan uzun ola bilməz.");
    }
}

public class RunChallengeQueryHandler : IRequestHandler<RunChallengeQuery, RunResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICodeExecutionEngine _codeExecutionEngine;
    private readonly ICurrentUserService _currentUser;

    public RunChallengeQueryHandler(IApplicationDbContext context, ICodeExecutionEngine codeExecutionEngine, ICurrentUserService currentUser)
    {
        _context = context;
        _codeExecutionEngine = codeExecutionEngine;
        _currentUser = currentUser;
    }

    public async Task<RunResultDto> Handle(RunChallengeQuery request, CancellationToken cancellationToken)
    {
        var challenge = await _context.Challenges
            .Include(c => c.TestCases)
            .FirstOrDefaultAsync(c => c.Id == request.ChallengeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Challenge), request.ChallengeId);

        if (!challenge.IsPublished && _currentUser.Role != "Admin")
        {
            throw new NotFoundException(nameof(Challenge), request.ChallengeId);
        }

        var testCaseInputs = challenge.TestCases
            .OrderBy(t => t.OrderIndex)
            .Select(t => new TestCaseInput(t.Id, t.Input, t.ExpectedOutput, IsHidden: false))
            .ToList();

        var execution = await _codeExecutionEngine.ExecuteAsync(
            request.SourceCode, testCaseInputs, challenge.TimeLimitMs, challenge.MemoryLimitMb, cancellationToken);

        var testCasesById = challenge.TestCases.ToDictionary(t => t.Id);
        var results = execution.TestResults.Select(r => new RunTestResultDto(
            r.Passed, testCasesById[r.TestCaseId].Input, testCasesById[r.TestCaseId].ExpectedOutput, r.ActualOutput, r.ExecutionTimeMs)).ToList();

        return new RunResultDto(execution.Verdict.ToString(), execution.ExecutionTimeMs, execution.CompileErrorMessage, results);
    }
}
