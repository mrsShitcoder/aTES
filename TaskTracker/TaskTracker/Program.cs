using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("cookie").AddCookie().AddOAuth("oauth",
    options =>
    {
        options.SignInScheme = "cookie";
        options.ClientId = "NotImplemented";
        options.ClientSecret = "NotImplemented";
        options.AuthorizationEndpoint = "https://localhost:7018";
        options.CallbackPath = "/oauth/callback";
        options.UsePkce = true;
        options.ClaimActions.MapJsonKey("sub", "sub");
        options.Events.OnCreatingTicket = async ctx =>
        {

        };
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
