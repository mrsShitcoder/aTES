using System.Security.Claims;
using Confluent.Kafka;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Analytics;
using Analytics.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<DbService>();
builder.Services.AddSingleton<EventBus>();
builder.Services.AddSingleton<AnalyticsService>();

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

string kafkaConnection = builder.Configuration.GetConnectionString("KafkaConnection");

var consumerConfig = new ConsumerConfig
{
    BootstrapServers = kafkaConnection,
    GroupId = "users-processing-group",
    SecurityProtocol = SecurityProtocol.Plaintext,
    EnableAutoCommit = false,
    StatisticsIntervalMs = 5000,
    SessionTimeoutMs = 6000,
    AutoOffsetReset = AutoOffsetReset.Earliest,
    EnablePartitionEof = true
};

builder.Services.AddSingleton(consumerConfig);
builder.Services.AddHostedService<KafkaUserStreamConsumerService>();
builder.Services.AddHostedService<KafkaTaskStreamConsumerService>();
builder.Services.AddHostedService<KafkaTaskEventsConsumerService>();
builder.Services.AddHostedService<KafkaAccountEventsConsumerService>();

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
