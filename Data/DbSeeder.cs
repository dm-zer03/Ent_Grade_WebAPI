using EcomAPI.Entities;
using EcomAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcomAPI.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        const string adminEmail = "admin@ecom.com";

        var adminExists = await context.Users
            .AnyAsync(x => x.Email == adminEmail);

        if (adminExists)
        {
            return;
        }

        var admin = new User
        {
            Email = adminEmail,
            PasswordHash = passwordHasher.Hash("Admin@12345"),
            Role = "Admin"
        };

        context.Users.Add(admin);

        await context.SaveChangesAsync();
    }
}