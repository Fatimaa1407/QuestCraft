using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.ExcelIO;

public record ExportMarketplaceDataQuery : IQuery<byte[]>;

public class ExportMarketplaceDataQueryHandler : IRequestHandler<ExportMarketplaceDataQuery, byte[]>
{
    private readonly IApplicationDbContext _context;
    private readonly IExcelExportService _excelExportService;

    public ExportMarketplaceDataQueryHandler(IApplicationDbContext context, IExcelExportService excelExportService)
    {
        _context = context;
        _excelExportService = excelExportService;
    }

    public async Task<byte[]> Handle(ExportMarketplaceDataQuery request, CancellationToken cancellationToken)
    {
        var rows = await _context.Purchases
            .Include(p => p.User)
            .Include(p => p.MarketplaceItem).ThenInclude(i => i.ItemType)
            .OrderByDescending(p => p.PurchasedAt)
            .Select(p => new
            {
                Username = p.User.Username,
                Item = p.MarketplaceItem.Name,
                ItemType = p.MarketplaceItem.ItemType.Name,
                p.PricePaid,
                p.PurchasedAt,
            })
            .ToListAsync(cancellationToken);

        return _excelExportService.Export(
            "MarketplaceData",
            ["Username", "Item", "ItemType", "PricePaid", "PurchasedAt"],
            rows,
            r => [r.Username, r.Item, r.ItemType, r.PricePaid, r.PurchasedAt]);
    }
}
