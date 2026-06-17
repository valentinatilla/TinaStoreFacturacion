using Microsoft.EntityFrameworkCore;
using Serilog;
using TinaStore.Application;
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

    // ─── Controladores y documentación ───────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "TinaStore API", Version = "v1" });
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
