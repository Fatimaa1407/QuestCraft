using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Categories;

public record RestoreCategoryCommand(int Id) : ICommand<CategoryDto>;

public class RestoreCategoryCommandHandler : IRequestHandler<RestoreCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public RestoreCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(RestoreCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ChallengeCategories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(ChallengeCategory), request.Id);

        if (await _context.ChallengeCategories.AnyAsync(c => c.Name == category.Name && c.Id != category.Id, cancellationToken))
        {
            throw new ConflictException($"\"{category.Name}\" adlı aktiv kateqoriya artıq mövcuddur, əvvəlcə onu dəyişin.");
        }

        category.IsDeleted = false;
        await _context.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id, category.Name, category.Description, category.IconUrl);
    }
}
