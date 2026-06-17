using Microsoft.EntityFrameworkCore;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Interfaces;
using TinaStore.Infrastructure.Data;

namespace TinaStore.Infrastructure.Repositories;

public class CustomerRepository(AppDbContext context) : Repository<Customer>(context), ICustomerRepository
{
    public async Task<Customer?> GetByDocumentAsync(string documentNumber, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(c => c.DocumentNumber == documentNumber, ct);

    public async Task<IReadOnlyList<Customer>> SearchAsync(string term, CancellationToken ct = default)
    {
        var lower = term.ToLower();
        return await DbSet
            .Where(c => c.IsActive &&
                        (c.FullName.ToLower().Contains(lower) ||
                         (c.DocumentNumber != null && c.DocumentNumber.Contains(term)) ||
                         (c.Phone != null && c.Phone.Contains(term)) ||
                         (c.Email != null && c.Email.ToLower().Contains(lower))))
            .ToListAsync(ct);
    }

    public async Task<Customer?> GetWithInvoicesAsync(int customerId, CancellationToken ct = default)
        => await DbSet
            .Include(c => c.Invoices)
            .Include(c => c.AccountReceivable)
            .FirstOrDefaultAsync(c => c.Id == customerId, ct);
}

public class ProductRepository(AppDbContext context) : Repository<Product>(context), IProductRepository
{
    public async Task<Product?> GetByInternalCodeAsync(string code, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(p => p.InternalCode == code, ct);

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(p => p.Sku == sku, ct);

    public async Task<IReadOnlyList<Product>> SearchAsync(string term, CancellationToken ct = default)
    {
        var lower = term.ToLower();
        return await DbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.IsActive &&
                        (p.Name.ToLower().Contains(lower) ||
                         p.InternalCode.Contains(term) ||
                         (p.Sku != null && p.Sku.Contains(term))))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken ct = default)
        => await DbSet
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.CurrentStock <= p.MinimumStock)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> GetActivesAsync(CancellationToken ct = default)
        => await DbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.IsActive)
            .ToListAsync(ct);
}

public class InvoiceRepository(AppDbContext context) : Repository<Invoice>(context), IInvoiceRepository
{
    public async Task<Invoice?> GetWithDetailsAsync(int invoiceId, CancellationToken ct = default)
        => await DbSet
            .Include(i => i.Customer)
            .Include(i => i.Details).ThenInclude(d => d.Product)
            .Include(i => i.Payments).ThenInclude(p => p.PaymentMethod)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, ct);

    public async Task<IReadOnlyList<Invoice>> GetByCustomerAsync(int customerId, CancellationToken ct = default)
        => await DbSet
            .Include(i => i.Details)
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Invoice>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => await DbSet
            .Include(i => i.Customer)
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(ct);

    public async Task<string> GetNextInvoiceNumberAsync(CancellationToken ct = default)
    {
        var settings = await Context.StoreSettings.FirstOrDefaultAsync(ct);
        var consecutive = settings?.InvoiceConsecutive ?? 1;
        return $"TIN-{consecutive:D6}";
    }
}

public class AccountReceivableRepository(AppDbContext context) : Repository<AccountReceivable>(context), IAccountReceivableRepository
{
    public async Task<AccountReceivable?> GetByCustomerAsync(int customerId, CancellationToken ct = default)
        => await DbSet
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.CustomerId == customerId, ct);

    public async Task<IReadOnlyList<AccountReceivable>> GetPendingAsync(CancellationToken ct = default)
        => await DbSet
            .Include(a => a.Customer)
            .Where(a => a.TotalDebt > a.TotalPaid)
            .OrderByDescending(a => a.TotalDebt - a.TotalPaid)
            .ToListAsync(ct);

    public async Task<decimal> GetTotalPendingAsync(CancellationToken ct = default)
        => await DbSet.SumAsync(a => a.TotalDebt - a.TotalPaid, ct);
}

public class ExpenseRepository(AppDbContext context) : Repository<Expense>(context), IExpenseRepository
{
    public async Task<IReadOnlyList<Expense>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => await DbSet
            .Include(e => e.ExpenseCategory)
            .Include(e => e.Supplier)
            .Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Expense>> GetByCategoryAsync(int categoryId, CancellationToken ct = default)
        => await DbSet
            .Include(e => e.ExpenseCategory)
            .Where(e => e.ExpenseCategoryId == categoryId)
            .ToListAsync(ct);

    public async Task<decimal> GetTotalByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => await DbSet
            .Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to && e.Status == Domain.Enums.ExpenseStatus.Active)
            .SumAsync(e => e.Amount, ct);
}
