using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using server.Data;
using server.Models;
using System.Text;
using System.Text.Json.Serialization;
using Stripe;

// Création du builder pour configurer services et pipeline
var builder = WebApplication.CreateBuilder(args);

// === Configuration CORS ===
// Permet de définir une politique CORS nommée "AllowAngularApp" autorisant uniquement
// les requêtes provenant de http://localhost:4200, avec tous les headers et méthodes.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// === Chargement de la section JwtSettings ===
// Lie la classe JwtSettings aux paramètres "JwtSettings" dans appsettings.json
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// === Configuration du DbContext Entity Framework ===
// Ajoute AppDbContext en tant que service, configuré pour utiliser SQL Server
// et la chaîne de connexion "DefaultConnection" depuis appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// === Ajout des contrôleurs et configuration JSON ===
// - Active les contrôleurs MVC/Web API
// - Ajoute un convertisseur pour sérialiser les enums sous forme de chaînes
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Documentation OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === Configuration de l’authentification JWT ===
// Utilise JwtBearer et les paramètres définis dans JwtSettings pour valider
// l’émetteur, le public, la durée de vie et la clé de signature des tokens
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? ""))
        };
    });

// === Configuration de Stripe ===
// Lie la classe StripeSettings à la section "Stripe" de appsettings.json
// puis initialise la clé secrète Stripe pour les appels API
builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Activation du middleware d’autorisation
builder.Services.AddAuthorization();

var app = builder.Build();

// === Pipeline HTTP ===
if (app.Environment.IsDevelopment())
{
    // En développement, active Swagger pour la documentation interactive
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Désactivé pour l’instant : redirection automatique vers HTTPS
// app.UseHttpsRedirection();

// Préparation du dossier de stockage des fichiers uploadés
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads");

if (!Directory.Exists(uploadsPath))
{
    // Crée le dossier s’il n’existe pas
    Directory.CreateDirectory(uploadsPath);
}

// Application de la politique CORS définie plus haut
app.UseCors("AllowAngularApp");

// Activation des middlewares d’authentification et d’autorisation
app.UseAuthentication();
app.UseAuthorization();

// Configuration des fichiers statiques pour servir les uploads depuis /uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// Mappe les contrôleurs sur les routes HTTP
app.MapControllers();

// Démarre l’application
app.Run();
