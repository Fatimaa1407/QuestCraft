using MediatR;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Gamification;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Admin.ExcelIO;

public record ExportLeaderboardQuery(LeaderboardPeriod Period) : IQuery<byte[]>;

public class ExportLeaderboardQueryHandler : IRequestHandler<ExportLeaderboardQuery, byte[]>
{
    private readonly IMediator _mediator;
    private readonly IExcelExportService _excelExportService;

    public ExportLeaderboardQueryHandler(IMediator mediator, IExcelExportService excelExportService)
    {
        _mediator = mediator;
        _excelExportService = excelExportService;
    }

    public async Task<byte[]> Handle(ExportLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var entries = await _mediator.Send(new GetLeaderboardQuery(request.Period, 200), cancellationToken);

        return _excelExportService.Export(
            $"Leaderboard-{request.Period}",
            ["Rank", "Username", "Xp", "Level"],
            entries,
            e => [e.Rank, e.Username, e.Xp, e.Level]);
    }
}
