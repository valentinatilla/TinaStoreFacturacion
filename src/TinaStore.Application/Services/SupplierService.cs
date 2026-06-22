using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public sealed class SupplierService : ISupplierService
{
    private readonly IRepository<Supplier> _suppliers;

    public SupplierService(IRepository<Supplier> suppliers)
    {
        _suppliers = suppliers;
    }

    public async Task<IEnumerable<SupplierDto>> GetAllAsync(bool soloActivos = false)
    {
        var lista = await _suppliers.GetAllAsync();
        var resultado = soloActivos ? lista.Where(s => s.IsActive) : lista;
        return resultado.Select(ToDto);
    }

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        var entity = await _suppliers.GetByIdAsync(id);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto)
    {
        var entity = new Supplier
        {
            Name = dto.Name,
            TaxId = dto.TaxId,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            Notes = dto.Notes,
            IsActive = true
        };

        await _suppliers.AddAsync(entity);
        await _suppliers.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierDto dto)
    {
        var entity = await _suppliers.GetByIdAsync(id);
        if (entity is null) return null;

        entity.Name = dto.Name;
        entity.TaxId = dto.TaxId;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.Address = dto.Address;
        entity.Notes = dto.Notes;
        entity.IsActive = dto.IsActive;

        await _suppliers.UpdateAsync(entity);
        await _suppliers.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _suppliers.GetByIdAsync(id);
        if (entity is null) return false;

        await _suppliers.DeleteAsync(entity);
        await _suppliers.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<SupplierDto>> SearchAsync(string termino)
    {
        var coincidencias = await _suppliers.FindAsync(s =>
            s.Name.Contains(termino) ||
            (s.TaxId != null && s.TaxId.Contains(termino)));
        return coincidencias.Select(ToDto);
    }

    private static SupplierDto ToDto(Supplier s) => new(
        s.Id,
        s.Name,
        s.TaxId,
        s.Phone,
        s.Email,
        s.Address,
        s.Notes,
        s.IsActive,
        s.Products?.Count(p => !p.IsDeleted) ?? 0,
        s.CreatedAt
    );
}
