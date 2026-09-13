using InventarioUPS.API.Data;
using InventarioUPS.API.Repositories;
using InventarioUPS.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


// =====================================================
// JWT
// =====================================================

var jwtKey =
    builder.Configuration["Jwt:Key"];

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"];

var jwtAudience =
    builder.Configuration["Jwt:Audience"];


builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtKey!
                        )
                    ),

                ValidateIssuer = true,

                ValidIssuer =
                    jwtIssuer,

                ValidateAudience = true,

                ValidAudience =
                    jwtAudience,

                ValidateLifetime = true,

                ClockSkew =
                    TimeSpan.Zero
            };
    });


builder.Services.AddAuthorization();


// =====================================================
// CONTROLLERS
// =====================================================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });


// =====================================================
// DATABASE
// =====================================================

builder.Services.AddDbContext<InventarioUPSDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection")));


// =====================================================
// CORS
// =====================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "React",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


// =====================================================
// REPOSITORIES
// =====================================================

builder.Services.AddScoped<
    IAgenciaRepository,
    AgenciaRepository>();

builder.Services.AddScoped<
    IUPSRepository,
    UPSRepository>();

builder.Services.AddScoped<
    IMantenimientoRepository,
    MantenimientoRepository>();

builder.Services.AddScoped<
    IUsuarioRepository,
    UsuarioRepository>();

builder.Services.AddScoped<
    IConfiguracionPresupuestoRepository,
    ConfiguracionPresupuestoRepository>();


// =====================================================
// SERVICES
// =====================================================

builder.Services.AddScoped<
    IAgenciaService,
    AgenciaService>();

builder.Services.AddScoped<
    IUPSService,
    UPSService>();

builder.Services.AddScoped<
    IMantenimientoService,
    MantenimientoService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IConfiguracionPresupuestoService,
    ConfiguracionPresupuestoService>();


// =====================================================
// PASSWORD HASHER
// =====================================================

builder.Services.AddScoped<
    IPasswordHasher<InventarioUPS.API.Models.Usuario>,
    PasswordHasher<InventarioUPS.API.Models.Usuario>>();


// =====================================================
// JWT SERVICE
// =====================================================

builder.Services.AddScoped<
    IJwtService,
    JwtService>();


// =====================================================
// SWAGGER
// =====================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description =
                "Ingrese el token JWT. Ejemplo: Bearer {token}"
        });


    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});


var app = builder.Build();


// =====================================================
// CARGA INICIAL DE CATÁLOGOS
// =====================================================

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
            .GetRequiredService<
                InventarioUPSDbContext>();

    await DbSeeder.SeedAsync(context);
}


// =====================================================
// PIPELINE
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors("React");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();