using LiteBanking.Сache;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using LiteBanking.EFCoreFiles;
using LiteBanking.Helpers;
using LiteBanking.Helpers.Interfaces;
using LiteBanking.Models.DTO;
using LiteBanking.Repositories;
using LiteBanking.Repositories.Interfaces;
using LiteBanking.Services;
using LiteBanking.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = $"{builder.Configuration["Cache:ConnectionString"]},abortConnect=false";
    options.InstanceName = "cache";
});

builder.Services.Configure<RouteOptions>(o =>
    o.SetParameterPolicy<RegexInlineRouteConstraint>("regex"));
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "LiteBanking API", Version = "v1" });
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 20,
                QueueLimit = 0,        
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });
    
    options.OnRejected = async (ctx, token) =>
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await ctx.HttpContext.Response.WriteAsync("Too many requests", token);
    };
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

builder.Services.AddScoped<IBalanceManagementService, BalanceManagementService>();
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
app.UseRateLimiter();
app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health");

var api = app.MapGroup("/api");

var accounts = api.MapGroup("/accounts");
var balances = api.MapGroup("/balances").RequireAuthorization();

accounts.MapPost("/register", async (RegisterRequestDTO req, IRandomWordGeneratorHelper helper, IAccountManagementService accounts, IJwtService jwt) =>
{
    var words = await helper.GetRandomWords(3);
    var result = await accounts.CreateAccount(req.Name, words);
    
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
    return Results.InternalServerError("Account creation failed. Try Again later");
});

accounts.MapPost("/delete", async (ClaimsPrincipal user, IAccountManagementService accounts) => 
    await accounts.DeleteAccount(user.Identity.Name) ? Results.Ok() : Results.InternalServerError("Account delete error")).RequireAuthorization();

accounts.MapPost("/login", async (LoginRequestDTO req, IAccountManagementService accounts, IJwtService jwt) =>
{
    var result = await accounts.Login(req.Name, req.Keywords);
    
    return result != null ? Results.Ok(jwt.GenerateToken(result.Id, "")) :  Results.Unauthorized();
});

accounts.MapPost("/checktoken", (ClaimsPrincipal user) =>
{
    return Results.Ok();
}).RequireAuthorization();


balances.MapPost("/send", async (ClaimsPrincipal user, SendMoneyRequestDTO req, CancellationToken ct, IBalanceManagementService balances) =>
{
    return await balances.Send(req.From, req.To, req.Amount,  long.Parse(user.Identity.Name), ct) ? Results.Ok() : Results.InternalServerError("Money Sending Error");
}).RequireAuthorization();

balances.MapPost("/create", async (ClaimsPrincipal user, IBalanceManagementService balances,CancellationToken ct) =>
{
    var result = await balances.CreateBalance(long.Parse(user.Identity.Name));
    
    return result.Item1 ? Results.Ok() : Results.InternalServerError("Create Balance Error");   
}).RequireAuthorization();

balances.MapPost("/getInfo", async (ClaimsPrincipal user, GetMoneyRequestDTO req, IBalanceManagementService balances) =>
{
    return await balances.GetBalanceAmount(req.BalanceId, long.Parse(user.Identity.Name));
}).RequireAuthorization();

balances.MapPost("/getMyBalancesCount", async (ClaimsPrincipal user, CancellationToken ct,IAccountManagementService accounts) =>
{
    return Results.Ok((await accounts.GetUserById(user.Identity.Name, ct)).Balances.Count);
}).RequireAuthorization();

app.Run();