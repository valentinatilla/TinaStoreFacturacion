using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Interfaces;
using TinaStore.Infrastructure.Data;
using TinaStore.Infrastructure.Repositories;
using TinaStore.Infrastructure.Services;

namespace TinaStore.Infrastructure;

/// <summary>
/// Registro de todos los servicios de infraestructura en el contenedor de dependencias.
/// Se llama desde Program.cs de la API o de la Web.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Base de datos SQLite
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=tinastore.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        // Repositorios
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IAccountReceivableRepository, AccountReceivableRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        // Servicios de infraestructura (PDF y Excel)
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IExcelService, ExcelService>();

        return services;
    }
}
