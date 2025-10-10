using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MusicShop.Common.Transport;
using MusicShop.Server.Core.Services;
using MusicShop.Server.Core.Services.Impl;
using MusicShop.Server.Data;
using MusicShop.Server.Data.Repositories;
using MusicShop.Server.Data.Repositories.Impl;
using MusicShop.Server.Host.Infrastructure; 

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<TcpServerOptions>(builder.Configuration.GetSection("Tcp"));
builder.Services.AddLogging();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var conn = builder.Configuration.GetConnectionString("Default")
              ?? "Server=.;Database=MusicShopDb;Trusted_Connection=True;TrustServerCertificate=True";
    opt.UseSqlServer(conn);
});

builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemService, ItemService>();

builder.Services.AddHostedService<TcpServerHostedService>();

var host = builder.Build();
await host.RunAsync();
