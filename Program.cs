using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using MassTransit;
using ModuleCRM.Consumers;
using ModuleCRM.Data;
using ModuleCRM.Services;

// Load .env file
var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
if (!File.Exists(envPath))
    envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
            continue;
        var idx = trimmed.IndexOf('=');
        if (idx > 0)
            Environment.SetEnvironmentVariable(trimmed[..idx], trimmed[(idx + 1)..]);
    }
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Authentication Authentik (OAuth2/OIDC).
// Le frontend obtient un token JWT via Authentik (signinRedirect),
// le backend valide ce token via la metadata OIDC d'Authentik.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentik:Authority"]
            ?? "http://authentik.itanis.tn/application/o/erp-application/";
        options.Audience = builder.Configuration["Authentik:ClientId"]
            ?? "BGnXFXMepfj4wh0AVli40YPWPjTFs9SgBxf1Udxk";
        // En HTTP (pas HTTPS encore en dev/staging)
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };

        // Authentik ne met que les "groups" dans le token. On en derive le role ITANIS
        // (ex. groupe "client-crm" -> role "contact") et on l'ajoute comme claim de role
        // standard, pour que [Authorize(Roles = "...")] fonctionne (ex. portail client).
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = ctx =>
            {
                if (ctx.Principal?.Identity is ClaimsIdentity identity)
                {
                    var role = ctx.Principal.GetItanisRole();
                    if (!string.IsNullOrEmpty(role) && !identity.HasClaim(ClaimTypes.Role, role))
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// AgentApiService et EquipeApiService lisent la BDD locale (read-replica alimente par RabbitMQ).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AgentApiService>();
builder.Services.AddScoped<EquipeApiService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Authentik : création des comptes clients via API REST (groupe client-crm).
// Token lu depuis env var AUTHENTIK_API_TOKEN (user-secrets en dev, Secret K8s en prod).
builder.Services.AddHttpClient<AuthentikService>();

// MassTransit + RabbitMQ : Consumers ProjetSync (GestionProjet), AgentSync/EquipeSync (RH) -> read-replicas locaux.
// Plus de RequestClient auth (Authentik s'en charge cote frontend).
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProjetSyncConsumer>();
    x.AddConsumer<AgentSyncConsumer>();
    x.AddConsumer<EquipeSyncConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var rabbitSection = builder.Configuration.GetSection("RabbitMq");
        var host = ResolveConfigValue(rabbitSection["HostName"] ?? "51.254.133.231");
        var port = ushort.Parse(rabbitSection["Port"] ?? "31672");
        var username = ResolveConfigValue(rabbitSection["UserName"] ?? "admin");
        var password = ResolveConfigValue(rabbitSection["Password"] ?? "rabbitMQ-dev");

        cfg.Host(host, port, "/", h =>
        {
            h.Username(username);
            h.Password(password);
        });

        cfg.ConfigureEndpoints(ctx);
    });
});

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!
    .Replace("${DB_HOST}", Environment.GetEnvironmentVariable("DB_HOST") ?? "")
    .Replace("${DB_USER}", Environment.GetEnvironmentVariable("DB_USER") ?? "")
    .Replace("${DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "");
builder.Services.AddDbContext<CrmDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Keep database schema aligned with EF Core model on startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();

// Serve uploaded files (contracts PDFs)
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "Uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static string ResolveConfigValue(string value)
{
    if (string.IsNullOrWhiteSpace(value)) return value;
    if (value.StartsWith("${") && value.EndsWith("}"))
    {
        var envName = value[2..^1];
        return Environment.GetEnvironmentVariable(envName) ?? value;
    }
    return value;
}
