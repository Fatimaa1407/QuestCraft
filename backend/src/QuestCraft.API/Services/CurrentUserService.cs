using System.Security.Claims;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Username => User?.FindFirstValue(ClaimTypes.Name);

    public string? Role => User?.FindFirstValue(ClaimTypes.Role);

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public bool IsEnglish =>
        string.Equals(_httpContextAccessor.HttpContext?.Request.Headers["X-Language"].ToString(), "en", StringComparison.OrdinalIgnoreCase);
}
