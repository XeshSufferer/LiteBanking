using System.Text.Json.Serialization.Metadata;
using LiteBanking.Сache;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = $"{builder.Configuration["Cache:ConnectionString"]},abortConnect=false";
    options.InstanceName = "cache";
});

builder.Services.AddSingleton<ICacheService, RedisService>();

var app = builder.Build();

app.Run();