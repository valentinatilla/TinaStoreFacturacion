using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public sealed class StoreSettingsService : IStoreSettingsService
{
    private readonly IRepository<StoreSettings> _repo;

    public StoreSettingsService(IRepository<StoreSettings> repo) => _repo = repo;

    public async Task<StoreSettingsDto> GetAsync()
    {
        var settings = await _repo.GetByIdAsync(1)
            ?? throw new InvalidOperationException("No se encontró la configuración de la tienda.");
        return ToDto(settings);
    }

    public async Task<StoreSettingsDto> UpdateAsync(UpdateStoreSettingsDto dto)
    {
        var settings = await _repo.GetByIdAsync(1)
            ?? throw new InvalidOperationException("No se encontró la configuración de la tienda.");

        settings.StoreName            = dto.StoreName;
        settings.Address              = dto.Address;
        settings.Phone                = dto.Phone;
        settings.Email                = dto.Email;
        settings.TaxId                = dto.TaxId;
        settings.InvoiceFooterMessage = dto.InvoiceFooterMessage;
        settings.Currency             = dto.Currency;
        settings.TaxPercentage        = dto.TaxPercentage;
        settings.AllowNegativeStock   = dto.AllowNegativeStock;

        await _repo.UpdateAsync(settings);
        await _repo.SaveChangesAsync();
        return ToDto(settings);
    }

    private static StoreSettingsDto ToDto(StoreSettings s) => new(
        s.Id, s.StoreName, s.LogoPath, s.Address, s.Phone, s.Email,
        s.TaxId, s.InvoiceFooterMessage, s.Currency,
        s.TaxPercentage, s.InvoiceConsecutive, s.AllowNegativeStock);
}
