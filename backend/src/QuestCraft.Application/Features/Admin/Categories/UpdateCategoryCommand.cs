using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Categories;

public record UpdateCategoryCommand(int Id, string Name, string? Description, string? IconUrl) : ICommand<CategoryDto>;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Ad boş ola bilməz.")
            .MaximumLength(100).WithMessage("Ad 100 simvoldan uzun ola bilməz.");
    }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ChallengeCategories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ChallengeCategory), request.Id);

        if (await _context.ChallengeCategories.AnyAsync(c => c.Name == request.Name && c.Id != request.Id, cancellationToken))
        {
            throw new ConflictException($"\"{request.Name}\" adlı kateqoriya artıq mövcuddur.");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.IconUrl = request.IconUrl;

        await _context.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id, category.Name, category.Description, category.IconUrl);
    }
}
