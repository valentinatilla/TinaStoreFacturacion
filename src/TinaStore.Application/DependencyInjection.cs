using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TinaStore.Application.Interfaces;
using TinaStore.Application.Services;

namespace TinaStore.Application;

/// <summary>
/// Registro de todos los servicios de la capa Application en el contenedor de dependencias.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Validadores de FluentValidation (registro automático por ensamblado)
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Servicios de aplicación — casos de uso por módulo
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IStoreSettingsService, StoreSettingsService>();

        return services;
    }
}
