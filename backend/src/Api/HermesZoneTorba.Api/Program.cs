using System.Text;
using HermesZoneTorba.Api.Hubs;
using HermesZoneTorba.Api.Middleware;
using HermesZoneTorba.Application;
using HermesZoneTorba.Infrastructure;
using HermesZoneTorba.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- Logging (Serilog) — structured, correlation-id aware. See docs/15-coding-standards.md#logging. ---
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// --- Application / Infrastructure composition — see docs/01-system-architecture.md#layering ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// --- Presentation ---
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Realtime (SignalR + Redis backplane) — see docs/04-api-spec.md#realtime-signalr ---
var signalR = builder.Services.AddSignalR();
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    signalR.AddStackExchangeRedis(redisConnectionString, options =>
    {
        options.Configuration.ChannelPrefix = StackExchange.Redis.RedisChannel.Literal("hzt");
    });
}

// --- AuthN/AuthZ (JWT) — see docs/11-security.md#authn--authz ---
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] ?? "insecure-development-key-replace-me-1234567890";
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "hermes-zone-torba",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "hermes-zone-torba-clients",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey))
        };

        // SignalR sends the token via query string, not the Authorization header.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// --- CORS (web dashboard) ---
const string DashboardCorsPolicy = "DashboardCors";
builder.Services.AddCors(options => options.AddPolicy(DashboardCorsPolicy, policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"])
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

// --- Observability — see docs/17-deployment.md#observability-in-production ---
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("hermes-zone-torba-api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter());

// --- Health checks ---
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("postgres");

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(DashboardCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<AgentMonitorHub>("/hubs/agents");
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();

// Exposed for WebApplicationFactory-based integration tests — see docs/16-testing.md.
public partial class Program;
