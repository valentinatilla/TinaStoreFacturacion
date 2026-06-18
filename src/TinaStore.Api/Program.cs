using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using TinaStore.Application;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Enums;
using TinaStore.Infrastructure;
using TinaStore.Infrastructure.Data;

// ─── Logger de arranque (captura errores antes de que la app inicie) ───────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando TinaStore API...");

    var builder = WebApplication.CreateBuilder(args);

    // ─── Serilog como proveedor de logging ────────────────────────────────────
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
        .WriteTo.Console()
        .WriteTo.File("logs/tinastore-.log", rollingInterval: RollingInterval.Day));

    // ─── Capas de la arquitectura ─────────────────────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ─── Autenticación JWT ────────────────────────────────────────────────────
    var jwtKey = builder.Configuration["Jwt:Key"]!;
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

    // ─── Controladores y documentación ───────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "TinaStore API", Version = "v1" });

        // Soporte Bearer JWT en Swagger UI
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Ingresa el token JWT. Ejemplo: Bearer {token}"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    // ─── CORS: permite que la app Web llame a la API ──────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });

    var app = builder.Build();

    // ─── Migraciones automáticas al iniciar (útil en desarrollo) ─────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Log.Information("Base de datos migrada correctamente.");

        // Crear usuario administrador inicial si no existe ningún usuario
        if (!db.Users.Any())
        {
            var hasher = scope.ServiceProvider.GetRequiredService<IAppPasswordHasher>();
            var admin = new User
            {
                FullName = "Administrador",
                Email = "admin@tinastore.com",
                Role = UserRole.Admin,
                IsActive = true,
                PasswordHash = string.Empty
            };
            admin.PasswordHash = hasher.Hash("Admin123!");
            db.Users.Add(admin);
            db.SaveChanges();
            Log.Information("Usuario administrador inicial creado: admin@tinastore.com / Admin123!");
        }
    }

    // ─── Pipeline HTTP ────────────────────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TinaStore API v1"));
    }

    app.UseSerilogRequestLogging();
    app.UseCors("AllowAll");
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.MapGet("/", () => Results.Ok(new { message = "TinaStore API funcionando ✓", version = "1.0" }));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó de forma inesperada.");
}
finally
{
    Log.CloseAndFlush();
}
