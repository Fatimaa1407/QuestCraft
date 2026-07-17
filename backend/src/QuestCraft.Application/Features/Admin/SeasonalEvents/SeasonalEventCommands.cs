using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.SeasonalEvents;

public record CreateSeasonalEventCommand(
    string Name, DateOnly StartDate, DateOnly EndDate, bool IsActive,
    string? NameEn = null, string? Description = null, string? DescriptionEn = null, string? Emoji = null)
    : ICommand<SeasonalEventDto>;

public class CreateSeasonalEventCommandValidator : AbstractValidator<CreateSeasonalEventCommand>
{
    public CreateSeasonalEventCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Ad boş ola bilməz.")
            .MaximumLength(100).WithMessage("Ad 100 simvoldan uzun ola bilməz.");
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Bitmə tarixi başlanğıc tarixindən əvvəl ola bilməz.");
    }
}

public class CreateSeasonalEventCommandHandler : IRequestHandler<CreateSeasonalEventCommand, SeasonalEventDto>
{
    private readonly IApplicationDbContext _context;

    public CreateSeasonalEventCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SeasonalEventDto> Handle(CreateSeasonalEventCommand request, CancellationToken cancellationToken)
    {
        var seasonalEvent = new SeasonalEvent
        {
            Name = request.Name,
            NameEn = request.NameEn,
            Description = request.Description,
            DescriptionEn = request.DescriptionEn,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            Emoji = request.Emoji,
        };

        _context.SeasonalEvents.Add(seasonalEvent);
        await _context.SaveChangesAsync(cancellationToken);

        return new SeasonalEventDto(
            seasonalEvent.Id, seasonalEvent.Name, seasonalEvent.NameEn, seasonalEvent.Description, seasonalEvent.DescriptionEn,
            seasonalEvent.StartDate, seasonalEvent.EndDate, seasonalEvent.IsActive, seasonalEvent.Emoji);
    }
}

public record UpdateSeasonalEventCommand(
    int Id, string Name, DateOnly StartDate, DateOnly EndDate, bool IsActive,
    string? NameEn = null, string? Description = null, string? DescriptionEn = null, string? Emoji = null)
    : ICommand<SeasonalEventDto>;

public class UpdateSeasonalEventCommandValidator : AbstractValidator<UpdateSeasonalEventCommand>
{
    public UpdateSeasonalEventCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Ad boş ola bilməz.")
            .MaximumLength(100).WithMessage("Ad 100 simvoldan uzun ola bilməz.");
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Bitmə tarixi başlanğıc tarixindən əvvəl ola bilməz.");
    }
}

public class UpdateSeasonalEventCommandHandler : IRequestHandler<UpdateSeasonalEventCommand, SeasonalEventDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSeasonalEventCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SeasonalEventDto> Handle(UpdateSeasonalEventCommand request, CancellationToken cancellationToken)
    {
        var seasonalEvent = await _context.SeasonalEvents.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(SeasonalEvent), request.Id);

        seasonalEvent.Name = request.Name;
        seasonalEvent.NameEn = request.NameEn;
        seasonalEvent.Description = request.Description;
        seasonalEvent.DescriptionEn = request.DescriptionEn;
        seasonalEvent.StartDate = request.StartDate;
        seasonalEvent.EndDate = request.EndDate;
        seasonalEvent.IsActive = request.IsActive;
        seasonalEvent.Emoji = request.Emoji;

        await _context.SaveChangesAsync(cancellationToken);

        return new SeasonalEventDto(
            seasonalEvent.Id, seasonalEvent.Name, seasonalEvent.NameEn, seasonalEvent.Description, seasonalEvent.DescriptionEn,
            seasonalEvent.StartDate, seasonalEvent.EndDate, seasonalEvent.IsActive, seasonalEvent.Emoji);
    }
}

public record DeleteSeasonalEventCommand(int Id) : ICommand<Unit>;

public class DeleteSeasonalEventCommandHandler : IRequestHandler<DeleteSeasonalEventCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteSeasonalEventCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteSeasonalEventCommand request, CancellationToken cancellationToken)
    {
        var seasonalEvent = await _context.SeasonalEvents.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(SeasonalEvent), request.Id);

        seasonalEvent.IsDeleted = true;
        seasonalEvent.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
