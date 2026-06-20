using System.Net.Http.Json;

namespace TinaStore.Web.Services;

// ─── DTOs de respuesta de la API ──────────────────────────────────────────────

public record TokenResponseDto(string AccessToken, string TokenType, int ExpiresInMinutes, UserInfoDto User);
public record UserInfoDto(int Id, string FullName, string Email, string Role, bool IsActive);

public record DashboardDto(
    decimal VentasHoy,
    int FacturasHoy,
    decimal VentasSemana,
    decimal VentasMes,
    decimal TotalPorCobrar,
    int ClientesConDeuda,
    decimal GastosHoy,
    decimal GastosMes,
    int ProductosStockBajo,
    int TotalProductosActivos,
    List<UltimaFacturaDto> UltimasFacturas,
    List<DeudorResumenDto> TopDeudores);

public record UltimaFacturaDto(int Id, string InvoiceNumber, DateTime InvoiceDate, string CustomerName, decimal Total, decimal Balance, int Status, string StatusName);
public record DeudorResumenDto(int CustomerId, string CustomerName, string? Phone, decimal Saldo);

public record ClienteDto(int Id, string FullName, string? DocumentType, string? DocumentNumber, string? Phone, string? Email, string? Address, string? Notes, bool IsActive, decimal PendingBalance, DateTime CreatedAt, DateTime? LastPurchaseDate, string CommercialStatus);
public record CreateClienteDto(string FullName, string? DocumentType, string? DocumentNumber, string? Phone, string? Email, string? Address, string? Notes);
public record UpdateClienteDto(string FullName, string? DocumentType, string? DocumentNumber, string? Phone, string? Email, string? Address, string? Notes, bool IsActive);

public record CategoriaDto(int Id, string Name, string? Description, bool IsActive, int ProductCount);
public record CreateCategoriaDto(string Name, string? Description);

public record ProveedorDto(int Id, string Name, string? TaxId, string? ContactName, string? Phone, string? Email, string? Address, string? Notes, bool IsActive, int ProductCount, DateTime CreatedAt);
public record CreateProveedorDto(string Name, string? TaxId, string? ContactName, string? Phone, string? Email, string? Address, string? Notes);
public record UpdateProveedorDto(string Name, string? TaxId, string? ContactName, string? Phone, string? Email, string? Address, string? Notes, bool IsActive);

public record MetodoPagoDto(int Id, string Name, string? Description, bool IsActive);

public record ProductoDto(int Id, string? Sku, string Name, string? Description, string? Unit, decimal SalePrice, decimal PurchasePrice, int CurrentStock, int MinimumStock, bool IsActive, bool IsLowStock, decimal ProfitMargin, int CategoryId, string CategoryName, int? SupplierId, string? SupplierName, string? ImagePath);
public record CreateProductoDto(string? Sku, string Name, string? Description, string? Unit, decimal PurchasePrice, decimal SalePrice, int CurrentStock, int MinimumStock, int CategoryId, int? SupplierId);
public record UpdateProductoDto(string? Sku, string Name, string? Description, string? Unit, decimal PurchasePrice, decimal SalePrice, int MinimumStock, bool IsActive, int CategoryId, int? SupplierId, int StockEntrada = 0);
public record AjusteStockDto(int Cantidad, string? Notas = null);

public record FacturaDto(int Id, string InvoiceNumber, DateTime InvoiceDate, string CustomerName, decimal Subtotal, decimal DiscountAmount, decimal TaxAmount, decimal Total, decimal AmountPaid, decimal Balance, int Status, string StatusName, string? Notes);
public record CreateFacturaDto(int CustomerId, decimal DiscountAmount, decimal TaxAmount, string? Notes, List<CreateDetalleFacturaDto> Details, CreatePagoInicialDto? PagoInicial);
public record CreateDetalleFacturaDto(int ProductId, int Quantity, decimal UnitPrice, decimal DiscountAmount = 0);
public record CreatePagoInicialDto(int PaymentMethodId, decimal Amount, string? Reference, string? Notes);

public record EgresoDto(int Id, DateTime ExpenseDate, string Description, decimal Amount, string? Notes, int Status, string StatusName, int ExpenseCategoryId, string ExpenseCategoryName, int? SupplierId, string? SupplierName, int? PaymentMethodId, string? PaymentMethodName);
public record CreateEgresoDto(DateTime ExpenseDate, string Description, decimal Amount, string? Notes, int ExpenseCategoryId, int? SupplierId, int? PaymentMethodId);

