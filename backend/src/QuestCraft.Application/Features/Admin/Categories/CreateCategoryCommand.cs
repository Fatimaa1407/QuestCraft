using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Categories;

public record CreateCategoryCommand(string Name, string? Description, string? IconUrl) : ICommand<CategoryDto>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Ad boş ola bilməz.")
            .MaximumLength(100).WithMessage("Ad 100 simvoldan uzun ola bilməz.");
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (await _context.ChallengeCategories.AnyAsync(c => c.Name == request.Name, cancellationToken))
        {
            throw new ConflictException($"\"{request.Name}\" adlı kateqoriya artıq mövcuddur.");
        }

        var category = new ChallengeCategory
        {
            Name = request.Name,
            Description = request.Description,
            IconUrl = request.IconUrl,
        };

        _context.ChallengeCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id, category.Name, category.Description, category.IconUrl);
    }
}
