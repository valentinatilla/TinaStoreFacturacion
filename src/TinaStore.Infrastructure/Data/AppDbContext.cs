using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;

namespace TinaStore.Infrastructure.Data;

/// <summary>
/// Contexto principal de Entity Framework Core.
/// Contiene todas las tablas de la aplicación y sus configuraciones.
/// </summary>
public class AppDbContext : DbContext
{
    private readonly IAppClock _clock;

    public AppDbContext(DbContextOptions<AppDbContext> options, IAppClock clock) : base(options)
    {
        _clock = clock;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<StoreSettings> StoreSettings => Set<StoreSettings>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceDetail> InvoiceDetails => Set<InvoiceDetail>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<AccountReceivable> AccountsReceivable => Set<AccountReceivable>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<ReminderHistory> ReminderHistories => Set<ReminderHistory>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportBatchDetail> ImportBatchDetails => Set<ImportBatchDetail>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar automáticamente todas las configuraciones del ensamblado
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Filtro global de borrado lógico: entidades marcadas como eliminadas no aparecen
        modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Supplier>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Expense>().HasQueryFilter(e => !e.IsDeleted);

        // Índices únicos parciales en Products (solo filas activas, no borradas)
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Name)
            .IsUnique()
            .HasDatabaseName("IX_Products_Name_Unique")
            .HasFilter("\"IsDeleted\" = 0");

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Sku)
            .IsUnique()
            .HasDatabaseName("IX_Products_Sku_Unique")
            .HasFilter("\"IsDeleted\" = 0 AND \"Sku\" IS NOT NULL AND \"Sku\" != ''");

        // Precisión decimal para columnas de dinero
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }

        // Datos iniciales: configuración de la tienda
        modelBuilder.Entity<StoreSettings>().HasData(new StoreSettings
        {
            Id = 1,
            StoreName = "Tina Store",
            Currency = "COP",
            TaxPercentage = 0,
            InvoiceConsecutive = 1,
            AllowNegativeStock = false,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Datos iniciales: categorías por defecto
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "General", Description = "Categoría general", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Datos iniciales: métodos de pago por defecto
        modelBuilder.Entity<PaymentMethod>().HasData(
            new PaymentMethod { Id = 1, Name = "Efectivo", Type = Domain.Enums.PaymentMethodType.Cash, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PaymentMethod { Id = 2, Name = "Nequi", Type = Domain.Enums.PaymentMethodType.Nequi, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PaymentMethod { Id = 3, Name = "Daviplata", Type = Domain.Enums.PaymentMethodType.Daviplata, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PaymentMethod { Id = 4, Name = "Transferencia", Type = Domain.Enums.PaymentMethodType.BankTransfer, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PaymentMethod { Id = 5, Name = "Tarjeta", Type = Domain.Enums.PaymentMethodType.Card, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PaymentMethod { Id = 6, Name = "Fiado / Crédito", Type = Domain.Enums.PaymentMethodType.Credit, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Datos iniciales: categorías de egreso
        modelBuilder.Entity<ExpenseCategory>().HasData(
            new ExpenseCategory { Id = 1, Name = "Arriendo", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ExpenseCategory { Id = 2, Name = "Servicios", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ExpenseCategory { Id = 3, Name = "Compras a proveedor", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ExpenseCategory { Id = 4, Name = "Transporte", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ExpenseCategory { Id = 5, Name = "Nómina", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ExpenseCategory { Id = 6, Name = "Otros", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }

    public override int SaveChanges()
    {
        SetAuditDates();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditDates();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditDates()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Entities.BaseEntity &&
                        e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            var entity = (Domain.Entities.BaseEntity)entry.Entity;
            if (entry.State == EntityState.Added)
                entity.CreatedAt = _clock.Now;
            else
                entity.UpdatedAt = _clock.Now;
        }
    }
}
