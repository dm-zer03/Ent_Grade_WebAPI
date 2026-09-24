
using Asp.Versioning;
using EcomAPI.Data;
using System.Reflection;
using EcomAPI.Mapping;
using EcomAPI.Middleware;
using EcomAPI.Repositories.Implementations;
using EcomAPI.Repositories.Interfaces;
using EcomAPI.Services.Implementations;
using EcomAPI.Services.Interfaces;
using EcomAPI.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog file logging.
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(
                "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true)
            .CreateLogger();

        // Connect Serilog with ASP.NET Core ILogger.
        builder.Host.UseSerilog();

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.IncludeXmlComments(
                Path.Combine(
                    AppContext.BaseDirectory,
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));

            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description =
                        "Enter your JWT token. Example: Bearer {token}"
                });

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
        });

        // Global exception handling.
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        // Database.
        builder.Services.AddDbContext<ApplicationDbContext>(
            options =>
                options.UseSqlServer(
                    builder.Configuration
                        .GetConnectionString("DefaultConnection"))
                .EnableDetailedErrors());

        // Health check.
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(
                name: "ECommerceDb");

        // Repositories and services.
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<ICustomerService, CustomerService>();
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IOrderService, OrderService>();

        // FluentValidation.
        builder.Services.AddFluentValidationAutoValidation();

        builder.Services.AddValidatorsFromAssemblyContaining<
            CreateCustomerRequestValidator>();

        // JWT configuration.
        var jwtKey = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT key is missing.");

        builder.Services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer =
                            builder.Configuration["Jwt:Issuer"],

                        ValidAudience =
                            builder.Configuration["Jwt:Audience"],

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwtKey))
                    };
            });

        // Authentication services.
        builder.Services.AddScoped<
            IPasswordHasher,
            PasswordHasher>();

        builder.Services.AddScoped<
            IAuthService,
            AuthService>();

        builder.Services.AddScoped<
            IJwtTokenService,
            JwtTokenService>();

        builder.Services.AddAuthorization();

        // API Versioning.
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion =
                new ApiVersion(1, 0);

            options.AssumeDefaultVersionWhenUnspecified = true;

            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";

            options.SubstituteApiVersionInUrl = true;
        });

        // AutoMapper.
        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(
                typeof(MappingProfile).Assembly);
        });

        // Rate Limiting.
        builder.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter =
                PartitionedRateLimiter
                    .Create<HttpContext, string>(
                        context =>
                            RateLimitPartition
                                .GetFixedWindowLimiter(
                                    partitionKey:
                                        context.Connection
                                            .RemoteIpAddress
                                            ?.ToString()
                                        ?? "unknown",

                                    factory: _ =>
                                        new FixedWindowRateLimiterOptions
                                        {
                                            PermitLimit = 100,

                                            Window =
                                                TimeSpan.FromMinutes(1),

                                            QueueLimit = 0
                                        }));

            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;
        });

        var app = builder.Build();

        // Seed database.
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var passwordHasher = scope.ServiceProvider
                .GetRequiredService<IPasswordHasher>();

            await DbSeeder.SeedAsync(
                context,
                passwordHasher);
        }

        // Middleware pipeline.
        app.UseRateLimiter();

        app.UseExceptionHandler();

        app.UseMiddleware<RequestLoggingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.MapHealthChecks("/health");

        await app.RunAsync();
    }
}
