using TinaStore.Domain.Entities;

namespace TinaStore.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByDocumentAsync(string documentNumber, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> SearchAsync(string term, CancellationToken ct = default);
    Task<Customer?> GetWithInvoicesAsync(int customerId, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> GetAllWithInvoicesAsync(CancellationToken ct = default);
}

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByInternalCodeAsync(string code, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<Product?> FindByNameAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> SearchAsync(string term, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetActivesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllWithNavigationAsync(CancellationToken ct = default);
}

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetWithDetailsAsync(int invoiceId, CancellationToken ct = default);
    Task<IReadOnlyList<Invoice>> GetByCustomerAsync(int customerId, CancellationToken ct = default);
    Task<IReadOnlyList<Invoice>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<string> GetNextInvoiceNumberAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Invoice>> GetAllWithCustomerAsync(CancellationToken ct = default);
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetAllWithProductsAsync(CancellationToken ct = default);
}

public interface IAccountReceivableRepository : IRepository<AccountReceivable>
{
    Task<AccountReceivable?> GetByCustomerAsync(int customerId, CancellationToken ct = default);
    Task<IReadOnlyList<AccountReceivable>> GetPendingAsync(CancellationToken ct = default);
    Task<decimal> GetTotalPendingAsync(CancellationToken ct = default);
}

public interface IExpenseRepository : IRepository<Expense>
{
    Task<IReadOnlyList<Expense>> GetAllWithNavigationAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Expense>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<Expense>> GetByCategoryAsync(int categoryId, CancellationToken ct = default);
    Task<decimal> GetTotalByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
}

public interface IDashboardRepository
{
    Task<decimal> GetSalesTodayAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<int> GetInvoiceCountTodayAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<decimal> GetSalesWeekAsync(DateTime from, CancellationToken ct = default);
    Task<decimal> GetSalesMonthAsync(DateTime from, CancellationToken ct = default);
    Task<decimal> GetTotalReceivableAsync(CancellationToken ct = default);
    Task<int> GetCustomersWithDebtAsync(CancellationToken ct = default);
    Task<decimal> GetExpensesTodayAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<decimal> GetExpensesMonthAsync(DateTime from, CancellationToken ct = default);
    Task<int> GetLowStockCountAsync(CancellationToken ct = default);
    Task<int> GetActiveProductCountAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Invoice>> GetLastInvoicesTodayAsync(DateTime from, DateTime to, int top, CancellationToken ct = default);
    Task<IReadOnlyList<AccountReceivable>> GetTopDebtorsAsync(int top, CancellationToken ct = default);
    /// <summary>Producto más vendido (unidades) en el mes actual.</summary>
    Task<(int ProductId, string ProductName, string? Sku, int Units, decimal Revenue)?> GetTopProductThisMonthAsync(DateTime mesInicio, CancellationToken ct = default);
    /// <summary>Ventas totales día a día en los últimos N días (de más antiguo a más reciente).</summary>
    Task<IReadOnlyList<(DateTime Fecha, decimal Total)>> GetSalesLast7DaysAsync(DateTime desde, CancellationToken ct = default);
}

public interface IReportRepository
{
    Task<IReadOnlyList<Invoice>> GetInvoicesByRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<InvoiceDetail>> GetSoldDetailsByRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<Expense>> GetExpensesByRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<AccountReceivable>> GetAllReceivablesAsync(CancellationToken ct = default);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken ct = default);
}

public interface IReminderRepository : IRepository<Reminder>
{
    Task<Reminder?> GetByCustomerAsync(int customerId, CancellationToken ct = default);
    Task<IReadOnlyList<ReminderHistory>> GetHistoryByCustomerAsync(int customerId, CancellationToken ct = default);
    Task AddHistoryAsync(ReminderHistory history, CancellationToken ct = default);
}
