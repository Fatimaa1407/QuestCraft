using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Marketplace;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Marketplace;

public record CreateMarketplaceItemCommand(
    string Name, string? Description, int ItemTypeId, int Price, string? ImageUrl, bool IsActive,
    string? NameEn = null, string? DescriptionEn = null) : ICommand<MarketplaceItemDto>;

public class CreateMarketplaceItemCommandValidator : AbstractValidator<CreateMarketplaceItemCommand>
{
    public CreateMarketplaceItemCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Ad boş ola bilməz.")
            .MaximumLength(150).WithMessage("Ad 150 simvoldan uzun ola bilməz.");
        RuleFor(x => x.ItemTypeId).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Qiymət 0-dan böyük olmalıdır.");
    }
}

public class CreateMarketplaceItemCommandHandler : IRequestHandler<CreateMarketplaceItemCommand, MarketplaceItemDto>
{
    private readonly IApplicationDbContext _context;

    public CreateMarketplaceItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MarketplaceItemDto> Handle(CreateMarketplaceItemCommand request, CancellationToken cancellationToken)
    {
        var itemType = await _context.MarketplaceItemTypes.FirstOrDefaultAsync(t => t.Id == request.ItemTypeId, cancellationToken)
            ?? throw new NotFoundException(nameof(MarketplaceItemType), request.ItemTypeId);

        var item = new MarketplaceItem
        {
            Name = request.Name,
            Description = request.Description,
            NameEn = request.NameEn,
            DescriptionEn = request.DescriptionEn,
            ItemTypeId = itemType.Id,
            Price = request.Price,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive,
        };

        _context.MarketplaceItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return new MarketplaceItemDto(item.Id, item.Name, item.Description, item.ItemTypeId, itemType.Name, item.Price, item.ImageUrl, item.IsActive, false);
    }
}

public record UpdateMarketplaceItemCommand(
    int Id, string Name, string? Description, int ItemTypeId, int Price, string? ImageUrl, bool IsActive,
    string? NameEn = null, string? DescriptionEn = null) : ICommand<MarketplaceItemDto>;

public class UpdateMarketplaceItemCommandValidator : AbstractValidator<UpdateMarketplaceItemCommand>
{
    public UpdateMarketplaceItemCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Ad boş ola bilməz.")
            .MaximumLength(150).WithMessage("Ad 150 simvoldan uzun ola bilməz.");
        RuleFor(x => x.ItemTypeId).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Qiymət 0-dan böyük olmalıdır.");
    }
}

public class UpdateMarketplaceItemCommandHandler : IRequestHandler<UpdateMarketplaceItemCommand, MarketplaceItemDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateMarketplaceItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MarketplaceItemDto> Handle(UpdateMarketplaceItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.MarketplaceItems.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(MarketplaceItem), request.Id);

        var itemType = await _context.MarketplaceItemTypes.FirstOrDefaultAsync(t => t.Id == request.ItemTypeId, cancellationToken)
            ?? throw new NotFoundException(nameof(MarketplaceItemType), request.ItemTypeId);

        item.Name = request.Name;
        item.Description = request.Description;
        item.NameEn = request.NameEn;
        item.DescriptionEn = request.DescriptionEn;
        item.ItemTypeId = itemType.Id;
        item.Price = request.Price;
        item.ImageUrl = request.ImageUrl;
        item.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return new MarketplaceItemDto(item.Id, item.Name, item.Description, item.ItemTypeId, itemType.Name, item.Price, item.ImageUrl, item.IsActive, false);
    }
}

public record DeleteMarketplaceItemCommand(int Id) : ICommand<Unit>;

public class DeleteMarketplaceItemCommandHandler : IRequestHandler<DeleteMarketplaceItemCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteMarketplaceItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteMarketplaceItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.MarketplaceItems.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(MarketplaceItem), request.Id);

        item.IsDeleted = true;
        item.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
