
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using supai_mp.Data;
using supai_mp.Models;
using supai_mp.Services;
using System.Security.Claims;
using System.Text;


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

builder.Services.AddScoped<IAutorizacaoInstitucionalService,
    AutorizacaoInstitucionalService>();

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

builder.Services.AddAuthorization(options =>
{
    // ============================================================
    // ADMINISTRADOR
    // ============================================================

    options.AddPolicy("Administrador", policy =>
    {
        policy.RequireRole(
            PerfisUsuario.Administrador);
    });

    // ============================================================
    // ADMINISTRADOR + GESTOR
    // ============================================================

    options.AddPolicy("AdministradorGestor", policy =>
    {
        policy.RequireRole(
            PerfisUsuario.Administrador,
            PerfisUsuario.Gestor);
    });

    // ============================================================
    // CONSULTA GERAL
    // ============================================================

    options.AddPolicy("ConsultaGeral", policy =>
    {
        policy.RequireRole(
            PerfisUsuario.Administrador,
            PerfisUsuario.Gestor,
            PerfisUsuario.Supervisor,
            PerfisUsuario.Operador,
            PerfisUsuario.Consultor);
    });

    // ============================================================
    // SEGURANÇA PESSOAL
    // ============================================================

    options.AddPolicy("SegurancaPessoal", policy =>
    {
        policy.RequireRole(
            PerfisUsuario.Administrador,
            PerfisUsuario.Gestor,
            PerfisUsuario.TecnicoSP);
    });

    // ============================================================
    // PROTECÇÃO DE OBJECTOS
    // ============================================================

    options.AddPolicy("ProteccaoObjectos", policy =>
    {
        policy.RequireRole(
            PerfisUsuario.Administrador,
            PerfisUsuario.Gestor,
            PerfisUsuario.TecnicoPO);
    });

    // ============================================================
    // TRANSFERÊNCIA / RETIRADA — SEGURANÇA PESSOAL
    // ============================================================

    options.AddPolicy("OperacaoSegurancaPessoal", policy =>
    {
        policy.RequireRole(
            PerfisUsuario.Administrador,
            PerfisUsuario.Gestor,
            PerfisUsuario.TecnicoSP);
    });

    // ============================================================
    // TRANSFERÊNCIA / RETIRADA — PROTECÇÃO DE OBJECTOS
    // ============================================================

    options.AddPolicy("OperacaoProteccaoObjectos", policy =>
    {
        policy.RequireRole(
            PerfisUsuario.Administrador,
            PerfisUsuario.Gestor,
            PerfisUsuario.TecnicoPO);
    });
});

// ============================================================
// JWT
// ============================================================

var jwtKey =
    builder.Configuration["Jwt:Key"];

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"];

var jwtAudience =
    builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "A variável Jwt:Key não foi encontrada. " +
        "Verifique a variável Jwt__Key no Railway.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException(
        "A variável Jwt:Issuer não foi encontrada. " +
        "Verifique a variável Jwt__Issuer no Railway.");
}

if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "A variável Jwt:Audience não foi encontrada. " +
        "Verifique a variável Jwt__Audience no Railway.");
}

builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                // ====================================================
                // CHAVE DE ASSINATURA
                // ====================================================

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                // ====================================================
                // ISSUER
                // ====================================================

                ValidateIssuer = true,

                ValidIssuer =
                    jwtIssuer,

                // ====================================================
                // AUDIENCE
                // ====================================================

                ValidateAudience = true,

                ValidAudience =
                    jwtAudience,

                // ====================================================
                // EXPIRAÇÃO
                // ====================================================

                ValidateLifetime = true,

                ClockSkew =
                    TimeSpan.Zero,

                // ====================================================
                // ROLE
                // ====================================================

                RoleClaimType =
                    ClaimTypes.Role,

                // ====================================================
                // UTILIZADOR AUTENTICADO
                // ====================================================

                NameClaimType =
                    ClaimTypes.Name
            };
    });

// ============================================================
// BUILD
// ============================================================

var app = builder.Build();

// ============================================================
// MIGRATIONS + SEED
// ============================================================

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider
//        .GetRequiredService<ApplicationDbContext>();
//
//    await db.Database.MigrateAsync();
//
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
// FOTOGRAFIAS
// ============================================================

// O caminho das fotografias deve ser configurado através
// da variável de ambiente FOTOS_PATH.
//
// PRODUÇÃO (Railway):
// FOTOS_PATH deve apontar para o ponto de montagem do Volume.
//
// DESENVOLVIMENTO LOCAL:
// Se FOTOS_PATH não existir, utiliza:
// wwwroot/fotos

var fotosPath =
    Environment.GetEnvironmentVariable("FOTOS_PATH");

if (string.IsNullOrWhiteSpace(fotosPath))
{
    fotosPath =
        Path.Combine(
            app.Environment.ContentRootPath,
            "wwwroot",
            "fotos");
}

// Normaliza o caminho.
fotosPath =
    Path.GetFullPath(fotosPath);

// Garante que a pasta existe.
Directory.CreateDirectory(fotosPath);

// Regista no log qual pasta está a ser utilizada.
app.Logger.LogInformation(
    "Pasta de fotografias: {FotosPath}",
    fotosPath);

// ============================================================
// SERVIR FOTOGRAFIAS
// ============================================================

app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider =
            new PhysicalFileProvider(
                fotosPath),

        RequestPath =
            "/fotos"
    });
// ============================================================
// AUTENTICAÇÃO
// ============================================================

app.UseAuthentication();

// ============================================================
// AUTORIZAÇÃO
// ============================================================

app.UseAuthorization();

// ============================================================
// CONTROLLERS
// ============================================================

app.MapControllers();

// ============================================================
// EXECUÇÃO
// ============================================================

app.Run();
