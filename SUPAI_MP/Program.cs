using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using supai_mp.Data;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONTROLLERS
// ============================================================

builder.Services.AddControllers();

// ============================================================
// SWAGGER
// ============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduza: Bearer {seu_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================================
// MYSQL + ENTITY FRAMEWORK CORE
// ============================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

// ============================================================
// AUTORIZAÇÃO
// ============================================================

builder.Services.AddAuthorization();

// ============================================================
// JWT
// ============================================================

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "A variável Jwt:Key não foi encontrada. " +
        "Verifique a variável Jwt__Key no Railway.");
}

builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                // Chave de assinatura
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
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

                // Role
                RoleClaimType = ClaimTypes.Role,

                // Utilizador autenticado
                NameClaimType = ClaimTypes.Name
            };
    });

var app = builder.Build();

// ============================================================
// MIGRATIONS + SEED
// ============================================================

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider
//        .GetRequiredService<ApplicationDbContext>();

//    await db.Database.MigrateAsync();

//    await OrganizacaoSeed.SeedAsync(db);
//}

// ============================================================
// SWAGGER
// ============================================================

app.UseSwagger();
app.UseSwaggerUI();

// ============================================================
// HTTPS
// ============================================================

app.UseHttpsRedirection();

// ============================================================
// FOTOGRAFIAS NO VOLUME DO RAILWAY
// ============================================================

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
    FileProvider =
        new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
            fotosPath),

    RequestPath = "/fotos"
});

// ============================================================
// AUTENTICAÇÃO E AUTORIZAÇÃO
// ============================================================

app.UseAuthentication();
app.UseAuthorization();

// ============================================================
// CONTROLLERS
// ============================================================

app.MapControllers();

app.Run();