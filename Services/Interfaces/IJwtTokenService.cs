using EcomAPI.DTOs.Auth;
using EcomAPI.Entities;

namespace EcomAPI.Services.Interfaces;

public interface IJwtTokenService
{
    LoginResponse CreateToken(User user);
}