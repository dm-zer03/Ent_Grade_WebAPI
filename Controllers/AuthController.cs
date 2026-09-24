using Asp.Versioning;
using EcomAPI.DTOs.Auth;
using EcomAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomAPI.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(
            request,
            cancellationToken);

        return Ok(response);
    }


    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
    RegisterRequest request,
    CancellationToken cancellationToken)
    {
        await _authService.RegisterAsync(
            request,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created);
    }
}