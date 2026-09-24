using EcomAPI.DTOs.Auth;

namespace EcomAPI.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);

    Task RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken);
}