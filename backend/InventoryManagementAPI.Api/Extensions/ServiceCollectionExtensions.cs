using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using InventoryManagementAPI.Application.Interfaces;
using InventoryManagementAPI.Application.Validators.Auth;
using InventoryManagementAPI.Infrastructure.Data;
using InventoryManagementAPI.Infrastructure.Security;
using InventoryManagementAPI.Infrastructure.Services;
using InventoryManagementAPI.Infrastructure.Mapping;

namespace InventoryManagementAPI.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabaseServices(configuration)
            .AddAuthenticationServices(configuration)
            .AddValidationServices()
            .AddMappingServices()
            .AddBusinessServices()
            .AddSwaggerServices();

        return services;
    }

    private static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' no configurada en appsettings.json");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.CommandTimeout(30);
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            }));

        return services;
    }

    private static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        
        var secretKey = jwtSettings.GetValue<string>("SecretKey")
            ?? throw new InvalidOperationException("JwtSettings:SecretKey no configurada");
        
        var issuer = jwtSettings.GetValue<string>("Issuer") ?? "InventoryManagementAPI";
        var audience = jwtSettings.GetValue<string>("Audience") ?? "OrderManagementClient";
        var expirationMinutes = jwtSettings.GetValue<int?>("ExpirationMinutes") ?? 60;

        services.AddSingleton<IJwtTokenService>(provider =>
            new JwtTokenService(secretKey, issuer, audience, expirationMinutes));
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<UserRegisterValidator>();
        services.AddValidatorsFromAssemblyContaining<UserLoginValidator>();

        return services;
    }

    private static IServiceCollection AddMappingServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        return services;
    }

    private static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }

    private static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // Información general de la API
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "📦 Inventory Management API",
                Version = "v1.0.0",
                Description = "API REST profesional para gestión integral de inventarios"
            });

            // Documentación XML desde comentarios del código
            var xmlFile = Path.Combine(AppContext.BaseDirectory, "InventoryManagementAPI.Api.xml");
            if (File.Exists(xmlFile))
            {
                options.IncludeXmlComments(xmlFile);
            }

            // Configuración de autenticación Bearer JWT
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Ingresa el token JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            // Ordenar endpoints por tags
            options.TagActionsBy(api =>
            {
                if (api.GroupName != null)
                {
                    return new[] { api.GroupName };
                }

                var controllerActionDescriptor = api.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
                return new[] { controllerActionDescriptor?.ControllerName ?? "Unknown" };
            });
        });

        return services;
    }
}
