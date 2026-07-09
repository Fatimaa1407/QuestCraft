using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Submissions;

public record GetSubmissionByIdQuery(int Id) : IQuery<SubmissionResultDto>;

public class GetSubmissionByIdQueryHandler : IRequestHandler<GetSubmissionByIdQuery, SubmissionResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSubmissionByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SubmissionResultDto> Handle(GetSubmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var submission = await _context.ChallengeSubmissions
            .Include(s => s.Results)
            .Include(s => s.Challenge).ThenInclude(c => c.TestCases)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ChallengeSubmission), request.Id);

        var isOwner = submission.UserId == _currentUser.UserId;
        var isAdmin = _currentUser.Role == "Admin";
        if (!isOwner && !isAdmin)
        {
            // Not found rather than forbidden — other users' submissions shouldn't be discoverable by id.
            throw new NotFoundException(nameof(ChallengeSubmission), request.Id);
        }

        var testCasesById = submission.Challenge.TestCases.ToDictionary(t => t.Id);
        var testCaseResults = submission.Results.Select(r => r.IsHidden
            ? new SubmissionTestResultDto(true, r.Passed, null, null, null)
            : new SubmissionTestResultDto(false, r.Passed, testCasesById[r.TestCaseId].Input, testCasesById[r.TestCaseId].ExpectedOutput, r.ActualOutput))
            .ToList();

        return new SubmissionResultDto(
            submission.Id,
            submission.Verdict.ToString(),
            testCaseResults.Count(r => r.Passed),
            testCaseResults.Count,
            submission.ExecutionTimeMs,
            submission.MemoryUsedKb,
            0,
            0,
            null,
            testCaseResults);
    }
}
