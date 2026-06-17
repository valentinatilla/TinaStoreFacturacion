using System.Net.Http.Json;

namespace TinaStore.Web.Services;

// ─── DTOs de respuesta de la API ──────────────────────────────────────────────

public record TokenResponseDto(string AccessToken, string TokenType, int ExpiresInMinutes, UserInfoDto User);
public record UserInfoDto(int Id, string FullName, string Email, string Role, bool IsActive);

public record DashboardDto(
    decimal VentasHoy, decimal VentasMes, int FacturasHoy,
    decimal TotalPorCobrar, int ClientesConDeuda,
    decimal GastosMes, int ProductosBajoStock,
    List<UltimaFacturaDto> UltimasFacturas,
    List<DeudorResumenDto> TopDeudores);

public record UltimaFacturaDto(int Id, string Numero, string ClienteNombre, decimal Total, string Estado, DateTime Fecha);
public record DeudorResumenDto(int ClienteId, string ClienteNombre, decimal SaldoPendiente);

public record ClienteDto(int Id, string FullName, string? DocumentNumber, string? Phone, string? Email, string? Address, decimal SaldoPendiente, bool IsActive);
public record CreateClienteDto(string FullName, string? DocumentNumber, string? Phone, string? Email, string? Address);
public record UpdateClienteDto(string FullName, string? DocumentNumber, string? Phone, string? Email, string? Address, bool IsActive);

public record CategoriaDto(int Id, string Name, string? Description, int ProductCount);
public record CreateCategoriaDto(string Name, string? Description);

public record ProveedorDto(int Id, string Name, string? ContactName, string? Phone, string? Email, string? Address, bool IsActive);
public record CreateProveedorDto(string Name, string? ContactName, string? Phone, string? Email, string? Address);

public record MetodoPagoDto(int Id, string Name, string? Description, bool IsActive);

public record ProductoDto(int Id, string Name, string? SKU, string? Description, decimal SalePrice, decimal PurchasePrice, int Stock, int MinStock, bool IsActive, string? CategoryName, string? SupplierName, int CategoryId, int? SupplierId);
public record CreateProductoDto(string Name, string? SKU, string? Description, decimal SalePrice, decimal PurchasePrice, int Stock, int MinStock, int CategoryId, int? SupplierId);
public record UpdateProductoDto(string Name, string? SKU, string? Description, decimal SalePrice, decimal PurchasePrice, int MinStock, bool IsActive, int CategoryId, int? SupplierId);

public record FacturaDto(int Id, string Numero, string ClienteNombre, decimal Subtotal, decimal Tax, decimal Total, decimal TotalPagado, string Estado, DateTime FechaEmision);
public record CreateFacturaDto(int ClienteId, int MetodoPagoId, List<CreateDetalleFacturaDto> Detalles, decimal? PagoInicial, string? Notas);
public record CreateDetalleFacturaDto(int ProductoId, int Cantidad, decimal PrecioUnitario);

public record EgresoDto(int Id, string Descripcion, decimal Monto, string Categoria, string Estado, DateTime Fecha);
public record CreateEgresoDto(int CategoriaGastoId, string Descripcion, decimal Monto, int MetodoPagoId, DateTime Fecha, string? Comprobante, string? Notas);

public record CategoriaGastoDto(int Id, string Name, string? Description, int ExpenseCount);

public record ConfiguracionTiendaDto(
    int Id, string StoreName, string? LogoPath, string? Address,
    string? Phone, string? Email, string? TaxId,
    string? InvoiceFooterMessage, string Currency,
    decimal TaxPercentage, int InvoiceConsecutive, bool AllowNegativeStock);

public record UpdateConfiguracionDto(
    string StoreName, string? Address, string? Phone, string? Email,
    string? TaxId, string? InvoiceFooterMessage, string Currency,
    decimal TaxPercentage, bool AllowNegativeStock);

public record ReporteVentasDto(DateTime Fecha, int CantidadFacturas, decimal TotalVentas, decimal TotalCobrado);
public record ReporteGastosDto(string Categoria, int CantidadGastos, decimal TotalGastos);
public record ReporteCuentasPorCobrarDto(string ClienteNombre, string? DocumentoCliente, decimal SaldoPendiente, int FacturasPendientes, DateTime? UltimaFacturaFecha);

/// <summary>
/// Wrapper centralizado de HttpClient para consumir la API de TinaStore.
/// Inyecta el token JWT en cada petición autenticada.
/// </summary>
public class TinaStoreApiClient
{
    private readonly HttpClient _http;
    private readonly SessionStateService _session;

    public TinaStoreApiClient(HttpClient http, SessionStateService session)
    {
        _http = http;
        _session = session;
    }

