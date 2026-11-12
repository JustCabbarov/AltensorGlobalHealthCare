using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AltensorGlobalHealthCare.Application;
using AltensorGlobalHealthCare.Application.Common;
using AltensorGlobalHealthCare.Application.Interfaces.Services;
using AltensorGlobalHealthCare.Domain.Entities;
using AltensorGlobalHealthCare.Infrastructure;
using AltensorGlobalHealthCare.Persistence;
using AltensorGlobalHealthCare.Persistence.Contexts;
using AltensorGlobalHealthCare.Persistence.Seeds;
using AltensorGlobalHealthCare.WebApi.Middlewares;
using AltensorGlobalHealthCare.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Clean Architecture Layers Dependency Injection
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// 2. Identity Configuration
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. JWT Authentication & Custom 401/403 Handling
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"]
    ?? throw new InvalidOperationException("JwtSettings:Secret konfiqurasiya edilməyib.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "AltensorGlobalHealthCare",
        ValidAudience = jwtSettings["Audience"] ?? "AltensorGlobalHealthCare",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json; charset=utf-8";
            var response = ApiResponse<object>.Fail("Bu əməliyyat üçün giriş etməlisiniz.", "UNAUTHORIZED");
            return context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json; charset=utf-8";
            var response = ApiResponse<object>.Fail("Bu resursa baxmaq üçün səlahiyyətiniz çatmır.", "FORBIDDEN");
            return context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    };
});

builder.Services.AddAuthorization();

// 4. Current User & Context
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// 5. Automatic Model Validation Error Response mapping to ApiResponse
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(m => m.Value?.Errors.Count > 0)
            .SelectMany(m => m.Value!.Errors.Select(e => new ErrorDetail(m.Key, e.ErrorMessage)))
            .ToList();

        var response = ApiResponse<object>.Fail("Daxil edilən məlumatlarda səhvlər var.", "VALIDATION_ERROR", errors);
        return new BadRequestObjectResult(response);
    };
});

// 6. CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 7. Swagger Documentation with JWT Bearer Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AltensorGlobalHealthCare Medical Tourism API",
        Version = "v1",
        Description = "ZagrebMed modelində qurulmuş rəqəmsal səhiyyə və tibbi turizm platforması (.NET 8 Clean Architecture)"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Tokeninizi daxil edin (Məsələn: Bearer {token})",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();

// 8. Custom Global Exception & Language Middlewares
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLanguageMiddleware>();

// Swagger UI directly at root: http://localhost:5000/
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AltensorGlobalHealthCare API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors("AllowAll");

// Serve public uploads (clinics logos, covers, doctor photos) from wwwroot
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 9. Auto Migration & Database Seeding on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        if (context.Database.CanConnect())
        {
            await context.Database.EnsureCreatedAsync();
            await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Verilənlər bazası qoşulmasında bildiriş (PostgreSQL sonradan işə salına bilər): {Message}", ex.Message);
    }
}

app.Run();