public record CategoriaGastoDto(int Id, string Name, string? Description, bool IsActive, int ExpenseCount);

public record UpdateEgresoDto(DateTime ExpenseDate, string Description, decimal Amount, string? Notes, int ExpenseCategoryId, int? SupplierId, int? PaymentMethodId);

public record RegisterPagoDto(int PaymentMethodId, decimal Amount, string? Reference, string? Notes);

public record ConfiguracionTiendaDto(
    int Id, string StoreName, string? LogoPath, string? Address,
    string? Phone, string? Email, string? TaxId,
    string? InvoiceFooterMessage, string? ReminderMessage, string Currency,
    decimal TaxPercentage, int InvoiceConsecutive, bool AllowNegativeStock);

public record UpdateConfiguracionDto(
    string StoreName, string? Address, string? Phone, string? Email,
    string? TaxId, string? InvoiceFooterMessage, string? ReminderMessage,
    string Currency, decimal TaxPercentage, bool AllowNegativeStock);

public record UsuarioDto(int Id, string FullName, string Email, string Role, bool IsActive, DateTime? LastLoginAt);
public record CreateUsuarioDto(string FullName, string Email, string Password, string Role);
public record UpdateUsuarioDto(string FullName, string Email, string Role, bool IsActive);
public record ResetPasswordDto(string NewPassword, string ConfirmNewPassword);

public record VentasPorPeriodoDto(DateTime Fecha, int CantidadFacturas, decimal TotalVentas, decimal TotalCobrado, decimal TotalPendiente);
public record TopProductoDto(int ProductId, string ProductName, string? Sku, int TotalVendido, decimal TotalIngresos);
public record ReporteVentasDto(DateTime Desde, DateTime Hasta, decimal TotalVentas, decimal TotalCobrado, decimal TotalPendiente, int TotalFacturas, List<VentasPorPeriodoDto> VentasPorDia, List<TopProductoDto> TopProductos);

public record ResumenGastosPorCategoriaDto(int CategoryId, string CategoryName, int TotalEgresos, decimal TotalMonto);
public record ReporteGastosDto(DateTime Desde, DateTime Hasta, decimal TotalGastos, int TotalEgresos, List<ResumenGastosPorCategoriaDto> PorCategoria);

public record RegistrarRecordatorioWhatsAppDto(int CustomerId, string Message);
public record ReminderHistoryDto(int Id, int CustomerId, string CustomerName, string Channel, string Status, DateTime SentAt, string Message);

public record ImportPreviewRowDto(
    int Fila,
    string? Nombre,
    string? Sku,
    string? Descripcion,
    decimal PrecioCosto,
    decimal PrecioVenta,
    int StockInicial,
    int StockMinimo,
    int CategoriaId,
    string? CategoriaNombre,
    int? ProveedorId,
    string? ProveedorNombre,
    bool Valido,
    string? MensajeError);

public record ExcelImportResultDto(int TotalFilas, int Importados, int Errores, List<string> MensajesError);

