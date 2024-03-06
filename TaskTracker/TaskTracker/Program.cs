using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TaskTracker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var jwksClient = new KeyClient(new HttpClient());
SecurityKey signingKey = await jwksClient.GetSigningKeyAsync();

builder.Services.AddAuthentication("jwt").AddJwtBearer("jwt", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "https://localhost:7018",
        IssuerSigningKey = signingKey,
        RoleClaimType = ClaimTypes.Role
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = (context) =>
        {
            string? token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token.IsNullOrEmpty())
            {
                context.Request.Cookies.TryGetValue("jwt", out token);
            }

            if (!token.IsNullOrEmpty())
            {
                context.Token = token;
            }

            return Task.CompletedTask;
        },
        
        OnChallenge = async context =>
        {
            context.HandleResponse();
            var redirectUri = "https://localhost:7018/login?redirectUrl=https://localhost:7012/";
            context.Response.Redirect(redirectUri);
            await Task.CompletedTask;
        }
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
