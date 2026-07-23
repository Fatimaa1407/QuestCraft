using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Challenges;
using QuestCraft.Domain.Entities;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Challenges;

public class PostChallengeCommentCommandHandlerTests
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
            Profile = new UserProfile(),
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
    public async Task Handle_TopLevelComment_Succeeds()
    {
        var (db, user, challenge) = await SeedAsync();
        var handler = new PostChallengeCommentCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        var result = await handler.Handle(new PostChallengeCommentCommand(challenge.Id, "Sual var", false, null), CancellationToken.None);

        Assert.Equal("Sual var", result.Content);
        Assert.Null(result.ParentCommentId);
    }

    [Fact]
    public async Task Handle_ReplyToTopLevelComment_Succeeds()
    {
        var (db, user, challenge) = await SeedAsync();
        var handler = new PostChallengeCommentCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });
        var parent = await handler.Handle(new PostChallengeCommentCommand(challenge.Id, "Sual", false, null), CancellationToken.None);

        var reply = await handler.Handle(new PostChallengeCommentCommand(challenge.Id, "Cavab", false, parent.Id), CancellationToken.None);

        Assert.Equal(parent.Id, reply.ParentCommentId);
    }

    [Fact]
    public async Task Handle_ReplyToReply_ThrowsBadRequest()
    {
        var (db, user, challenge) = await SeedAsync();
        var handler = new PostChallengeCommentCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });
        var parent = await handler.Handle(new PostChallengeCommentCommand(challenge.Id, "Sual", false, null), CancellationToken.None);
        var reply = await handler.Handle(new PostChallengeCommentCommand(challenge.Id, "Cavab", false, parent.Id), CancellationToken.None);

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(new PostChallengeCommentCommand(challenge.Id, "İkinci cavab", false, reply.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ParentFromDifferentChallenge_ThrowsBadRequest()
    {
        var (db, user, challenge) = await SeedAsync();
        var otherChallenge = new Challenge { Title = "Other", Description = "Desc", StarterCode = "// code", CategoryId = 1, DifficultyId = 1 };
        db.Challenges.Add(otherChallenge);
        await db.SaveChangesAsync();

        var handler = new PostChallengeCommentCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });
        var parent = await handler.Handle(new PostChallengeCommentCommand(challenge.Id, "Sual", false, null), CancellationToken.None);

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(new PostChallengeCommentCommand(otherChallenge.Id, "Cavab", false, parent.Id), CancellationToken.None));
    }
}
