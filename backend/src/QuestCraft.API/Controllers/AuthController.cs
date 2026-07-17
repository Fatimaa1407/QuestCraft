using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Auth.Commands.Login;
using QuestCraft.Application.Features.Auth.Commands.Logout;
using QuestCraft.Application.Features.Auth.Commands.Refresh;
using QuestCraft.Application.Features.Auth.Commands.Register;
using QuestCraft.Application.Features.Auth.Dtos;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";

    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<UserDto>.Ok(result, "Qeydiyyat uğurludur. İndi daxil ola bilərsiniz."));
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Giriş uğurludur."));
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(ApiResponse<object>.Fail("Refresh token tapılmadı."));
        }

        var result = await _mediator.Send(new RefreshTokenCommand(refreshToken), cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _mediator.Send(new LogoutCommand(refreshToken), cancellationToken);
        }

        Response.Cookies.Delete(RefreshTokenCookieName);
        return Ok(ApiResponse<object?>.Ok(null, "Çıxış edildi."));
    }

    private void SetRefreshTokenCookie(string token, DateTime expiresAtUtc)
    {
        // SameSite=None: the SPA and API are intentionally on different origins (different port in dev,
        // likely different subdomains in production), which browsers treat as cross-site — schemeful
        // same-site rules would even block SameSite=Strict here since dev is http (SPA) vs https (API).
        // Secure=true keeps it HTTPS-only despite the relaxed SameSite.
        Response.Cookies.Append(RefreshTokenCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expiresAtUtc,
        });
    }
}
