using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Common.Models;

namespace QuestCraft.Application.Features.Admin.Users;

public record AdminUserListItemDto(
    int Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    int Level,
    int Xp,
    int Coins,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

public record GetAdminUsersQuery(int Page, int PageSize, string? Search) : IQuery<PagedResult<AdminUserListItemDto>>;

public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, PagedResult<AdminUserListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAdminUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<PagedResult<AdminUserListItemDto>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .Include(u => u.Role)
            .Include(u => u.Profile)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(u => u.Username.Contains(term) || u.Email.Contains(term));
        }

        var projected = query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserListItemDto(
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role.Name,
                u.Profile != null ? u.Profile.Level : 1,
                u.Profile != null ? u.Profile.Xp : 0,
                u.Profile != null ? u.Profile.Coins : 0,
                u.IsActive,
                u.CreatedAt,
                u.LastLoginAt));

        return PagedResult<AdminUserListItemDto>.CreateAsync(projected, request.Page, request.PageSize, cancellationToken);
    }
}
