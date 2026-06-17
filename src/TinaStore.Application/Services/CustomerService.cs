using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
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
        var lista = await _customers.GetAllAsync();
        var resultado = soloActivos ? lista.Where(c => c.IsActive) : lista;
        return resultado.Select(ToDto);
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var entity = await _customers.GetByIdAsync(id);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
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
        var entity = await _customers.GetByIdAsync(id);
        if (entity is null) return null;

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
        c.CreatedAt
    );
}
