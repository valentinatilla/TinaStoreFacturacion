using FluentAssertions;
using TinaStore.Application.DTOs;
using TinaStore.Application.Validators;

namespace TinaStore.Tests.Unit;

/// <summary>
/// Tests unitarios para los validadores de clientes, productos y categorías
/// verificando los límites y reglas de negocio establecidos en las fases Q y R.
/// </summary>
public class ValidatorsTests
{
    // ══════════════════════════════════════════════════════════════════════════
    // CLIENTES
    // ══════════════════════════════════════════════════════════════════════════

    private static readonly CreateCustomerValidator _createClienteValidator = new();
    private static readonly UpdateCustomerValidator _updateClienteValidator = new();

    [Fact]
    public void Cliente_NombreVacio_FallaValidacion()
    {
        var dto = new CreateCustomerDto("", null, null, null, null, null, null);
        var resultado = _createClienteValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    [Fact]
    public void Cliente_NombreMasDe30Chars_FallaValidacion()
    {
        var nombre = new string('A', 31);
        var dto = new CreateCustomerDto(nombre, null, null, null, null, null, null);
        var resultado = _createClienteValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    [Fact]
    public void Cliente_NombreExacto30Chars_PasaValidacion()
    {
        var nombre = new string('A', 30);
        var dto = new CreateCustomerDto(nombre, null, null, null, null, null, null);
        var resultado = _createClienteValidator.Validate(dto);
        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("312456789")]    // 9 dígitos — muy corto
    [InlineData("30012345678")]  // 11 dígitos — muy largo
    [InlineData("300 123 456")]  // contiene espacios
    [InlineData("300-123-456")]  // contiene guiones
    [InlineData("+573001234567")] // incluye prefijo internacional
    public void Cliente_TelefonoNoTiene10Digitos_FallaValidacion(string telefono)
    {
        var dto = new CreateCustomerDto("Ana García", null, null, telefono, null, null, null);
        var resultado = _createClienteValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Phone");
    }

    [Fact]
    public void Cliente_TelefonoExacto10Digitos_PasaValidacion()
    {
        var dto = new CreateCustomerDto("Ana García", null, null, "3001234567", null, null, null);
        var resultado = _createClienteValidator.Validate(dto);
        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Cliente_TelefonoNulo_PasaValidacion()
    {
        var dto = new CreateCustomerDto("Ana García", null, null, null, null, null, null);
        var resultado = _createClienteValidator.Validate(dto);
        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Cliente_TelefonoVacio_PasaValidacion()
    {
        var dto = new CreateCustomerDto("Ana García", null, null, "", null, null, null);
        var resultado = _createClienteValidator.Validate(dto);
        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Cliente_EmailInvalido_FallaValidacion()
    {
        var dto = new CreateCustomerDto("Ana García", null, null, null, "no-es-email", null, null);
        var resultado = _createClienteValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void ClienteUpdate_TelefonoExacto10Digitos_PasaValidacion()
    {
        var dto = new UpdateCustomerDto("Ana García", null, null, "3159876543", null, null, null, true);
        var resultado = _updateClienteValidator.Validate(dto);
        resultado.IsValid.Should().BeTrue();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // PRODUCTOS
    // ══════════════════════════════════════════════════════════════════════════

    private static readonly CreateProductValidator _createProdValidator = new();
    private static readonly UpdateProductValidator _updateProdValidator = new();

    [Fact]
    public void Producto_NombreVacio_FallaValidacion()
    {
        var dto = new CreateProductDto(null, "", null, null, 0, 1000, 0, 0, 1, null);
        var resultado = _createProdValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Producto_NombreMasDe30Chars_FallaValidacion()
    {
        var nombre = new string('B', 31);
        var dto = new CreateProductDto(null, nombre, null, null, 0, 1000, 0, 0, 1, null);
        var resultado = _createProdValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Producto_SkuMasDe30Chars_FallaValidacion()
    {
        var sku = new string('S', 31);
        var dto = new CreateProductDto(sku, "Nombre", null, null, 0, 1000, 0, 0, 1, null);
        var resultado = _createProdValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Sku");
    }

    [Fact]
    public void Producto_SkuExacto30Chars_PasaValidacion()
    {
        var sku = new string('S', 30);
        var dto = new CreateProductDto(sku, "Nombre válido", null, null, 0, 1000, 0, 0, 1, null);
        var resultado = _createProdValidator.Validate(dto);
        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Producto_DescripcionMasDe300Chars_FallaValidacion()
    {
        var desc = new string('D', 301);
        var dto = new CreateProductDto(null, "Nombre", desc, null, 0, 1000, 0, 0, 1, null);
        var resultado = _createProdValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Producto_UnidadMasDe30Chars_FallaValidacion()
    {
        var unidad = new string('u', 31);
        var dto = new CreateProductDto(null, "Nombre", null, unidad, 0, 1000, 0, 0, 1, null);
        var resultado = _createProdValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Unit");
    }

    [Fact]
    public void Producto_PrecioVentaCero_FallaValidacion()
    {
        var dto = new CreateProductDto(null, "Nombre", null, null, 0, 0, 0, 0, 1, null);
        var resultado = _createProdValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "SalePrice");
    }

    [Fact]
    public void Producto_PrecioVentaNegativo_FallaValidacion()
    {
        var dto = new CreateProductDto(null, "Nombre", null, null, 0, -1, 0, 0, 1, null);
        var resultado = _createProdValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "SalePrice");
    }

    [Fact]
    public void Producto_PrecioVentaSuperaTecho_FallaValidacion()
    {
        var dto = new CreateProductDto(null, "Nombre", null, null, 0, 10_000_000m, 0, 0, 1, null);
        var resultado = _createProdValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "SalePrice");
    }

    [Fact]
    public void Producto_PrecioVentaEnTecho_PasaValidacion()
    {
        var dto = new CreateProductDto(null, "Nombre", null, null, 0, 9_999_999.99m, 0, 0, 1, null);
        var resultado = _createProdValidator.Validate(dto);
        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Producto_PrecioCompraSuperaTecho_FallaValidacion()
    {
        var dto = new CreateProductDto(null, "Nombre", null, null, 10_000_001m, 1000, 0, 0, 1, null);
        var resultado = _createProdValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "PurchasePrice");
    }

    [Fact]
    public void Producto_SinCategoria_FallaValidacion()
    {
        var dto = new CreateProductDto(null, "Nombre", null, null, 0, 1000, 0, 0, 0, null);
        var resultado = _createProdValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "CategoryId");
    }

    [Fact]
    public void ProductoUpdate_PrecioVentaCero_FallaValidacion()
    {
        var dto = new UpdateProductDto(null, "Nombre", null, null, 0, 0, 0, true, 1, null);
        var resultado = _updateProdValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "SalePrice");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // CATEGORÍAS
    // ══════════════════════════════════════════════════════════════════════════

    private static readonly CreateCategoryValidator _createCatValidator = new();
    private static readonly UpdateCategoryValidator _updateCatValidator = new();

    [Fact]
    public void Categoria_NombreVacio_FallaValidacion()
    {
        var dto = new CreateCategoryDto("", null);
        var resultado = _createCatValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Categoria_NombreMasDe30Chars_FallaValidacion()
    {
        var nombre = new string('C', 31);
        var dto = new CreateCategoryDto(nombre, null);
        var resultado = _createCatValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Categoria_NombreExacto30Chars_PasaValidacion()
    {
        var nombre = new string('C', 30);
        var dto = new CreateCategoryDto(nombre, null);
        var resultado = _createCatValidator.Validate(dto);
        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Categoria_DescripcionMasDe200Chars_FallaValidacion()
    {
        var desc = new string('D', 201);
        var dto = new CreateCategoryDto("Nombre", desc);
        var resultado = _createCatValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void CategoriaUpdate_NombreMasDe30Chars_FallaValidacion()
    {
        var nombre = new string('C', 31);
        var dto = new UpdateCategoryDto(nombre, null, true);
        var resultado = _updateCatValidator.Validate(dto);
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Name");
    }
}
