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

    public async Task<IReadOnlyList<Product>> GetAllWithNavigationAsync(CancellationToken ct = default)
        => await DbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier)
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

    public async Task<IReadOnlyList<Invoice>> GetAllWithCustomerAsync(CancellationToken ct = default)
        => await DbSet
            .Include(i => i.Customer)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(ct);
}

public class CategoryRepository(AppDbContext context) : Repository<Category>(context), ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllWithProductsAsync(CancellationToken ct = default)
        => await DbSet
            .Include(c => c.Products)
            .ToListAsync(ct);
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

public class DashboardRepository(AppDbContext context) : IDashboardRepository
{
    private readonly AppDbContext _db = context;

    public async Task<decimal> GetSalesTodayAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => await _db.Invoices
            .Where(i => i.Status != Domain.Enums.InvoiceStatus.Cancelled && i.InvoiceDate >= from && i.InvoiceDate <= to)
            .SumAsync(i => i.Total, ct);

    public async Task<int> GetInvoiceCountTodayAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => await _db.Invoices
            .CountAsync(i => i.Status != Domain.Enums.InvoiceStatus.Cancelled && i.InvoiceDate >= from && i.InvoiceDate <= to, ct);

    public async Task<decimal> GetSalesWeekAsync(DateTime from, CancellationToken ct = default)
        => await _db.Invoices
            .Where(i => i.Status != Domain.Enums.InvoiceStatus.Cancelled && i.InvoiceDate >= from)
            .SumAsync(i => i.Total, ct);

    public async Task<decimal> GetSalesMonthAsync(DateTime from, CancellationToken ct = default)
        => await _db.Invoices
            .Where(i => i.Status != Domain.Enums.InvoiceStatus.Cancelled && i.InvoiceDate >= from)
            .SumAsync(i => i.Total, ct);

    public async Task<decimal> GetTotalReceivableAsync(CancellationToken ct = default)
        => await _db.AccountsReceivable
            .Where(a => a.TotalDebt > a.TotalPaid)
            .SumAsync(a => a.TotalDebt - a.TotalPaid, ct);

    public async Task<int> GetCustomersWithDebtAsync(CancellationToken ct = default)
        => await _db.AccountsReceivable.CountAsync(a => a.TotalDebt > a.TotalPaid, ct);

    public async Task<decimal> GetExpensesTodayAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => await _db.Expenses
            .Where(e => e.Status == Domain.Enums.ExpenseStatus.Active && e.ExpenseDate >= from && e.ExpenseDate <= to)
            .SumAsync(e => e.Amount, ct);

    public async Task<decimal> GetExpensesMonthAsync(DateTime from, CancellationToken ct = default)
        => await _db.Expenses
            .Where(e => e.Status == Domain.Enums.ExpenseStatus.Active && e.ExpenseDate >= from)
            .SumAsync(e => e.Amount, ct);

    public async Task<int> GetLowStockCountAsync(CancellationToken ct = default)
        => await _db.Products.CountAsync(p => p.IsActive && !p.IsDeleted && p.CurrentStock <= p.MinimumStock, ct);

    public async Task<int> GetActiveProductCountAsync(CancellationToken ct = default)
        => await _db.Products.CountAsync(p => p.IsActive && !p.IsDeleted, ct);

    public async Task<IReadOnlyList<Invoice>> GetLastInvoicesTodayAsync(DateTime from, DateTime to, int top, CancellationToken ct = default)
        => await _db.Invoices
            .Include(i => i.Customer)
            .Where(i => i.Status != Domain.Enums.InvoiceStatus.Cancelled && i.InvoiceDate >= from && i.InvoiceDate <= to)
            .OrderByDescending(i => i.InvoiceDate)
            .Take(top)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AccountReceivable>> GetTopDebtorsAsync(int top, CancellationToken ct = default)
        => await _db.AccountsReceivable
            .Include(a => a.Customer)
            .Where(a => a.TotalDebt > a.TotalPaid)
            .OrderByDescending(a => a.TotalDebt - a.TotalPaid)
            .Take(top)
            .ToListAsync(ct);
}

public class ReportRepository(AppDbContext context) : IReportRepository
{
    private readonly AppDbContext _db = context;

    public async Task<IReadOnlyList<Invoice>> GetInvoicesByRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => await _db.Invoices
            .Include(i => i.Customer)
            .Where(i => i.Status != Domain.Enums.InvoiceStatus.Cancelled
                        && i.InvoiceDate >= from && i.InvoiceDate <= to)
            .OrderBy(i => i.InvoiceDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<InvoiceDetail>> GetSoldDetailsByRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => await _db.InvoiceDetails
            .Include(d => d.Invoice)
            .Include(d => d.Product)
            .Where(d => d.Invoice!.Status != Domain.Enums.InvoiceStatus.Cancelled
                        && d.Invoice.InvoiceDate >= from && d.Invoice.InvoiceDate <= to)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Expense>> GetExpensesByRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => await _db.Expenses
            .Include(e => e.ExpenseCategory)
            .Where(e => e.Status == Domain.Enums.ExpenseStatus.Active
                        && e.ExpenseDate >= from && e.ExpenseDate <= to)
            .OrderBy(e => e.ExpenseDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AccountReceivable>> GetAllReceivablesAsync(CancellationToken ct = default)
        => await _db.AccountsReceivable
            .Include(a => a.Customer)
                .ThenInclude(c => c.Invoices)
            .Where(a => a.TotalDebt > a.TotalPaid)
            .OrderByDescending(a => a.TotalDebt - a.TotalPaid)
            .ToListAsync(ct);
}

// ─── UserRepository ───────────────────────────────────────────────────────────

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) : base(db) => _db = db;

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => await _db.Users.AnyAsync(u => u.Email == email, ct);

    public async Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken ct = default)
        => await _db.Users.OrderBy(u => u.FullName).ToListAsync(ct);
}
