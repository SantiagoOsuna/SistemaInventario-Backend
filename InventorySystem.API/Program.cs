using InventorySystem.Application.Interfaces;
using InventorySystem.Application.Services;
using InventorySystem.Application.DTOs.Product;
using InventorySystem.Infrastructure.Repositories;
using InventorySystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InventorySystem.Infrastructure.Security;
using InventorySystem.Infrastructure.Services;
using InventorySystem.Application.DTOs.Auth;
using InventorySystem.Application.DTOs.SaleSimulation;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var key = builder.Configuration["Jwt:Key"]!;

// Servicios
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresa el token JWT así: Bearer {tu_token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=inventory.db"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key!)
        )
    };
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISaleSimulationRepository, SaleSimulationRepository>();
builder.Services.AddScoped<ISaleSimulationService, SaleSimulationService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

// Mapeo endpoints
app.MapGet("/products", async (IProductService service) =>
{
    return await service.GetAllAsync();
});

app.MapGet("/products/{id}", async (int id, IProductService service) =>
{
    var product = await service.GetByIdAsync(id);
    return product is null ? Results.NotFound() : Results.Ok(product);
});

app.MapPost("/products", async (CreateProductDto dto, IProductService service) =>
{
    var product = await service.CreateAsync(dto);
    return Results.Created($"/products/{product.Id}", product);
})
.RequireAuthorization();

app.MapPut("/products/{id}", async (int id, UpdateProductDto dto, IProductService service) =>
{
    var updated = await service.UpdateAsync(id, dto);
    return updated ? Results.NoContent() : Results.NotFound();
});

app.MapDelete("/products/{id}", async (int id, IProductService service) =>
{
    var deleted = await service.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.MapPost("/salesimulation", async (CreateSaleSimulationDto dto, ISaleSimulationService service) =>
{
    var result = await service.CreateAsync(dto);
    return Results.Ok(result);
});

app.MapGet("/salesimulation", async (ISaleSimulationService service) =>
{
    return await service.GetAllAsync();
});
//Authentication endpoints
app.MapPost("/auth/register", async (RegisterDto dto, IAuthService service) =>
{
    var token = await service.RegisterAsync(dto);
    return Results.Ok(token);
});

app.MapPost("/auth/login", async (LoginDto dto, IAuthService service) =>
{
    var token = await service.LoginAsync(dto);

    if (token == null)
        return Results.Unauthorized();

    return Results.Ok(token);
});

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
//app.UseHttpsRedirection();
app.Run();