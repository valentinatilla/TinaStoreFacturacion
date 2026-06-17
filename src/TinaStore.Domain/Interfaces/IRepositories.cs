using TinaStore.Domain.Entities;

namespace TinaStore.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByDocumentAsync(string documentNumber, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> SearchAsync(string term, CancellationToken ct = default);
    Task<Customer?> GetWithInvoicesAsync(int customerId, CancellationToken ct = default);
}

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByInternalCodeAsync(string code, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> SearchAsync(string term, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetActivesAsync(CancellationToken ct = default);
}

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetWithDetailsAsync(int invoiceId, CancellationToken ct = default);
    Task<IReadOnlyList<Invoice>> GetByCustomerAsync(int customerId, CancellationToken ct = default);
    Task<IReadOnlyList<Invoice>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<string> GetNextInvoiceNumberAsync(CancellationToken ct = default);
}

public interface IAccountReceivableRepository : IRepository<AccountReceivable>
{
    Task<AccountReceivable?> GetByCustomerAsync(int customerId, CancellationToken ct = default);
    Task<IReadOnlyList<AccountReceivable>> GetPendingAsync(CancellationToken ct = default);
    Task<decimal> GetTotalPendingAsync(CancellationToken ct = default);
}

public interface IExpenseRepository : IRepository<Expense>
{
    Task<IReadOnlyList<Expense>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<Expense>> GetByCategoryAsync(int categoryId, CancellationToken ct = default);
    Task<decimal> GetTotalByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
}
