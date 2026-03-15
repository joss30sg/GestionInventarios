using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using Serilog;
using InventoryManagementAPI.Application.Interfaces;
using InventoryManagementAPI.Application.Validators.Auth;
using InventoryManagementAPI.Infrastructure.Data;
using InventoryManagementAPI.Infrastructure.Security;
using InventoryManagementAPI.Infrastructure.Services;
using InventoryManagementAPI.Infrastructure.Mapping;
using InventoryManagementAPI.Api.Middleware;
using InventoryManagementAPI.Api.Extensions;
using InventoryManagementAPI.Infrastructure.Hubs;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/inventory-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddScoped<IInventoryNotificationService, InventoryNotificationService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

await app.ApplyDatabaseMigrationsAsync();
app.UseApplicationPipeline();
app.Run();
