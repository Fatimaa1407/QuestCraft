using QuestCraft.Application.Features.Submissions;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Submissions;

public class GetChallengeReplayQueryHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User User, Challenge Challenge)> SeedAsync()
    {
        var db = InMemoryDbContextFactory.Create();

        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var user = new User
        {
            Username = "tester",
            FirstName = "Test",
            LastName = "User",
            Email = "tester@test.local",
            PasswordHash = "hash",
            RoleId = role.Id,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var challenge = new Challenge
        {
            Title = "Test Challenge",
            Description = "Desc",
            StarterCode = "// code",
            CategoryId = 1,
            DifficultyId = 1,
        };
        db.Challenges.Add(challenge);
        await db.SaveChangesAsync();

        return (db, user, challenge);
    }

    [Fact]
    public async Task Handle_NoSubmissions_ReturnsEmptyDto()
    {
        var (db, user, challenge) = await SeedAsync();
        var handler = new GetChallengeReplayQueryHandler(db, new FakeCurrentUserService { UserId = user.Id });

        var result = await handler.Handle(new GetChallengeReplayQuery(challenge.Id), CancellationToken.None);

        Assert.Equal(0, result.TotalAttempts);
        Assert.Empty(result.Attempts);
    }

    [Fact]
    public async Task Handle_TwoWrongThenAccepted_CountsWrongAttemptsBeforeFirstAccept()
    {
        var (db, user, challenge) = await SeedAsync();
        var baseTime = DateTime.UtcNow.AddMinutes(-10);

        db.ChallengeSubmissions.AddRange(
            new ChallengeSubmission { UserId = user.Id, ChallengeId = challenge.Id, SourceCode = "x", Verdict = SubmissionVerdict.WrongAnswer, SubmittedAt = baseTime },
            new ChallengeSubmission { UserId = user.Id, ChallengeId = challenge.Id, SourceCode = "x", Verdict = SubmissionVerdict.WrongAnswer, SubmittedAt = baseTime.AddMinutes(1) },
            new ChallengeSubmission { UserId = user.Id, ChallengeId = challenge.Id, SourceCode = "x", Verdict = SubmissionVerdict.Accepted, SubmittedAt = baseTime.AddMinutes(2), SolveTimeMs = 120_000 },
            // A later, unrelated resubmission (practice mode) must not affect wrong-attempt counting.
            new ChallengeSubmission { UserId = user.Id, ChallengeId = challenge.Id, SourceCode = "x", Verdict = SubmissionVerdict.WrongAnswer, SubmittedAt = baseTime.AddMinutes(3) }
        );
        await db.SaveChangesAsync();

        var handler = new GetChallengeReplayQueryHandler(db, new FakeCurrentUserService { UserId = user.Id });
        var result = await handler.Handle(new GetChallengeReplayQuery(challenge.Id), CancellationToken.None);

        Assert.Equal(4, result.TotalAttempts);
        Assert.Equal(2, result.WrongAttempts);
        Assert.Equal(120_000, result.TimeToSolveMs);
        Assert.NotNull(result.FirstAcceptedAt);
    }
}
