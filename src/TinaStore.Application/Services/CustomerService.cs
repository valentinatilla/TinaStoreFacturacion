using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Enums;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customers;

    public CustomerService(ICustomerRepository customers)
    {
        _customers = customers;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync(bool soloActivos = false)
    {
        var lista = await _customers.GetAllWithInvoicesAsync();
        var resultado = soloActivos ? lista.Where(c => c.IsActive) : lista;
        return resultado.Select(ToDto);
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var entity = await _customers.GetWithInvoicesAsync(id);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.DocumentNumber))
        {
            var existing = await _customers.GetByDocumentAsync(dto.DocumentNumber.Trim());
            if (existing is not null)
                throw new InvalidOperationException($"Ya existe un cliente con el documento '{dto.DocumentNumber}'.");
        }

        var entity = new Customer
        {
            FullName = dto.FullName,
            DocumentType = dto.DocumentType,
            DocumentNumber = dto.DocumentNumber,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            Notes = dto.Notes,
            IsActive = true
        };

        await _customers.AddAsync(entity);
        await _customers.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto)
    {
        var entity = await _customers.GetWithInvoicesAsync(id);
        if (entity is null) return null;

        if (!string.IsNullOrWhiteSpace(dto.DocumentNumber))
        {
            var existing = await _customers.GetByDocumentAsync(dto.DocumentNumber.Trim());
            if (existing is not null && existing.Id != id)
                throw new InvalidOperationException($"Ya existe otro cliente con el documento '{dto.DocumentNumber}'.");
        }

        entity.FullName = dto.FullName;
        entity.DocumentType = dto.DocumentType;
        entity.DocumentNumber = dto.DocumentNumber;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.Address = dto.Address;
        entity.Notes = dto.Notes;
        entity.IsActive = dto.IsActive;

        await _customers.UpdateAsync(entity);
        await _customers.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _customers.GetByIdAsync(id);
        if (entity is null) return false;

        await _customers.DeleteAsync(entity);
        await _customers.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CustomerDto>> SearchAsync(string termino)
    {
        var lista = await _customers.SearchAsync(termino);
        return lista.Select(ToDto);
    }

    /// <summary>
    /// Calcula la fecha de última compra (última factura no anulada).
    /// </summary>
    private static DateTime? GetLastPurchaseDate(Customer c)
    {
        if (c.Invoices is null || !c.Invoices.Any()) return null;
        var ultima = c.Invoices
            .Where(i => i.Status != InvoiceStatus.Cancelled && !i.IsDeleted)
            .OrderByDescending(i => i.InvoiceDate)
            .FirstOrDefault();
        return ultima?.InvoiceDate;
    }

    /// <summary>
    /// Estado comercial automático:
    /// "Activo" si compró en los últimos 6 meses.
    /// "Inactivo" si no ha comprado o la última compra fue hace más de 6 meses.
    /// </summary>
    private static string GetCommercialStatus(Customer c)
    {
        var ultimaCompra = GetLastPurchaseDate(c);
        if (ultimaCompra is null) return "Sin compras";
        return ultimaCompra.Value >= DateTime.UtcNow.AddMonths(-6)
            ? "Activo"
            : "Inactivo";
    }

    private static CustomerDto ToDto(Customer c) => new(
        c.Id,
        c.FullName,
        c.DocumentType,
        c.DocumentNumber,
        c.Phone,
        c.Email,
        c.Address,
        c.Notes,
        c.IsActive,
        c.AccountReceivable?.Balance ?? 0,
        c.CreatedAt,
        GetLastPurchaseDate(c),
        GetCommercialStatus(c)
    );
}