public record DeudorCXCDto(int CustomerId, string CustomerName, string? DocumentNumber, string? Phone, decimal SaldoPendiente, int FacturasPendientes, DateTime? UltimaFacturaFecha);
public record ReporteCuentasPorCobrarDto(decimal TotalPorCobrar, int TotalClientes, List<DeudorCXCDto> Deudores);

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

    // Helper: GET que devuelve (resultado, mensaje de error) para diagnóstico
    private async Task<(T? Data, string? Error)> GetWithErrorAsync<T>(string url) where T : class
    {
        SetAuthHeader();
        try
        {
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return (null, $"La API respondió con error {(int)response.StatusCode}.");
            var data = await response.Content.ReadFromJsonAsync<T>();
            return (data, null);
        }
        catch (HttpRequestException ex)
        {
            return (null, $"No se pudo conectar con la API ({ex.Message}).");
        }
        catch (Exception ex)
        {
            return (null, $"Error inesperado: {ex.Message}");
        }
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

    public async Task<(TokenResponseDto? Token, string? Error)> LoginConGoogleAsync(string idToken)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/auth/google", new { idToken });
            if (response.IsSuccessStatusCode)
                return (await response.Content.ReadFromJsonAsync<TokenResponseDto>(), null);

            // Intentar leer el mensaje de error de la API
            try
            {
                var err = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                var msg = err.TryGetProperty("message", out var m) ? m.GetString() : null;
                return (null, msg ?? $"Error {(int)response.StatusCode}");
            }
            catch { return (null, $"Error {(int)response.StatusCode}"); }
        }
        catch (Exception ex) { return (null, ex.Message); }
    }

    /// <summary>
    /// Obtiene el perfil del usuario usando un token explícito.
    /// Usado para restaurar la sesión desde la cookie al arrancar el circuito Blazor.
    /// </summary>
    public async Task<UserInfoDto?> GetPerfilAsync(string token)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/profile");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserInfoDto>();
        }
        catch { return null; }
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────
    public Task<DashboardDto?> GetDashboardAsync() =>
        GetSafeAsync<DashboardDto>("/api/dashboard");

    public Task<(DashboardDto? Data, string? Error)> GetDashboardConDiagnosticoAsync() =>
        GetWithErrorAsync<DashboardDto>("/api/dashboard");

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

    public async Task<bool> UpdateProveedorAsync(int id, UpdateProveedorDto dto)
    {
        SetAuthHeader();
        var r = await _http.PutAsJsonAsync($"/api/suppliers/{id}", dto);
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

    public async Task<ProductoDto?> CreateProductoAsync(CreateProductoDto dto)
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync("/api/products", dto);
        if (!r.IsSuccessStatusCode) return null;
        return await r.Content.ReadFromJsonAsync<ProductoDto>();
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

    public async Task<ProductoDto?> SubirImagenProductoAsync(int id, Stream contenido, string nombreArchivo)
    {
        SetAuthHeader();
        using var form = new MultipartFormDataContent();
        using var sc = new StreamContent(contenido);
        sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        form.Add(sc, "archivo", nombreArchivo);
        var r = await _http.PostAsync($"/api/products/{id}/imagen", form);
        if (!r.IsSuccessStatusCode) return null;
        return await r.Content.ReadFromJsonAsync<ProductoDto>();
    }

    public async Task<bool> EliminarImagenProductoAsync(int id)
    {
        SetAuthHeader();
        var r = await _http.DeleteAsync($"/api/products/{id}/imagen");
        return r.IsSuccessStatusCode;
    }

    public async Task<ProductoDto?> AjustarStockAsync(int id, AjusteStockDto dto)
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync($"/api/products/{id}/ajuste-stock", dto);
        if (!r.IsSuccessStatusCode) return null;
        return await r.Content.ReadFromJsonAsync<ProductoDto>();
    }

    // ── Facturas
    public Task<List<FacturaDto>?> GetFacturasAsync() =>
        GetSafeAsync<List<FacturaDto>>("/api/invoices");

    public async Task<bool> CreateFacturaAsync(CreateFacturaDto dto)
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync("/api/invoices", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> RegisterPagoAsync(int facturaId, RegisterPagoDto dto)
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync($"/api/invoices/{facturaId}/pagos", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> AnularFacturaAsync(int id, string razon = "Anulada por el usuario")
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync($"/api/invoices/{id}/anular", new { Reason = razon });
        return r.IsSuccessStatusCode;
    }

    public async Task<byte[]?> DescargarPdfFacturaAsync(int id)
    {
        SetAuthHeader();
        var r = await _http.GetAsync($"/api/documents/facturas/{id}/pdf");
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

    public async Task<bool> UpdateEgresoAsync(int id, UpdateEgresoDto dto)
    {
        SetAuthHeader();
        var r = await _http.PutAsJsonAsync($"/api/expenses/{id}", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> AnularEgresoAsync(int id)
    {
        SetAuthHeader();
        var r = await _http.PostAsync($"/api/expenses/{id}/anular", null);
        return r.IsSuccessStatusCode;
    }

    // ── Configuración de tienda ───────────────────────────────────────────────
    public string BaseUrl => _http.BaseAddress?.ToString() ?? string.Empty;

    public Task<ConfiguracionTiendaDto?> GetConfiguracionAsync() =>
        GetSafeAsync<ConfiguracionTiendaDto>("/api/settings");

    public async Task<bool> UpdateConfiguracionAsync(UpdateConfiguracionDto dto)
    {
        SetAuthHeader();
        var r = await _http.PutAsJsonAsync("/api/settings", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<ConfiguracionTiendaDto?> UploadLogoAsync(Stream fileStream, string fileName)
    {
        SetAuthHeader();
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);
        var r = await _http.PostAsync("/api/settings/logo", content);
        return r.IsSuccessStatusCode
            ? await r.Content.ReadFromJsonAsync<ConfiguracionTiendaDto>()
            : null;
    }

    // ── Recordatorios ─────────────────────────────────────────────────────────
    public async Task<ReminderHistoryDto?> RegistrarRecordatorioWhatsAppAsync(int customerId, string message)
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync("/api/reminders/whatsapp",
            new RegistrarRecordatorioWhatsAppDto(customerId, message));
        return r.IsSuccessStatusCode
            ? await r.Content.ReadFromJsonAsync<ReminderHistoryDto>()
            : null;
    }

    public Task<List<ReminderHistoryDto>?> GetHistorialRecordatoriosAsync(int customerId) =>
        GetSafeAsync<List<ReminderHistoryDto>>($"/api/reminders/historial/{customerId}");

    // ── Reportes ──────────────────────────────────────────────────────────────
    public Task<ReporteVentasDto?> GetReporteVentasAsync(DateTime desde, DateTime hasta) =>
        GetSafeAsync<ReporteVentasDto>(
            $"/api/reports/ventas?from={desde:yyyy-MM-dd}&to={hasta:yyyy-MM-dd}");

    public Task<ReporteGastosDto?> GetReporteGastosAsync(DateTime desde, DateTime hasta) =>
        GetSafeAsync<ReporteGastosDto>(
            $"/api/reports/gastos?from={desde:yyyy-MM-dd}&to={hasta:yyyy-MM-dd}");

    public Task<ReporteCuentasPorCobrarDto?> GetReporteCuentasPorCobrarAsync() =>
        GetSafeAsync<ReporteCuentasPorCobrarDto>("/api/reports/cuentas-por-cobrar");

    public async Task<byte[]?> ExportarProductosExcelAsync()
    {
        SetAuthHeader();
        var r = await _http.GetAsync("/api/documents/productos/excel");
        return r.IsSuccessStatusCode ? await r.Content.ReadAsByteArrayAsync() : null;
    }

    public async Task<byte[]?> DescargarPlantillaExcelAsync()
    {
        SetAuthHeader();
        var r = await _http.GetAsync("/api/documents/productos/plantilla");
        return r.IsSuccessStatusCode ? await r.Content.ReadAsByteArrayAsync() : null;
    }

    public async Task<List<ImportPreviewRowDto>?> PrevisualizarImportacionAsync(Stream contenido, string nombreArchivo)
    {
        SetAuthHeader();
        using var form = new MultipartFormDataContent();
        using var sc   = new StreamContent(contenido);
        sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        form.Add(sc, "file", nombreArchivo);
        var r = await _http.PostAsync("/api/documents/productos/previsualizar", form);
        return r.IsSuccessStatusCode
            ? await r.Content.ReadFromJsonAsync<List<ImportPreviewRowDto>>()
            : null;
    }

    public async Task<ExcelImportResultDto?> ImportarProductosAsync(List<ImportPreviewRowDto> filas)
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync("/api/documents/productos/importar-confirmado", filas);
        return r.IsSuccessStatusCode
            ? await r.Content.ReadFromJsonAsync<ExcelImportResultDto>()
            : null;
    }

    // ── Usuarios ─────────────────────────────────────────────────────────────
    public Task<List<UsuarioDto>?> GetUsuariosAsync() =>
        GetSafeAsync<List<UsuarioDto>>("/api/users");

    public async Task<bool> CreateUsuarioAsync(CreateUsuarioDto dto)
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync("/api/users", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateUsuarioAsync(int id, UpdateUsuarioDto dto)
    {
        SetAuthHeader();
        var r = await _http.PutAsJsonAsync($"/api/users/{id}", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteUsuarioAsync(int id)
    {
        SetAuthHeader();
        var r = await _http.DeleteAsync($"/api/users/{id}");
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> ResetPasswordUsuarioAsync(int id, ResetPasswordDto dto)
    {
        SetAuthHeader();
        var r = await _http.PostAsJsonAsync($"/api/users/{id}/reset-password", dto);
        return r.IsSuccessStatusCode;
    }
}
