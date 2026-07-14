using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.DailyQuestTemplates;

public record DeleteDailyQuestTemplateCommand(int Id) : ICommand<Unit>;

public class DeleteDailyQuestTemplateCommandHandler : IRequestHandler<DeleteDailyQuestTemplateCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteDailyQuestTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteDailyQuestTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.DailyQuestTemplates.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(DailyQuestTemplate), request.Id);

        var inUse = await _context.UserDailyQuests.AnyAsync(q => q.DailyQuestTemplateId == request.Id, cancellationToken);
        if (inUse)
        {
            throw new ConflictException("Bu tapşırıq artıq istifadəçilərə təyin edildiyi üçün silinə bilməz. Əvəzinə deaktiv edin.");
        }

        template.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
