using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Auth.Commands.Login;
using QuestCraft.Application.Features.Auth.Commands.Register;
using QuestCraft.Application.Features.Auth.Dtos;
using QuestCraft.Domain.Entities;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.IntegrationTests.TestSupport;

namespace QuestCraft.IntegrationTests.Auth;

// Full HTTP round-trip through the real controller/MediatR/validator pipeline — confirms the
// email-format fix (RegisterCommandValidator) is enforced at the actual API boundary, not just
// in the validator unit tests. Only 3 auth requests total across this class, well under the
// 5-per-minute-per-IP rate limit on /api/auth/* so the suite doesn't self-rate-limit.
public class RegisterLoginFlowTests : IClassFixture<QuestCraftWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly QuestCraftWebApplicationFactory _factory;
    private HttpClient _client = default!;

    public RegisterLoginFlowTests(QuestCraftWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!await db.Roles.AnyAsync(r => r.Name == "Student"))
        {
            db.Roles.Add(new Role { Name = "Student" });
            await db.SaveChangesAsync();
        }

        _client = _factory.CreateClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_ThenLogin_ReturnsAccessToken()
    {
        var email = $"flow_{Guid.NewGuid():N}@gmail.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterCommand(
            Username: $"flowuser{Guid.NewGuid():N}"[..20], FirstName: "Flow", LastName: "Test", Email: email, Password: "Passw0rd!"));

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(JsonOptions);
        Assert.True(registerBody!.Success);
        Assert.Equal(email, registerBody.Data!.Email);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginCommand(email, "Passw0rd!"));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>(JsonOptions);
        Assert.True(loginBody!.Success);
        Assert.False(string.IsNullOrWhiteSpace(loginBody.Data!.AccessToken));
    }

    [Fact]
    public async Task Register_InvalidEmailFormat_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterCommand(
            Username: $"bademail{Guid.NewGuid():N}"[..20], FirstName: "Bad", LastName: "Email", Email: "ali@gmailcom", Password: "Passw0rd!"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
