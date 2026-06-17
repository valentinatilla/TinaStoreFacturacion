using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public sealed class PaymentMethodService : IPaymentMethodService
{
    private readonly IRepository<PaymentMethod> _paymentMethods;

    public PaymentMethodService(IRepository<PaymentMethod> paymentMethods)
    {
        _paymentMethods = paymentMethods;
    }

    public async Task<IEnumerable<PaymentMethodDto>> GetAllAsync(bool soloActivos = false)
    {
        var lista = await _paymentMethods.GetAllAsync();
        var resultado = soloActivos ? lista.Where(p => p.IsActive) : lista;
        return resultado.Select(ToDto);
    }

    public async Task<PaymentMethodDto?> GetByIdAsync(int id)
    {
        var entity = await _paymentMethods.GetByIdAsync(id);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<PaymentMethodDto> CreateAsync(CreatePaymentMethodDto dto)
    {
        var entity = new PaymentMethod
        {
            Name = dto.Name,
            Type = dto.Type,
            Description = dto.Description,
            IsActive = true
        };

        await _paymentMethods.AddAsync(entity);
        await _paymentMethods.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<PaymentMethodDto?> UpdateAsync(int id, UpdatePaymentMethodDto dto)
    {
        var entity = await _paymentMethods.GetByIdAsync(id);
        if (entity is null) return null;

        entity.Name = dto.Name;
        entity.Type = dto.Type;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;

        await _paymentMethods.UpdateAsync(entity);
        await _paymentMethods.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _paymentMethods.GetByIdAsync(id);
        if (entity is null) return false;

        await _paymentMethods.DeleteAsync(entity);
        await _paymentMethods.SaveChangesAsync();
        return true;
    }

    private static PaymentMethodDto ToDto(PaymentMethod p) => new(
        p.Id,
        p.Name,
        p.Type,
        p.Type.ToString(),
        p.Description,
        p.IsActive
    );
}
