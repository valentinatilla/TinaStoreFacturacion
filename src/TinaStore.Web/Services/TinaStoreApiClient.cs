using System.Net.Http.Json;

namespace TinaStore.Web.Services;

// ─── DTOs de respuesta de la API ──────────────────────────────────────────────

public record TokenResponseDto(string Token, string UserName, string Email, string Role, int ExpiresInMinutes);

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

    // ── Auth ──────────────────────────────────────────────────────────────────
    public Task<TokenResponseDto?> LoginAsync(string email, string password) =>
        _http.PostAsJsonAsync("/api/auth/login", new { email, password })
             .ContinueWith(t => t.Result.IsSuccessStatusCode
                 ? t.Result.Content.ReadFromJsonAsync<TokenResponseDto>().Result
                 : null);

    // ── Dashboard ─────────────────────────────────────────────────────────────
    public async Task<DashboardDto?> GetDashboardAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<DashboardDto>("/api/dashboard");
    }

    // ── Clientes ──────────────────────────────────────────────────────────────
    public async Task<List<ClienteDto>?> GetClientesAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<ClienteDto>>("/api/customers");
    }

    public async Task<ClienteDto?> GetClienteAsync(int id)
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<ClienteDto>($"/api/customers/{id}");
    }

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
    public async Task<List<CategoriaDto>?> GetCategoriasAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<CategoriaDto>>("/api/categories");
    }

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
    public async Task<List<ProveedorDto>?> GetProveedoresAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<ProveedorDto>>("/api/suppliers");
    }

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
    public async Task<List<MetodoPagoDto>?> GetMetodosPagoAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<MetodoPagoDto>>("/api/paymentmethods");
    }

    // ── Productos ─────────────────────────────────────────────────────────────
    public async Task<List<ProductoDto>?> GetProductosAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<ProductoDto>>("/api/products");
    }

    public async Task<ProductoDto?> GetProductoAsync(int id)
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<ProductoDto>($"/api/products/{id}");
    }

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
    public async Task<List<FacturaDto>?> GetFacturasAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<FacturaDto>>("/api/invoices");
    }

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
    public async Task<List<EgresoDto>?> GetEgresosAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<EgresoDto>>("/api/expenses");
    }

    public async Task<List<CategoriaGastoDto>?> GetCategoriasGastoAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<CategoriaGastoDto>>("/api/expensecategories");
    }

    public async Task<bool> CreateEgresoAsync(CreateEgresoDto dto)
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync("/api/expenses", dto);
        return r.IsSuccessStatusCode;
    }

    // ── Configuración de tienda ───────────────────────────────────────────────
    public async Task<ConfiguracionTiendaDto?> GetConfiguracionAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<ConfiguracionTiendaDto>("/api/settings");
    }

    public async Task<bool> UpdateConfiguracionAsync(UpdateConfiguracionDto dto)
    {
        SetAuthHeader();
        var r = await _http.PutAsJsonAsync("/api/settings", dto);
        return r.IsSuccessStatusCode;
    }

    // ── Reportes ──────────────────────────────────────────────────────────────
    public async Task<List<ReporteVentasDto>?> GetReporteVentasAsync(DateTime desde, DateTime hasta)
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<ReporteVentasDto>>(
            $"/api/reports/sales?from={desde:yyyy-MM-dd}&to={hasta:yyyy-MM-dd}");
    }

    public async Task<List<ReporteGastosDto>?> GetReporteGastosAsync(DateTime desde, DateTime hasta)
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<ReporteGastosDto>>(
            $"/api/reports/expenses?from={desde:yyyy-MM-dd}&to={hasta:yyyy-MM-dd}");
    }

    public async Task<List<ReporteCuentasPorCobrarDto>?> GetReporteCuentasPorCobrarAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<ReporteCuentasPorCobrarDto>>("/api/reports/accounts-receivable");
    }

    public async Task<byte[]?> ExportarProductosExcelAsync()
    {
        SetAuthHeader();
        var r = await _http.GetAsync("/api/documents/products/export");
        return r.IsSuccessStatusCode ? await r.Content.ReadAsByteArrayAsync() : null;
    }
}
