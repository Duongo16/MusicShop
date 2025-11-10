using Microsoft.AspNetCore.Authentication.Cookies;
using MusicShop.Common.Transport;
using MusicShop.Web.Services;
using MusicShop.Web.Services.Impl;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromDays(7); 
        options.SlidingExpiration = true;       
    });

var tcpHost = builder.Configuration.GetValue<string>("Tcp:Host", "127.0.0.1");
var tcpPort = builder.Configuration.GetValue<int>("Tcp:Port", 5055);

builder.Services.AddSingleton(new TcpClientHelper());
builder.Services.AddScoped<IItemClientService, ItemClientService>();
builder.Services.AddScoped<IAuthClientService, AuthClientService>();

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
