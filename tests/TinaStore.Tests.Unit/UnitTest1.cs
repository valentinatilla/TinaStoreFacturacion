using TinaStore.Application.Helpers;
using TinaStore.Domain.Enums;

namespace TinaStore.Tests.Unit;

/// <summary>
/// Tests de humo básicos para verificar que el ensamblado de pruebas carga correctamente
/// y que helpers de Application funcionan sin infraestructura.
/// </summary>
public class SmokeTests
{
    [Theory]
    [InlineData(InvoiceStatus.Paid,      "Pagada")]
    [InlineData(InvoiceStatus.Partial,   "Parcial")]
    [InlineData(InvoiceStatus.Cancelled, "Anulada")]
    [InlineData(InvoiceStatus.Pending,   "Pendiente")]
    public void InvoiceStatusHelper_TraduceCorrectamenteAlEspanol(InvoiceStatus status, string esperado)
    {
        var resultado = InvoiceStatusHelper.EnEspanol(status);
        Assert.Equal(esperado, resultado);
    }
}
