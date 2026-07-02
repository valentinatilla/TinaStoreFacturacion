using FluentAssertions;
using Moq;
using TinaStore.Application.DTOs;
using TinaStore.Application.Services;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Enums;
using TinaStore.Domain.Exceptions;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Tests.Unit;

public class ExpenseServiceTests
{
    private readonly Mock<IExpenseRepository> _expenseRepoMock;
    private readonly Mock<IRepository<ExpenseCategory>> _categoryRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IRepository<InvoiceDetail>> _invoiceDetailRepoMock;
    private readonly ExpenseService _sut;

    private static readonly ExpenseCategory _categoria = new()
    {
        Id = 1, Name = "Servicios", IsActive = true
    };

    public ExpenseServiceTests()
    {
        _expenseRepoMock       = new Mock<IExpenseRepository>();
        _categoryRepoMock      = new Mock<IRepository<ExpenseCategory>>();
        _productRepoMock       = new Mock<IProductRepository>();
        _invoiceDetailRepoMock = new Mock<IRepository<InvoiceDetail>>();
        _sut = new ExpenseService(_expenseRepoMock.Object, _categoryRepoMock.Object, _productRepoMock.Object, _invoiceDetailRepoMock.Object);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_RetornaEgresosOrdenadosPorFechaDesc()
    {
        var egresos = new List<Expense>
        {
            new() { Id = 1, ExpenseDate = DateTime.Today.AddDays(-2), Description = "A", Amount = 100, ExpenseCategory = _categoria, ExpenseCategoryId = 1 },
            new() { Id = 2, ExpenseDate = DateTime.Today,             Description = "B", Amount = 200, ExpenseCategory = _categoria, ExpenseCategoryId = 1 },
        };
        _expenseRepoMock.Setup(r => r.GetAllWithNavigationAsync(default)).ReturnsAsync(egresos);

        var resultado = await _sut.GetAllAsync();

        resultado.Should().HaveCount(2);
        resultado.First().Description.Should().Be("B"); // más reciente primero
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_EgresoExiste_RetornaDto()
    {
        var egreso = new Expense { Id = 3, Description = "Arriendo", Amount = 500_000, ExpenseCategory = _categoria, ExpenseCategoryId = 1 };
        _expenseRepoMock.Setup(r => r.GetByIdAsync(3, default)).ReturnsAsync(egreso);

        var resultado = await _sut.GetByIdAsync(3);

        resultado.Should().NotBeNull();
        resultado!.Description.Should().Be("Arriendo");
        resultado.Amount.Should().Be(500_000);
    }

    [Fact]
    public async Task GetByIdAsync_EgresoNoExiste_RetornaNull()
    {
        _expenseRepoMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Expense?)null);

        var resultado = await _sut.GetByIdAsync(99);

        resultado.Should().BeNull();
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_CategoriaValida_CreaEgresoYRetornaDto()
    {
        var dto = new CreateExpenseDto(DateTime.Today, "Agua y luz", 80_000, null, 1, null, null);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(_categoria);
        _expenseRepoMock.Setup(r => r.AddAsync(It.IsAny<Expense>(), default))
                        .ReturnsAsync((Expense e, CancellationToken _) => e);
        _expenseRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var resultado = await _sut.CreateAsync(dto);

        resultado.Should().NotBeNull();
        resultado.Description.Should().Be("Agua y luz");
        resultado.Amount.Should().Be(80_000);
        resultado.StatusName.Should().Be("Active");
        _expenseRepoMock.Verify(r => r.AddAsync(It.IsAny<Expense>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CategoriaNoExiste_LanzaEntityNotFoundException()
    {
        var dto = new CreateExpenseDto(DateTime.Today, "Test", 1000, null, 99, null, null);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((ExpenseCategory?)null);

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<EntityNotFoundException>();
        _expenseRepoMock.Verify(r => r.AddAsync(It.IsAny<Expense>(), default), Times.Never);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_EgresoActivo_ActualizaYRetornaDto()
    {
        var existente = new Expense { Id = 4, Description = "Viejo", Amount = 100, Status = ExpenseStatus.Active, ExpenseCategory = _categoria, ExpenseCategoryId = 1 };
        _expenseRepoMock.Setup(r => r.GetByIdAsync(4, default)).ReturnsAsync(existente);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(_categoria);
        _expenseRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Expense>(), default)).Returns(Task.CompletedTask);
        _expenseRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var dto = new UpdateExpenseDto(DateTime.Today, "Nuevo", 200, null, 1, null, null);
        var resultado = await _sut.UpdateAsync(4, dto);

        resultado.Should().NotBeNull();
        resultado!.Description.Should().Be("Nuevo");
        resultado.Amount.Should().Be(200);
    }

    [Fact]
    public async Task UpdateAsync_EgresoAnulado_LanzaDomainException()
    {
        var anulado = new Expense { Id = 5, Description = "X", Status = ExpenseStatus.Cancelled, ExpenseCategory = _categoria, ExpenseCategoryId = 1 };
        _expenseRepoMock.Setup(r => r.GetByIdAsync(5, default)).ReturnsAsync(anulado);

        var dto = new UpdateExpenseDto(DateTime.Today, "Y", 100, null, 1, null, null);
        var act = async () => await _sut.UpdateAsync(5, dto);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*anulado*");
    }

    // ── CancelAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelAsync_EgresoExiste_CambiaEstadoYRetornaTrue()
    {
        var egreso = new Expense { Id = 6, Status = ExpenseStatus.Active, ExpenseCategory = _categoria, ExpenseCategoryId = 1 };
        _expenseRepoMock.Setup(r => r.GetByIdAsync(6, default)).ReturnsAsync(egreso);
        _expenseRepoMock.Setup(r => r.UpdateAsync(egreso, default)).Returns(Task.CompletedTask);
        _expenseRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var ok = await _sut.CancelAsync(6);

        ok.Should().BeTrue();
        egreso.Status.Should().Be(ExpenseStatus.Cancelled);
    }

    [Fact]
    public async Task CancelAsync_EgresoNoExiste_RetornaFalse()
    {
        _expenseRepoMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Expense?)null);

        var ok = await _sut.CancelAsync(99);

        ok.Should().BeFalse();
    }
}
