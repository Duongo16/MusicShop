using MusicShop.Common.Transport;
using MusicShop.Web.Services;
using MusicShop.Web.Services.Impl;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


var tcpHost = builder.Configuration.GetValue<string>("Tcp:Host", "127.0.0.1");
var tcpPort = builder.Configuration.GetValue<int>("Tcp:Port", 5055);

builder.Services.AddSingleton(new TcpClientHelper());
builder.Services.AddScoped<IItemClientService, ItemClientService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
