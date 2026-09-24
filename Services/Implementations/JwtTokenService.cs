using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EcomAPI.DTOs.Auth;
using EcomAPI.Entities;
using EcomAPI.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace EcomAPI.Services.Implementations;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LoginResponse CreateToken(User user)
    {
        var key = _configuration["Jwt:Key"]!;

        var expiresAt = DateTime.UtcNow.AddMinutes(
            Convert.ToDouble(
                _configuration["Jwt:ExpiryMinutes"]));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler()
                .WriteToken(token),

            ExpiresAt = expiresAt
        };
    }
}