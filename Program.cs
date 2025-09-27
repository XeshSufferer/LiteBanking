using System.Text.Json.Serialization.Metadata;
using LiteBanking.Сache;
using System.Security.Claims;
using System.Text;
using LiteBanking.EFCoreFiles;
using LiteBanking.Repositories;
using LiteBanking.Repositories.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = $"{builder.Configuration["Cache:ConnectionString"]},abortConnect=false";
    options.InstanceName = "cache";
});

builder.Services.AddScoped<AppDbContext>();

builder.Services.AddSingleton<ICacheService, RedisService>();

builder.Services.AddScoped<IBalanceRepository, BalanceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();



var app = builder.Build();




app.Run();