    private void SetAuthHeader()
    {
        if (!string.IsNullOrEmpty(_session.Token))
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token);
    }

    // Helper: GET que devuelve null en lugar de lanzar excepción ante 401/403/red caída
    private async Task<T?> GetSafeAsync<T>(string url) where T : class
    {
        SetAuthHeader();
        try
        {
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch { return null; }
    }

    // ── Auth ──────────────────────────────────────────────────────────────────
    public async Task<TokenResponseDto?> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/auth/login", new { email, password });
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<TokenResponseDto>();
        }
        catch { return null; }
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────
    public Task<DashboardDto?> GetDashboardAsync() =>
        GetSafeAsync<DashboardDto>("/api/dashboard");

    // ── Clientes ──────────────────────────────────────────────────────────────
    public Task<List<ClienteDto>?> GetClientesAsync() =>
        GetSafeAsync<List<ClienteDto>>("/api/customers");

    public Task<ClienteDto?> GetClienteAsync(int id) =>
        GetSafeAsync<ClienteDto>($"/api/customers/{id}");

    public async Task<bool> CreateClienteAsync(CreateClienteDto dto)
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync("/api/customers", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateClienteAsync(int id, UpdateClienteDto dto)
    {
        SetAuthHeader();
        var r = await _http.PutAsJsonAsync($"/api/customers/{id}", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteClienteAsync(int id)
    {
        SetAuthHeader();
        var r = await _http.DeleteAsync($"/api/customers/{id}");
        return r.IsSuccessStatusCode;
    }

    // ── Categorías ────────────────────────────────────────────────────────────
    public Task<List<CategoriaDto>?> GetCategoriasAsync() =>
        GetSafeAsync<List<CategoriaDto>>("/api/categories");

    public async Task<bool> CreateCategoriaAsync(CreateCategoriaDto dto)
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync("/api/categories", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCategoriaAsync(int id)
    {
        SetAuthHeader();
        var r = await _http.DeleteAsync($"/api/categories/{id}");
        return r.IsSuccessStatusCode;
    }

    // ── Proveedores ───────────────────────────────────────────────────────────
    public Task<List<ProveedorDto>?> GetProveedoresAsync() =>
        GetSafeAsync<List<ProveedorDto>>("/api/suppliers");

    public async Task<bool> CreateProveedorAsync(CreateProveedorDto dto)
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync("/api/suppliers", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProveedorAsync(int id)
    {
        SetAuthHeader();
        var r = await _http.DeleteAsync($"/api/suppliers/{id}");
        return r.IsSuccessStatusCode;
    }

    // ── Métodos de pago ───────────────────────────────────────────────────────
    public Task<List<MetodoPagoDto>?> GetMetodosPagoAsync() =>
        GetSafeAsync<List<MetodoPagoDto>>("/api/paymentmethods");

    // ── Productos ─────────────────────────────────────────────────────────────
    public Task<List<ProductoDto>?> GetProductosAsync() =>
        GetSafeAsync<List<ProductoDto>>("/api/products");

    public Task<ProductoDto?> GetProductoAsync(int id) =>
        GetSafeAsync<ProductoDto>($"/api/products/{id}");

    public async Task<bool> CreateProductoAsync(CreateProductoDto dto)
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync("/api/products", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProductoAsync(int id, UpdateProductoDto dto)
    {
        SetAuthHeader();
        var r = await _http.PutAsJsonAsync($"/api/products/{id}", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProductoAsync(int id)
    {
        SetAuthHeader();
        var r = await _http.DeleteAsync($"/api/products/{id}");
        return r.IsSuccessStatusCode;
    }

    // ── Facturas ──────────────────────────────────────────────────────────────
    public Task<List<FacturaDto>?> GetFacturasAsync() =>
        GetSafeAsync<List<FacturaDto>>("/api/invoices");

    public async Task<bool> CreateFacturaAsync(CreateFacturaDto dto)
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync("/api/invoices", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> AnularFacturaAsync(int id)
    {
        SetAuthHeader();
        var r = await _http.PostAsync($"/api/invoices/{id}/cancel", null);
        return r.IsSuccessStatusCode;
    }

    public async Task<byte[]?> DescargarPdfFacturaAsync(int id)
    {
        SetAuthHeader();
        var r = await _http.GetAsync($"/api/documents/invoice/{id}/pdf");
        return r.IsSuccessStatusCode ? await r.Content.ReadAsByteArrayAsync() : null;
    }

    // ── Egresos ───────────────────────────────────────────────────────────────
    public Task<List<EgresoDto>?> GetEgresosAsync() =>
        GetSafeAsync<List<EgresoDto>>("/api/expenses");

    public Task<List<CategoriaGastoDto>?> GetCategoriasGastoAsync() =>
        GetSafeAsync<List<CategoriaGastoDto>>("/api/expensecategories");

    public async Task<bool> CreateEgresoAsync(CreateEgresoDto dto)
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync("/api/expenses", dto);
        return r.IsSuccessStatusCode;
    }

    // ── Configuración de tienda ───────────────────────────────────────────────
    public Task<ConfiguracionTiendaDto?> GetConfiguracionAsync() =>
        GetSafeAsync<ConfiguracionTiendaDto>("/api/settings");

    public async Task<bool> UpdateConfiguracionAsync(UpdateConfiguracionDto dto)
    {
        SetAuthHeader();
        var r = await _http.PutAsJsonAsync("/api/settings", dto);
        return r.IsSuccessStatusCode;
    }

    // ── Reportes ──────────────────────────────────────────────────────────────
    public Task<List<ReporteVentasDto>?> GetReporteVentasAsync(DateTime desde, DateTime hasta) =>
        GetSafeAsync<List<ReporteVentasDto>>(
            $"/api/reports/sales?from={desde:yyyy-MM-dd}&to={hasta:yyyy-MM-dd}");

    public Task<List<ReporteGastosDto>?> GetReporteGastosAsync(DateTime desde, DateTime hasta) =>
        GetSafeAsync<List<ReporteGastosDto>>(
            $"/api/reports/expenses?from={desde:yyyy-MM-dd}&to={hasta:yyyy-MM-dd}");

    public Task<List<ReporteCuentasPorCobrarDto>?> GetReporteCuentasPorCobrarAsync() =>
        GetSafeAsync<List<ReporteCuentasPorCobrarDto>>("/api/reports/accounts-receivable");

    public async Task<byte[]?> ExportarProductosExcelAsync()
    {
        SetAuthHeader();
        var r = await _http.GetAsync("/api/documents/products/export");
        return r.IsSuccessStatusCode ? await r.Content.ReadAsByteArrayAsync() : null;
    }
}
