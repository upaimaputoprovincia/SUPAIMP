using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using supai_mp.Data;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Introduza: Bearer {seu_token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
});

// MySQL + Entity Framework Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    ));

// Autorização
// Autorização
builder.Services.AddAuthorization();

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "A variável Jwt:Key não foi encontrada. Verifique a variável Jwt__Key no Railway.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Chave de assinatura
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            // Issuer
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            // Audience
            ValidateAudience = true,
            ValidAudience = jwtAudience,

            // Expiração
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            // IMPORTANTE:
            // indica ao ASP.NET Core qual claim representa a Role
            RoleClaimType = ClaimTypes.Role,

            // Indica qual claim representa o utilizador autenticado
            NameClaimType = ClaimTypes.Name
        };
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// ==========================================
// FOTOGRAFIAS NO VOLUME DO RAILWAY
// ==========================================

var fotosPath = Environment.GetEnvironmentVariable("FOTOS_PATH");

if (string.IsNullOrWhiteSpace(fotosPath))
{
    fotosPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "wwwroot",
        "fotos");
}

if (!Directory.Exists(fotosPath))
{
    Directory.CreateDirectory(fotosPath);
}

app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        fotosPath),
    RequestPath = "/fotos"
});

app.UseAuthentication();

app.UseAuthorization();

// Mapear Controllers
app.MapControllers();

app.Run();
