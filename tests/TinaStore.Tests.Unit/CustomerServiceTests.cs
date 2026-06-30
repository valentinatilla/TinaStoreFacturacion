using FluentAssertions;
using Moq;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Application.Services;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Tests.Unit;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repoMock;
    private readonly Mock<IAppClock> _clockMock;
    private readonly CustomerService _sut;

    public CustomerServiceTests()
    {
        _repoMock  = new Mock<ICustomerRepository>();
        _clockMock = new Mock<IAppClock>();
        _clockMock.Setup(c => c.Now).Returns(new DateTime(2026, 1, 1, 10, 0, 0));
        _sut = new CustomerService(_repoMock.Object, _clockMock.Object);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_SinFiltro_RetornaTodosLosClientes()
    {
        var clientes = new List<Customer>
        {
            new() { Id = 1, FullName = "Ana García",  IsActive = true,  Invoices = [] },
            new() { Id = 2, FullName = "Luis Pérez",  IsActive = false, Invoices = [] }
        };
        _repoMock.Setup(r => r.GetAllWithInvoicesAsync(default)).ReturnsAsync(clientes);

        var resultado = await _sut.GetAllAsync();

        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_SoloActivos_RetornaUnicamenteActivos()
    {
        var clientes = new List<Customer>
        {
            new() { Id = 1, FullName = "Ana García",  IsActive = true,  Invoices = [] },
            new() { Id = 2, FullName = "Luis Pérez",  IsActive = false, Invoices = [] }
        };
        _repoMock.Setup(r => r.GetAllWithInvoicesAsync(default)).ReturnsAsync(clientes);

        var resultado = await _sut.GetAllAsync(soloActivos: true);

        resultado.Should().HaveCount(1);
        resultado.First().FullName.Should().Be("Ana García");
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ClienteExiste_RetornaDto()
    {
        var cliente = new Customer { Id = 5, FullName = "María López", IsActive = true, Invoices = [] };
        _repoMock.Setup(r => r.GetWithInvoicesAsync(5, default)).ReturnsAsync(cliente);

        var resultado = await _sut.GetByIdAsync(5);

        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(5);
        resultado.FullName.Should().Be("María López");
    }



    [Fact]
    public async Task GetByIdAsync_ClienteNoExiste_RetornaNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Customer?)null);

        var resultado = await _sut.GetByIdAsync(99);

        resultado.Should().BeNull();
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_DatosValidos_AgregaClienteYRetornaDto()
    {
        var dto = new CreateCustomerDto("Carlos Ruiz", "CC", "123456", "3001234567", null, null, null);

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Customer>(), default))
                 .ReturnsAsync((Customer c, CancellationToken _) => c);
        _repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var resultado = await _sut.CreateAsync(dto);

        resultado.Should().NotBeNull();
        resultado.FullName.Should().Be("Carlos Ruiz");
        resultado.DocumentNumber.Should().Be("123456");
        _repoMock.Verify(r => r.AddAsync(It.Is<Customer>(c => c.FullName == "Carlos Ruiz"), default), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ClienteExiste_ActualizaYRetornaDto()
    {
        var existente = new Customer { Id = 3, FullName = "Pedro Original", IsActive = true, Invoices = [] };
        _repoMock.Setup(r => r.GetWithInvoicesAsync(3, default)).ReturnsAsync(existente);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Customer>(), default)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var dto = new UpdateCustomerDto("Pedro Actualizado", "CC", "99999", "3009999999", null, null, null, true);
        var resultado = await _sut.UpdateAsync(3, dto);

        resultado.Should().NotBeNull();
        resultado!.FullName.Should().Be("Pedro Actualizado");
        existente.FullName.Should().Be("Pedro Actualizado");
    }

    [Fact]
    public async Task UpdateAsync_ClienteNoExiste_RetornaNull()
    {
        _repoMock.Setup(r => r.GetWithInvoicesAsync(99, default)).ReturnsAsync((Customer?)null);
        var dto = new UpdateCustomerDto("X", null, null, null, null, null, null, true);

        var resultado = await _sut.UpdateAsync(99, dto);

        resultado.Should().BeNull();
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Customer>(), default), Times.Never);
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ClienteExiste_RetornaTrue()
    {
        var cliente = new Customer { Id = 1, FullName = "A", IsActive = true };
        _repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(cliente);
        _repoMock.Setup(r => r.DeleteAsync(cliente, default)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var ok = await _sut.DeleteAsync(1);

        ok.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ClienteNoExiste_RetornaFalse()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Customer?)null);

        var ok = await _sut.DeleteAsync(99);

        ok.Should().BeFalse();
    }

    // ── SearchAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_TerminoCoincide_RetornaResultados()
    {
        var resultados = new List<Customer> { new() { Id = 1, FullName = "Ana García" } };
        _repoMock.Setup(r => r.SearchAsync("Ana", default)).ReturnsAsync(resultados);

        var resultado = await _sut.SearchAsync("Ana");

        resultado.Should().HaveCount(1);
        resultado.First().FullName.Should().Be("Ana García");
    }
}
