
using EcomAPI.Common.Exceptions;
using EcomAPI.DTOs.Auth;
using EcomAPI.Entities;
using EcomAPI.Repositories.Interfaces;
using EcomAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcomAPI.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        // Read-only query, so disable EF Core tracking.
        var user = await _unitOfWork.Users
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Email == request.Email,
                cancellationToken);

        if (user == null)
        {
            _logger.LogWarning(
                "Login failed. User was not found.");

            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        if (!_passwordHasher.Verify(
            request.Password,
            user.PasswordHash))
        {
            _logger.LogWarning(
                "Login failed for user {UserId}. Invalid password.",
                user.Id);

            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        var response = _jwtTokenService.CreateToken(user);

        _logger.LogInformation(
            "User {UserId} logged in successfully with role {Role}.",
            user.Id,
            user.Role);

        return response;
    }

    public async Task RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        // Only checking whether the user exists.
        // AnyAsync() avoids loading the complete entity.
        var userExists = await _unitOfWork.Users
            .ExistsAsync(
                x => x.Email == request.Email,
                cancellationToken);

        if (userExists)
        {
            _logger.LogWarning(
                "Registration failed. User already exists.");

            throw new ConflictException(
                "A user with this email already exists.");
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = "Customer"
        };

        await _unitOfWork.Users.AddAsync(
            user,
            cancellationToken);

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(
                cancellationToken);

        try
        {
            // Save User first so SQL Server generates User.Id.
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            var customer = new Customer
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = string.Empty,
                LastName = string.Empty,
                PhoneNumber = string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Customers.AddAsync(
                customer,
                cancellationToken);

            // Save Customer inside the same transaction.
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            _logger.LogInformation(
                "User {UserId} registered successfully with role {Role}.",
                user.Id,
                user.Role);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            _logger.LogError(
                ex,
                "Registration failed while creating User and Customer.");

            throw;
        }
    }
}
