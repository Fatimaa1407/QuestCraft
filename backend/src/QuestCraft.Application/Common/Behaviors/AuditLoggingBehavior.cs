using MediatR;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Common.Behaviors;

// Blanket audit trail for admin actions: any command living under a "...Features.Admin.*" namespace gets
// one AuditLogs row recording who ran it and when. Runs inside the DB transaction (registered after
// TransactionBehavior) so the audit row commits or rolls back together with the command's own writes.
// Handlers that need richer detail (e.g. PurchaseItemCommand) still write their own AuditLog entry too.
public class AuditLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AuditLoggingBehavior(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        var requestNamespace = typeof(TRequest).Namespace;
        if (_currentUser.UserId is not null && requestNamespace is not null && requestNamespace.Contains(".Admin."))
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = _currentUser.UserId,
                Action = typeof(TRequest).Name,
                EntityName = requestNamespace.Split('.').Last(),
                IpAddress = _currentUser.IpAddress,
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
