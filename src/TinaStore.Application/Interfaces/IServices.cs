using TinaStore.Application.DTOs;

namespace TinaStore.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllAsync(bool soloActivos = false);
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
    Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CustomerDto>> SearchAsync(string termino);
}

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync(bool soloActivas = false);
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface IProductService
{
    Task<IEnumerable<ProductSummaryDto>> GetAllAsync(bool soloActivos = false);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProductSummaryDto>> GetLowStockAsync();
    Task<IEnumerable<ProductSummaryDto>> SearchAsync(string termino);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteAsync(int id);
    Task<ProductDto?> UpdateImagePathAsync(int id, string? imagePath);
    Task<ProductDto?> AjustarStockAsync(int id, AjusteStockDto dto);
}

public interface ISupplierService
{
    Task<IEnumerable<SupplierDto>> GetAllAsync(bool soloActivos = false);
    Task<SupplierDto?> GetByIdAsync(int id);
    Task<SupplierDto> CreateAsync(CreateSupplierDto dto);
    Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<SupplierDto>> SearchAsync(string termino);
}

public interface IPaymentMethodService
{
    Task<IEnumerable<PaymentMethodDto>> GetAllAsync(bool soloActivos = false);
    Task<PaymentMethodDto?> GetByIdAsync(int id);
    Task<PaymentMethodDto> CreateAsync(CreatePaymentMethodDto dto);
    Task<PaymentMethodDto?> UpdateAsync(int id, UpdatePaymentMethodDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceSummaryDto>> GetAllAsync();
    Task<IEnumerable<InvoiceSummaryDto>> GetByCustomerAsync(int customerId);
    Task<IEnumerable<InvoiceSummaryDto>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<InvoiceDto?> GetByIdAsync(int id);
    Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto);
    Task<InvoiceDto?> RegisterPaymentAsync(int invoiceId, RegisterPaymentDto dto);
    Task<InvoiceDto?> CancelAsync(int invoiceId, CancelInvoiceDto dto);
}

public interface IExpenseCategoryService
{
    Task<IEnumerable<ExpenseCategoryDto>> GetAllAsync(bool soloActivas = false);
    Task<ExpenseCategoryDto?> GetByIdAsync(int id);
    Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryDto dto);
    Task<ExpenseCategoryDto?> UpdateAsync(int id, UpdateExpenseCategoryDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface IExpenseService
{
    Task<IEnumerable<ExpenseDto>> GetAllAsync();
    Task<IEnumerable<ExpenseDto>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<ExpenseDto>> GetByCategoryAsync(int categoryId);
    Task<ExpenseDto?> GetByIdAsync(int id);
    Task<ExpenseDto> CreateAsync(CreateExpenseDto dto);
    Task<ExpenseDto?> UpdateAsync(int id, UpdateExpenseDto dto);
    Task<bool> CancelAsync(int id);
}

public interface IDashboardService
{
    Task<DashboardDto> GetSummaryAsync();
}

public interface IReportService
{
    Task<ReporteVentasDto> GetSalesReportAsync(DateTime from, DateTime to);
    Task<ReporteGastosDto> GetExpensesReportAsync(DateTime from, DateTime to);
    Task<ReporteCuentasPorCobrarDto> GetReceivablesReportAsync();
    Task<IEnumerable<TopProductoDto>> GetTopProductsAsync(DateTime from, DateTime to, int top = 10);
}

public interface IPdfService
{
    Task<byte[]> GenerateInvoicePdfAsync(int invoiceId);
}

public interface IExcelService
{
    Task<byte[]> ExportProductsAsync();
    Task<ExcelImportResultDto> ImportProductsAsync(Stream excelStream);
    Task<byte[]> GetProductTemplateAsync();
    Task<List<ImportPreviewRowDto>> PreviewImportAsync(Stream excelStream);
    Task<ExcelImportResultDto> ImportFromPreviewAsync(List<ImportPreviewRowDto> filas);
}

public interface IAuthService
{
    Task<TokenResponseDto?> LoginAsync(LoginDto dto);
    /// <summary>
    /// Busca o crea el usuario a partir de datos ya validados de Google y emite un JWT de TinaStore.
    /// La validación del id_token ocurre en la capa API antes de llamar a este método.
    /// </summary>
    Task<TokenResponseDto?> LoginWithGoogleAsync(GoogleUserInfoDto googleUser);
    Task<UserInfoDto?> GetProfileAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
}

public interface ITokenService
{
    string GenerateToken(int userId, string email, string fullName, string role);
    int ExpiresInMinutes { get; }
}

public interface IAppPasswordHasher
{
    string Hash(string password);
    bool Verify(string hashedPassword, string providedPassword);
}

public interface IUserService
{
    Task<IEnumerable<UserInfoDto>> GetAllAsync();
    Task<UserInfoDto?> GetByIdAsync(int id);
    Task<UserInfoDto> CreateAsync(CreateUserDto dto);
    Task<UserInfoDto?> UpdateAsync(int id, UpdateUserDto dto);
    Task<bool> ResetPasswordAsync(int id, ResetPasswordDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface IReminderService
{
    Task<ReminderHistoryDto> RegistrarRecordatorioWhatsAppAsync(RegistrarRecordatorioWhatsAppDto dto);
    Task<IEnumerable<ReminderHistoryDto>> GetHistorialAsync(int customerId);
}

public interface IStoreSettingsService
{
    Task<StoreSettingsDto> GetAsync();
    Task<StoreSettingsDto> UpdateAsync(UpdateStoreSettingsDto dto);
    Task<StoreSettingsDto> UploadLogoAsync(Stream fileStream, string fileName);
}
