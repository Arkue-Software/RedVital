using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RedVital.Identidad.Infraestructura.Persistencia;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "Este host de prueba solo admite el ambiente Development; configure el host real antes de desplegarlo.");
}

var connectionString = builder.Configuration.GetConnectionString("Identidad")
    ?? throw new InvalidOperationException(
        "Configure ConnectionStrings__Identidad para conectar con PostgreSQL.");
var issuer = builder.Configuration["Jwt:Emisor"]
    ?? throw new InvalidOperationException("Configure Jwt:Emisor.");
var audience = builder.Configuration["Jwt:Audiencia"]
    ?? throw new InvalidOperationException("Configure Jwt:Audiencia.");
var publicKeyPath = builder.Configuration["Jwt:LlavePublicaDesarrollo"]
    ?? throw new InvalidOperationException("Configure Jwt:LlavePublicaDesarrollo.");

if (!Path.IsPathFullyQualified(publicKeyPath))
{
    publicKeyPath = Path.GetFullPath(publicKeyPath, builder.Environment.ContentRootPath);
}

if (!File.Exists(publicKeyPath))
{
    throw new FileNotFoundException("No se encontró la llave pública JWT de Desarrollo.", publicKeyPath);
}

var rsa = RSA.Create();
rsa.ImportFromPem(File.ReadAllText(publicKeyPath));
var signingKey = new RsaSecurityKey(rsa);

builder.Services.AddDbContext<IdentidadDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            RoleClaimType = "role",
            NameClaimType = "sub"
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "identidad" }));
app.MapGet("/auth/check", (HttpContext context) => Results.Ok(new
{
    authenticated = true,
    subject = context.User.FindFirst("sub")?.Value,
    role = context.User.FindFirst("role")?.Value,
    jurisdiction = context.User.FindFirst("jurisdiction")?.Value
})).RequireAuthorization();

app.Run();
