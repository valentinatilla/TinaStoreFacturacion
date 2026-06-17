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
