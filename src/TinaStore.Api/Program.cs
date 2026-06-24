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

    // ─── Puerto dinámico Railway (Railway inyecta $PORT en runtime) ───────────
    // Solo sobreescribir cuando la variable PORT esté definida (Railway).
    // En desarrollo se usan las URLs de launchSettings.json.
    var railwayPort = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrEmpty(railwayPort))
    {
        builder.WebHost.UseUrls($"http://0.0.0.0:{railwayPort}");
    }

    // ─── Serilog como proveedor de logging ────────────────────────────────────
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
        .WriteTo.Console()
        .WriteTo.File("logs/tinastore-.log", rollingInterval: RollingInterval.Day));

    // ─── Capas de la arquitectura ─────────────────────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Registra IStoreSettingsService con la ruta física de wwwroot
    var webRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
    builder.Services.AddScoped<TinaStore.Application.Interfaces.IStoreSettingsService>(sp =>
        new TinaStore.Application.Services.StoreSettingsService(
            sp.GetRequiredService<TinaStore.Domain.Interfaces.IRepository<TinaStore.Domain.Entities.StoreSettings>>(),
            webRootPath));

    // ─── Autenticación JWT ────────────────────────────────────────────────────
    var jwtKey = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrWhiteSpace(jwtKey))
        throw new InvalidOperationException(
            "La clave JWT (Jwt:Key) no está configurada. " +
            "Establécela mediante user-secrets o una variable de entorno antes de iniciar la aplicación.");
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
    // En producción configura Cors__AllowedOrigins con la URL de Railway del Web.
    // Ejemplo: Cors__AllowedOrigins=https://tinastore-web.up.railway.app
    var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"];
    var esProduccion   = builder.Environment.IsProduction();

    if (esProduccion && string.IsNullOrWhiteSpace(allowedOrigins))
        Log.Warning("⚠️  CORS no configurado en producción. " +
                    "Establece la variable de entorno Cors__AllowedOrigins para restringir los orígenes permitidos. " +
                    "Actualmente se permite cualquier origen, lo que puede representar un riesgo de seguridad.");

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            if (!string.IsNullOrWhiteSpace(allowedOrigins))
                policy.WithOrigins(allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            else
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // ─── Migraciones automáticas al iniciar (útil en desarrollo) ─────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Log.Information("Base de datos migrada correctamente.");

        // Crear usuario administrador inicial
        // En producción, configura App__AdminEmail y App__AdminPassword como env vars.
        var adminEmail    = builder.Configuration["App:AdminEmail"]    ?? "admin@tinastore.com";
        var adminPassword = builder.Configuration["App:AdminPassword"] ?? "Admin123!";
        if (esProduccion && builder.Configuration["App:AdminPassword"] is null)
            Log.Warning("⚠️  La contraseña del administrador inicial no está configurada. " +
                        "Se usará la contraseña por defecto, lo que supone un riesgo de seguridad en producción. " +
                        "Establece la variable de entorno App__AdminPassword antes de iniciar la aplicación.");
        if (!db.Users.Any(u => u.Email == adminEmail))
        {
            var hasher = scope.ServiceProvider.GetRequiredService<IAppPasswordHasher>();
            var admin = new User
            {
                FullName = "Administrador",
                Email = adminEmail,
                Role = UserRole.Admin,
                IsActive = true,
                PasswordHash = string.Empty
            };
            admin.PasswordHash = hasher.Hash(adminPassword);
            db.Users.Add(admin);
            db.SaveChanges();
            Log.Information("Usuario administrador inicial creado: {Email}", adminEmail);
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
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.MapGet("/", () => Results.Ok(new
    {
        message     = "TinaStore API funcionando ✓",
        version     = "1.0.0",
        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
    }));

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
