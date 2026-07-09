using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Categories;

public record DeleteCategoryCommand(int Id) : ICommand<Unit>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ChallengeCategories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ChallengeCategory), request.Id);

        var inUse = await _context.Challenges.AnyAsync(c => c.CategoryId == request.Id, cancellationToken);
        if (inUse)
        {
            throw new ConflictException("Bu kateqoriyaya aid challenge-lər olduğu üçün silinə bilməz.");
        }

        category.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
