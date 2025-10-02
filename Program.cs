using System.Collections;
using System.Text.Json.Serialization.Metadata;
using LiteBanking.Сache;
using System.Security.Claims;
using System.Text;
using gnuciDictionary;
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
using Microsoft.AspNetCore.Mvc;
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
    options.UseNpgsql(builder.Configuration["Db:ConnectionString"]));

builder.Services.AddAuthorization();

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
builder.Services.AddSingleton<IJwtService, JwtService>();

builder.Services.AddSingleton<IRandomWordGeneratorHelper, RandomWordGeneratorHelper>();
builder.Services.AddSingleton<IHashingHelper, HashingHelper>();

builder.Services.AddScoped<IBalanceRepository, BalanceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IAccountManagementService, AccountsManagementService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    var maxAttempts = 10;
    for (int i = 0; i < maxAttempts; i++)
    {
        try
        {
            Console.WriteLine($"Attempting to connect to database... Attempt {i + 1}");
            dbContext.Database.EnsureCreated();
            Console.WriteLine("Database connection successful!");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database not ready yet: {ex.Message}");
            if (i == maxAttempts - 1)
            {
                Console.WriteLine("Failed to connect to database after all attempts");
                throw;
            }
            await Task.Delay(2000); 
        }
    }
}


app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api");

var accounts = api.MapGroup("/accounts");
var balances = api.MapGroup("/balances").RequireAuthorization();

accounts.MapPost("/register", async (string name, IRandomWordGeneratorHelper helper, IAccountManagementService accounts, IJwtService jwt) =>
{
    var words = await helper.GetRandomWords(3);
    var result = await accounts.CreateAccount(name, words);
    
    string allwords = "";

    foreach (var word in words)
    {
        allwords += word + ", ";
    }

    if (result != null) return Results.Ok(new
    {
        words = allwords,
        token = jwt.GenerateToken(result.Id, "")
    });
    return Results.Conflict();
});

accounts.MapPost("/delete", async (ClaimsPrincipal user, IAccountManagementService accounts) => 
    await accounts.DeleteAccount(user.Identity.Name) ? Results.Ok() : Results.NotFound()).RequireAuthorization();

accounts.MapPost("/login", async (string name, [FromBody] List<string> keywords, IAccountManagementService accounts) =>
{
    var result = await accounts.Login(name, keywords);
    
    return result != null ? Results.Ok() :  Results.Unauthorized();
});

accounts.MapPost("/loginbytoken", (ClaimsPrincipal user) =>
{
    return Results.Ok();
}).RequireAuthorization();

app.Run();