using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.UnitTests.TestSupport;

public class FakeCurrentUserService : ICurrentUserService
{
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }
    public string? IpAddress { get; set; }
    public bool IsEnglish { get; set; }
}
