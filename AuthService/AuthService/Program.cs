using System.Security.Claims;
using AuthService.Models;
using AuthService.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);

var mongoDbSettings = builder.Configuration.GetSection("MongoDbConfig").Get<MongoDbConfig>();

if (mongoDbSettings is null)
{
    throw new Exception($"The {nameof(MongoDbConfig)} section is missing from the configuration file.");
}

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddMongoDbStores<ApplicationUser, ApplicationRole, ObjectId>
    (
        mongoDbSettings.ConnectionString, mongoDbSettings.Name
    );

Keys keys = new Keys(builder.Environment);
builder.Services.AddSingleton(keys);


builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "jwt";
        options.DefaultChallengeScheme = "jwt";
    })
    .AddJwtBearer("jwt",options =>
    {
        options.Events = new JwtBearerEvents()
        {
            OnMessageReceived = (ctx) =>
            {
                string? token = ctx.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (token.IsNullOrEmpty())
                {
                    ctx.Request.Cookies.TryGetValue("jwt", out token);
                }

                if (!token.IsNullOrEmpty())
                {
                    ctx.Token = token;
                }

                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                var redirectUri = $"https://localhost:7018/login?redirectUrl={context.Request.Path}";
                context.Response.Redirect(redirectUri);
                await Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://localhost:7018",
            IssuerSigningKey = keys.RsaSecurityKey,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    await roleManager.CreateAsync(new ApplicationRole("Admin"));
    await roleManager.CreateAsync(new ApplicationRole("Worker"));

    var config = app.Configuration.GetSection("AdministrationConfig").Get<AdministrationConfig>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var admin = new ApplicationUser(config.UserName, config.Email);
    await userManager.CreateAsync(admin, config.Password);
    await userManager.AddToRoleAsync(admin, "Admin");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();