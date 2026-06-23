using FluentAssertions;
using Moq;
using TinaStore.Application.DTOs;
using TinaStore.Application.Services;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Tests.Unit;

public class StoreSettingsServiceTests
{
    private readonly Mock<IRepository<StoreSettings>> _repoMock;
    private readonly StoreSettingsService _sut;

    private static StoreSettings ConfiguracionBase() => new()
    {
        Id = 1,
        StoreName = "Tina Store",
        Currency = "COP",
        TaxPercentage = 0,
        InvoiceConsecutive = 1,
        AllowNegativeStock = false
    };

    public StoreSettingsServiceTests()
    {
        _repoMock = new Mock<IRepository<StoreSettings>>();

        // El constructor requiere un uploadsRoot; se usa un path temporal ficticio
        _sut = new StoreSettingsService(_repoMock.Object, Path.GetTempPath());
    }

    // ── GetAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_ConfigExiste_RetornaDto()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(ConfiguracionBase());

        var resultado = await _sut.GetAsync();

        resultado.Should().NotBeNull();
        resultado.StoreName.Should().Be("Tina Store");
        resultado.Currency.Should().Be("COP");
    }

    [Fact]
    public async Task GetAsync_ConfigNoExiste_LanzaExcepcion()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync((StoreSettings?)null);

        var act = async () => await _sut.GetAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*configuración de la tienda*");
    }

    // ── UpdateAsync — nombre siempre es "Tina Store" ──────────────────────────

    [Fact]
    public async Task UpdateAsync_SiempreGuardaNombreFixo_TinaStore()
    {
        var config = ConfiguracionBase();
        _repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(config);
        _repoMock.Setup(r => r.UpdateAsync(config, default)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        // El usuario intenta cambiar el nombre a algo diferente
        var dto = new UpdateStoreSettingsDto(
            StoreName: "Otro nombre cualquiera",
            Address: "Calle 10 #20-30",
            Phone: "3001234567",
            Email: "tinastore@mail.com",
            TaxId: null,
            InvoiceFooterMessage: null,
            ReminderMessage: null,
            Currency: "COP",
            TaxPercentage: 0,
            AllowNegativeStock: false);

        var resultado = await _sut.UpdateAsync(dto);

        resultado.StoreName.Should().Be("Tina Store",
            "el nombre de la tienda es fijo y no debe cambiar aunque el DTO envíe otro valor");
    }

    [Fact]
    public async Task UpdateAsync_ActualizaOtrosCampos_CorrectamenteSinTocarNombre()
    {
        var config = ConfiguracionBase();
        _repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(config);
        _repoMock.Setup(r => r.UpdateAsync(config, default)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var dto = new UpdateStoreSettingsDto(
            StoreName: "No importa",
            Address: "Nueva dirección 123",
            Phone: "3159876543",
            Email: "nuevo@correo.com",
            TaxId: "900123456",
            InvoiceFooterMessage: "Gracias por su compra",
            ReminderMessage: null,
            Currency: "USD",
            TaxPercentage: 19m,
            AllowNegativeStock: true);

        var resultado = await _sut.UpdateAsync(dto);

        resultado.StoreName.Should().Be("Tina Store");
        resultado.Address.Should().Be("Nueva dirección 123");
        resultado.Phone.Should().Be("3159876543");
        resultado.Email.Should().Be("nuevo@correo.com");
        resultado.TaxPercentage.Should().Be(19m);
        resultado.AllowNegativeStock.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ConfigNoExiste_LanzaExcepcion()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync((StoreSettings?)null);

        var dto = new UpdateStoreSettingsDto("X", null, null, null, null, null, null, "COP", 0, false);

        var act = async () => await _sut.UpdateAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*configuración de la tienda*");
    }
}
