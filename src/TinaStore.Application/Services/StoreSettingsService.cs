using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public sealed class StoreSettingsService : IStoreSettingsService
{
    private readonly IRepository<StoreSettings> _repo;
    private readonly string _uploadsRoot;

    public StoreSettingsService(IRepository<StoreSettings> repo, string uploadsRoot)
    {
        _repo = repo;
        _uploadsRoot = uploadsRoot;
    }

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
        settings.ReminderMessage      = dto.ReminderMessage;
        settings.Currency             = dto.Currency;
        settings.TaxPercentage        = dto.TaxPercentage;
        settings.AllowNegativeStock   = dto.AllowNegativeStock;

        await _repo.UpdateAsync(settings);
        await _repo.SaveChangesAsync();
        return ToDto(settings);
    }

    public async Task<StoreSettingsDto> UploadLogoAsync(Stream fileStream, string fileName)
    {
        var settings = await _repo.GetByIdAsync(1)
            ?? throw new InvalidOperationException("No se encontró la configuración de la tienda.");

        var logosDir = Path.Combine(_uploadsRoot, "uploads", "logos");
        Directory.CreateDirectory(logosDir);

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var nuevoNombre = $"logo{ext}";
        var rutaFisica = Path.Combine(logosDir, nuevoNombre);

        await using var fs = new FileStream(rutaFisica, FileMode.Create, FileAccess.Write);
        await fileStream.CopyToAsync(fs);

        settings.LogoPath = $"/uploads/logos/{nuevoNombre}";
        await _repo.UpdateAsync(settings);
        await _repo.SaveChangesAsync();
        return ToDto(settings);
    }

    private static StoreSettingsDto ToDto(StoreSettings s) => new(
        s.Id, s.StoreName, s.LogoPath, s.Address, s.Phone, s.Email,
        s.TaxId, s.InvoiceFooterMessage, s.ReminderMessage, s.Currency,
        s.TaxPercentage, s.InvoiceConsecutive, s.AllowNegativeStock);
}
