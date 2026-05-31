using JwtDemo.DbContext;
using JwtDemo.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;
using JwtDemo.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using JwtDemo.Services.Auth.Interfaces;
using JwtDemo.Services.Auth.Implimentations;
using JwtDemo.Models;
using Microsoft.AspNetCore.RateLimiting;
using JwtDemo.Services.Products.Interfaces;
using JwtDemo.Services.Products.Implimentaions;
using JwtDemo.Services.Users.Interfaces;
using JwtDemo.Services.Users.Implimentations;
using JwtDemo.Services.Caching.Interfaces;
using JwtDemo.Services.Caching.Implimentations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//Add and load env variables from .env file
DotNetEnv.Env.Load();
builder.Configuration.AddEnvironmentVariables();


var connectionString = builder.Configuration["DEFAULT_DB_CONNECTIONSTRING"] ?? throw new InvalidOperationException("Database connection string is not configured.");

builder.Services.AddDbContext<AppDbContext>(o => 
    o.UseNpgsql(connectionString)
    .UseSnakeCaseNamingConvention());

builder.Services.AddIdentity<User, IdentityRole>( o =>
{
    o.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddRoles<IdentityRole>()
    .AddDefaultTokenProviders();


//Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            Message = "Too many password reset requests. Please try again after 10 minutes."
        }, token);
    };

    options.AddFixedWindowLimiter("resetPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(10);
        opt.PermitLimit = 3;
        opt.QueueLimit = 0;
    });
});

//Redis cache
builder.Services.AddStackExchangeRedisCache( o =>
{
    o.Configuration = builder.Configuration["REDIS_CONNECTIONSTRING"] ?? throw new InvalidOperationException("Redis connection string is not configured.");
    o.InstanceName = "JwtDemoCache";
});

//App services
builder.Services.AddScoped<IAuthService, AuthService>(); 
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();


//Add JwtOptions from .env file
builder.Services.Configure<JwtOptions>(o =>
{
    o.Secret = builder.Configuration["JWT_SECRET"] ?? throw new InvalidOperationException("JWT secret is not configured.");
    o.Issuer = builder.Configuration["JWT_ISSUER"] ?? throw new InvalidOperationException("JWT issuer is not configured.");
    o.Audience = builder.Configuration["JWT_AUDIENCE"] ?? throw new InvalidOperationException("JWT audience is not configured.");
    o.ExpirationInMinutes = int.Parse(builder.Configuration["JWT_EXPIRATION_MINUTES"] ?? throw new InvalidOperationException("JWT expiration time is not configured."));
});

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT_ISSUER"] ?? throw new InvalidOperationException("JWT issuer is not configured."),
        ValidAudience = builder.Configuration["JWT_AUDIENCE"] ?? throw new InvalidOperationException("JWT audience is not configured."),
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT_SECRET"] ?? throw new InvalidOperationException("JWT secret is not configured."))),
        ClockSkew = TimeSpan.Zero,

        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseRateLimiter();

app.MapOpenApi();
app.MapScalarApiReference("", o =>
{
    o.WithTitle("Jwt Demo");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
