using AuthService.Models;
using AuthService.Settings;
using Microsoft.AspNetCore.Identity;
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


builder.Services.AddAuthentication("cookie").AddCookie("cookie", o =>
{
    o.LoginPath = "/login";
});
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    await roleManager.CreateAsync(new ApplicationRole("Admin"));
    await roleManager.CreateAsync(new ApplicationRole("Default"));

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

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();