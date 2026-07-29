using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Common.Behaviors;

// Closes an entire class of reward-granting races (double XP from two simultaneous "first solve"
// submissions, double coin deduction from two simultaneous purchases, double daily-quest claims,
// etc.) in one place, rather than hand-adding locking to every handler that touches UserProfile.
//
// UserProfile carries a RowVersion concurrency token (see UserProfileConfiguration), so the instant
// two concurrent commands both read a user's profile, then both try to save a change to it, the
// loser's SaveChangesAsync throws DbUpdateConcurrencyException instead of silently overwriting the
// winner's write. Wrapping outside TransactionBehavior means each retry gets a genuinely fresh
// transaction; clearing the change tracker first means the retried handler re-reads current
// state from the database rather than resubmitting the same (now-stale, already-rejected) values —
// so a retried "first solve" correctly re-checks and finds it's no longer first, a retried purchase
// re-checks and finds the item now already owned, etc.
public class ConcurrencyRetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private const int MaxAttempts = 3;

    private readonly IApplicationDbContext _context;
    private readonly ILogger<ConcurrencyRetryBehavior<TRequest, TResponse>> _logger;

    public ConcurrencyRetryBehavior(IApplicationDbContext context, ILogger<ConcurrencyRetryBehavior<TRequest, TResponse>> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await next();
            }
            catch (DbUpdateConcurrencyException) when (attempt < MaxAttempts)
            {
                _logger.LogWarning(
                    "{RequestName} yarış vəziyyəti səbəbindən {Attempt}-ci cəhddə uğursuz oldu, yenidən sınanılır.",
                    typeof(TRequest).Name, attempt);
                _context.ClearChangeTracking();
            }
        }
    }
}
