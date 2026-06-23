using FluentAssertions;
using Moq;
using TinaStore.Application.DTOs;
using TinaStore.Application.Services;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Tests.Unit;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repoMock;
    private readonly CategoryService _sut;

    // Categoría del sistema que no debe poder tocarse
    private static readonly Category SinCategoria = new()
    {
        Id = 99,
        Name = "Sin categoría",
        Description = "Categoría de reserva para productos sin clasificar",
        IsActive = true,
        Products = []
    };

    private static Category NuevaCategoria(int id = 1, string nombre = "General") => new()
    {
        Id = id,
        Name = nombre,
        IsActive = true,
        Products = []
    };

    public CategoryServiceTests()
    {
        _repoMock = new Mock<ICategoryRepository>();
        _sut = new CategoryService(_repoMock.Object);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_RetornaTodasLasCategorias()
    {
        var lista = new List<Category> { NuevaCategoria(), SinCategoria };
        _repoMock.Setup(r => r.GetAllWithProductsAsync(default)).ReturnsAsync(lista);

        var resultado = await _sut.GetAllAsync();

        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_SoloActivas_FiltrasInactivas()
    {
        var lista = new List<Category>
        {
            new() { Id = 1, Name = "Activa",   IsActive = true,  Products = [] },
            new() { Id = 2, Name = "Inactiva",  IsActive = false, Products = [] }
        };
        _repoMock.Setup(r => r.GetAllWithProductsAsync(default)).ReturnsAsync(lista);

        var resultado = await _sut.GetAllAsync(soloActivas: true);

        resultado.Should().HaveCount(1);
        resultado.First().Name.Should().Be("Activa");
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_NombreValido_CreaYRetornaDto()
    {
        _repoMock.Setup(r => r.GetAllWithProductsAsync(default))
                 .ReturnsAsync(new List<Category>());
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Category>(), default))
                 .ReturnsAsync((Category c, CancellationToken _) => c);
        _repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var dto = new CreateCategoryDto("Skincare", "Cuidado de la piel");
        var resultado = await _sut.CreateAsync(dto);

        resultado.Should().NotBeNull();
        resultado.Name.Should().Be("Skincare");
    }

    [Fact]
    public async Task CreateAsync_NombreReservadoSinCategoria_LanzaExcepcion()
    {
        _repoMock.Setup(r => r.GetAllWithProductsAsync(default))
                 .ReturnsAsync(new List<Category>());

        var dto = new CreateCategoryDto("Sin categoría", null);

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*reservado*");
    }

    [Fact]
    public async Task CreateAsync_NombreDuplicado_LanzaExcepcion()
    {
        _repoMock.Setup(r => r.GetAllWithProductsAsync(default))
                 .ReturnsAsync(new List<Category> { NuevaCategoria(1, "General") });

        var dto = new CreateCategoryDto("General", null);

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe*");
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_CategoriaExiste_ActualizaYRetornaDto()
    {
        var cat = NuevaCategoria(3, "Medias");
        _repoMock.Setup(r => r.GetByIdAsync(3, default)).ReturnsAsync(cat);
        _repoMock.Setup(r => r.UpdateAsync(cat, default)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var dto = new UpdateCategoryDto("Medias Actualizado", null, true);
        var resultado = await _sut.UpdateAsync(3, dto);

        resultado.Should().NotBeNull();
        resultado!.Name.Should().Be("Medias Actualizado");
    }

    [Fact]
    public async Task UpdateAsync_IdEsSinCategoria_LanzaExcepcion()
    {
        var dto = new UpdateCategoryDto("Otro nombre", null, true);

        var act = async () => await _sut.UpdateAsync(99, dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no puede modificarse*");
    }

    [Fact]
    public async Task UpdateAsync_CategoriaNoExiste_RetornaNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((Category?)null);

        var resultado = await _sut.UpdateAsync(999, new UpdateCategoryDto("X", null, true));

        resultado.Should().BeNull();
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_CategoriaExisteSinProductos_RetornaTrue()
    {
        var cat = NuevaCategoria(5, "Temporal");
        _repoMock.Setup(r => r.GetByIdAsync(5, default)).ReturnsAsync(cat);
        _repoMock.Setup(r => r.DeleteAsync(cat, default)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var ok = await _sut.DeleteAsync(5);

        ok.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_IdEsSinCategoria_LanzaExcepcion()
    {
        var act = async () => await _sut.DeleteAsync(99);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no puede eliminarse*");
    }

    [Fact]
    public async Task DeleteAsync_CategoriaNoExiste_RetornaFalse()
    {
        _repoMock.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((Category?)null);

        var ok = await _sut.DeleteAsync(999);

        ok.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_CategoriaConProductos_ReasignaProductosASinCategoria()
    {
        var producto1 = new Product { Id = 1, Name = "P1", SalePrice = 100, CategoryId = 4 };
        var producto2 = new Product { Id = 2, Name = "P2", SalePrice = 200, CategoryId = 4 };
        var cat = new Category
        {
            Id = 4,
            Name = "A eliminar",
            IsActive = true,
            Products = [producto1, producto2]
        };

        _repoMock.Setup(r => r.GetByIdAsync(4, default)).ReturnsAsync(cat);
        _repoMock.Setup(r => r.DeleteAsync(cat, default)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        await _sut.DeleteAsync(4);

        producto1.CategoryId.Should().Be(99, "los productos deben reasignarse a Sin categoría");
        producto2.CategoryId.Should().Be(99, "los productos deben reasignarse a Sin categoría");
    }
}
