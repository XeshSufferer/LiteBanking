using System.Text.Json.Serialization.Metadata;
using LiteBanking.Сache;
using System.Security.Claims;
using System.Text;
using LiteBanking.EFCoreFiles;
using LiteBanking.Helpers;
using LiteBanking.Helpers.Interfaces;
using LiteBanking.Repositories;
using LiteBanking.Repositories.Interfaces;
using LiteBanking.Services;
using LiteBanking.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = $"{builder.Configuration["Cache:ConnectionString"]},abortConnect=false";
    options.InstanceName = "cache";
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(builder.Configuration["Db:ConnectionString"])));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
        
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            }
        };
    });




builder.Services.AddScoped<AppDbContext>();

builder.Services.AddSingleton<ICacheService, RedisService>();

builder.Services.AddSingleton<IRandomWordGeneratorHelper, RandomWordGeneratorHelper>();
builder.Services.AddSingleton<IHashingHelper, HashingHelper>();

builder.Services.AddScoped<IBalanceRepository, BalanceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


builder.Services.AddScoped<IAccountManagementService, AccountsManagementService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


var api = app.MapGroup("/api");

var accounts = api.MapGroup("/accounts");
var balances = api.MapGroup("/balances").RequireAuthorization();


IRandomWordGeneratorHelper _randomWordHelper = app.Services.GetRequiredService<IRandomWordGeneratorHelper>();

// Services

IAccountManagementService _accounts = app.Services.GetRequiredService<IAccountManagementService>();



accounts.MapPost("/register", async (string name) =>
{
    var words = await _randomWordHelper.GetRandomWords(3);
    var result = await _accounts.CreateAccount(name, words);
    if (result != null) return Results.Created();
    return Results.Conflict();
});

accounts.MapPost("/delete", async (ClaimsPrincipal user) => 
    await _accounts.DeleteAccount(user.Identity.Name) ? Results.Ok() : Results.NotFound()).RequireAuthorization();


accounts.MapPost("/login", async (string name, List<string> keywords) =>
{
    var result = await _accounts.Login(name, keywords);
    
    return result != null ? Results.Ok() :  Results.Unauthorized();
});

accounts.MapPost("/loginbytoken", (ClaimsPrincipal user) =>
{
    return Results.Ok();
}).RequireAuthorization();


app.Run();