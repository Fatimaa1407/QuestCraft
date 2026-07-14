namespace QuestCraft.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    string? Role { get; }
    string? IpAddress { get; }

    /// <summary>True when the client requested English content via the X-Language header.</summary>
    bool IsEnglish { get; }
}
