using FluentAssertions;
using Moq;
using TinaStore.Application.DTOs;
using TinaStore.Application.Services;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Enums;
using TinaStore.Domain.Exceptions;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Tests.Unit;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IAccountReceivableRepository> _receivableRepoMock;
    private readonly Mock<IRepository<Payment>> _paymentRepoMock;
    private readonly Mock<IRepository<InventoryMovement>> _movementRepoMock;
    private readonly Mock<IRepository<StoreSettings>> _settingsMock;
    private readonly InvoiceService _sut;

    // Instancia fresca por cada test para evitar que CurrentStock se contamine
    private static Product NuevoProducto() => new()
    {
        Id = 10,
        Name = "Producto Test",
        SalePrice = 1000,
        CurrentStock = 50,
        IsActive = true,
        CategoryId = 1,
        Category = new Category { Id = 1, Name = "General" }
    };

    // Instancia fresca por cada test para evitar que InvoiceConsecutive++ contamine otros tests
    private static StoreSettings NuevasSettings() => new()
    {
        Id = 1,
        StoreName = "Tina Store Test",
        InvoiceConsecutive = 1,
        AllowNegativeStock = false
    };

    public InvoiceServiceTests()
    {
        _invoiceRepoMock    = new Mock<IInvoiceRepository>();
        _productRepoMock    = new Mock<IProductRepository>();
        _receivableRepoMock = new Mock<IAccountReceivableRepository>();
        _paymentRepoMock    = new Mock<IRepository<Payment>>();
        _movementRepoMock   = new Mock<IRepository<InventoryMovement>>();
        _settingsMock       = new Mock<IRepository<StoreSettings>>();

        _sut = new InvoiceService(
            _invoiceRepoMock.Object,
            _productRepoMock.Object,
            _receivableRepoMock.Object,
            _paymentRepoMock.Object,
            _movementRepoMock.Object,
            _settingsMock.Object);
    }

    // ── Helper: configura mocks base para crear una factura ───────────────────

    private void SetupCrearFactura(Product? producto = null, StoreSettings? settings = null)
    {
        var prod = producto ?? NuevoProducto();  // fresca cada vez
        var cfg  = settings  ?? NuevasSettings();  // fresca cada vez

        _settingsMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(cfg);
        _productRepoMock.Setup(r => r.GetByIdAsync(prod.Id, default)).ReturnsAsync(prod);
        _productRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), default)).Returns(Task.CompletedTask);
        _receivableRepoMock.Setup(r => r.GetByCustomerAsync(It.IsAny<int>(), default))
                            .ReturnsAsync((AccountReceivable?)null);
        _receivableRepoMock.Setup(r => r.AddAsync(It.IsAny<AccountReceivable>(), default))
                            .ReturnsAsync((AccountReceivable a, CancellationToken _) => a);
        _invoiceRepoMock.Setup(r => r.AddAsync(It.IsAny<Invoice>(), default))
                        .ReturnsAsync((Invoice i, CancellationToken _) => i);
        _invoiceRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);
        _settingsMock.Setup(r => r.UpdateAsync(It.IsAny<StoreSettings>(), default)).Returns(Task.CompletedTask);

        // GetWithDetailsAsync devuelve la factura guardada
        _invoiceRepoMock.Setup(r => r.GetWithDetailsAsync(It.IsAny<int>(), default))
                        .ReturnsAsync((int _, CancellationToken _) =>
                        {
                            var inv = new Invoice
                            {
                                Id = 1,
                                InvoiceNumber = $"TIN-{cfg.InvoiceConsecutive:D6}",
                                CustomerId = 1,
                                Customer = new Customer { Id = 1, FullName = "Cliente Test" },
                                Total = 1000,
                                AmountPaid = 0,
                                Status = InvoiceStatus.Pending
                            };
                            return inv;
                        });
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_SinDetalles_LanzaDomainException()
    {
        _settingsMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(NuevasSettings());
        var dto = new CreateInvoiceDto(1, 0, 0, null, [], null);

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*al menos un producto*");
    }

    [Fact]
    public async Task CreateAsync_ProductoSinStock_LanzaInsufficientStockException()
    {
        var sinStock = new Product
        {
            Id = 10, Name = "Sin Stock", CurrentStock = 0,
            IsActive = true, CategoryId = 1, Category = new Category { Id = 1, Name = "G" }
        };
        _settingsMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(NuevasSettings());
        _productRepoMock.Setup(r => r.GetByIdAsync(10, default)).ReturnsAsync(sinStock);

        var dto = new CreateInvoiceDto(1, 0, 0, null,
            [new CreateInvoiceDetailDto(10, 5, 1000)], null);

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<InsufficientStockException>();
    }

    [Fact]
    public async Task CreateAsync_PagoCompletoCubrio_EstadoPagada()
    {
        SetupCrearFactura();
        var pago = new RegisterPaymentDto(1, 1000, null, null);
        var dto  = new CreateInvoiceDto(1, 0, 0, null,
            [new CreateInvoiceDetailDto(10, 1, 1000)], pago);

        // Simula que GetWithDetailsAsync retorna factura pagada
        _invoiceRepoMock.Setup(r => r.GetWithDetailsAsync(It.IsAny<int>(), default))
                        .ReturnsAsync(new Invoice
                        {
                            Id = 1, InvoiceNumber = "TIN-000001",
                            CustomerId = 1,
                            Customer = new Customer { Id = 1, FullName = "Cliente Test" },
                            Total = 1000, AmountPaid = 1000, Status = InvoiceStatus.Paid
                        });

        var resultado = await _sut.CreateAsync(dto);

        resultado.Should().NotBeNull();
        resultado.StatusName.Should().Be("Pagada");
    }

    [Fact]
    public async Task CreateAsync_SinPago_CreaFacturaPendienteYCXC()
    {
        SetupCrearFactura();
        var dto = new CreateInvoiceDto(1, 0, 0, null,
            [new CreateInvoiceDetailDto(10, 1, 1000)], null);

        var resultado = await _sut.CreateAsync(dto);

        resultado.Should().NotBeNull();
        _receivableRepoMock.Verify(r => r.AddAsync(It.IsAny<AccountReceivable>(), default), Times.Once);
        _settingsMock.Verify(r => r.UpdateAsync(It.Is<StoreSettings>(s => s.InvoiceConsecutive == 2), default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DescuentoYImpuesto_CalculaTotalCorrectamente()
    {
        SetupCrearFactura();
        // subtotal = 2000, descuento = 200, impuesto = 100 → total = 1900
        var dto = new CreateInvoiceDto(1, 200, 100, null,
            [new CreateInvoiceDetailDto(10, 2, 1000)], null);

        _invoiceRepoMock.Setup(r => r.GetWithDetailsAsync(It.IsAny<int>(), default))
                        .ReturnsAsync(new Invoice
                        {
                            Id = 1, InvoiceNumber = "TIN-000001",
                            CustomerId = 1,
                            Customer = new Customer { Id = 1, FullName = "C" },
                            Subtotal = 2000, DiscountAmount = 200, TaxAmount = 100,
                            Total = 1900, AmountPaid = 0, Status = InvoiceStatus.Pending
                        });

        var resultado = await _sut.CreateAsync(dto);

        resultado.Total.Should().Be(1900);
        resultado.DiscountAmount.Should().Be(200);
        resultado.TaxAmount.Should().Be(100);
    }

    // ── RegisterPaymentAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task RegisterPaymentAsync_FacturaAnulada_LanzaInvoiceCancelledException()
    {
        var cancelada = new Invoice
        {
            Id = 2, Status = InvoiceStatus.Cancelled, Total = 1000, AmountPaid = 0,
            CustomerId = 1, Customer = new Customer { Id = 1, FullName = "C" }
        };
        _invoiceRepoMock.Setup(r => r.GetWithDetailsAsync(2, default)).ReturnsAsync(cancelada);

        var dto = new RegisterPaymentDto(1, 500, null, null);
        var act = async () => await _sut.RegisterPaymentAsync(2, dto);

        await act.Should().ThrowAsync<InvoiceCancelledException>();
    }

    [Fact]
    public async Task RegisterPaymentAsync_PagoExcedeSaldo_AplicaSoloElSaldo()
    {
        var factura = new Invoice
        {
            Id = 3, Status = InvoiceStatus.Pending,
            Total = 1000, AmountPaid = 0, CustomerId = 1,
            Customer = new Customer { Id = 1, FullName = "C" }
        };
        _invoiceRepoMock.SetupSequence(r => r.GetWithDetailsAsync(3, default))
            .ReturnsAsync(factura)
            .ReturnsAsync(new Invoice
            {
                Id = 3, InvoiceNumber = "TIN-000003",
                CustomerId = 1, Customer = new Customer { Id = 1, FullName = "C" },
                Total = 1000, AmountPaid = 1000, Status = InvoiceStatus.Paid
            });
        _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>(), default))
                        .ReturnsAsync((Payment p, CancellationToken _) => p);
        _invoiceRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Invoice>(), default)).Returns(Task.CompletedTask);
        _receivableRepoMock.Setup(r => r.GetByCustomerAsync(1, default))
                           .ReturnsAsync((AccountReceivable?)null);
        _invoiceRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var dto = new RegisterPaymentDto(1, 9999 /*excede el saldo*/, null, null);
        var resultado = await _sut.RegisterPaymentAsync(3, dto);

        resultado.Should().NotBeNull();
        // El pago debe haber sido limitado al saldo (Balance = Total - AmountPaid = 1000 - 0 = 1000)
        _paymentRepoMock.Verify(
            r => r.AddAsync(It.Is<Payment>(p => p.Amount == 1000), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── CancelAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelAsync_FacturaExistente_AnulaYRevierteCXC()
    {
        var detalle = new InvoiceDetail
        {
            ProductId = 10, ProductName = "PT", Quantity = 2, UnitPrice = 500
        };
        var factura = new Invoice
        {
            Id = 5, Status = InvoiceStatus.Pending,
            Total = 1000, AmountPaid = 0, CustomerId = 1,
            Customer = new Customer { Id = 1, FullName = "C" },
            InvoiceNumber = "TIN-000005",
            Details = [detalle]
        };
        var cxc = new AccountReceivable { CustomerId = 1, TotalDebt = 1000, TotalPaid = 0 };

        _invoiceRepoMock.SetupSequence(r => r.GetWithDetailsAsync(5, default))
            .ReturnsAsync(factura)
            .ReturnsAsync(new Invoice
            {
                Id = 5, InvoiceNumber = "TIN-000005",
                CustomerId = 1, Customer = new Customer { Id = 1, FullName = "C" },
                Total = 1000, AmountPaid = 0,
                Status = InvoiceStatus.Cancelled, CancellationReason = "Prueba",
                Details = [detalle]
            });
        var prod = NuevoProducto(); // CurrentStock = 50, Quantity = 2 → tras cancelar = 52
        _productRepoMock.Setup(r => r.GetByIdAsync(10, default)).ReturnsAsync(prod);
        _productRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), default)).Returns(Task.CompletedTask);
        _movementRepoMock.Setup(r => r.AddAsync(It.IsAny<InventoryMovement>(), default))
                         .ReturnsAsync((InventoryMovement m, CancellationToken _) => m);
        _receivableRepoMock.Setup(r => r.GetByCustomerAsync(1, default)).ReturnsAsync(cxc);
        _receivableRepoMock.Setup(r => r.UpdateAsync(It.IsAny<AccountReceivable>(), default)).Returns(Task.CompletedTask);
        _invoiceRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Invoice>(), default)).Returns(Task.CompletedTask);
        _invoiceRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var resultado = await _sut.CancelAsync(5, new CancelInvoiceDto("Prueba"));

        resultado.Should().NotBeNull();
        resultado!.StatusName.Should().Be("Anulada");
        // Stock debe haberse revertido
        _productRepoMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.CurrentStock == 52), default), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_FacturaYaAnulada_LanzaInvoiceCancelledException()
    {
        var cancelada = new Invoice
        {
            Id = 6, Status = InvoiceStatus.Cancelled, Total = 1000, AmountPaid = 0,
            CustomerId = 1, Customer = new Customer { Id = 1, FullName = "C" }
        };
        _invoiceRepoMock.Setup(r => r.GetWithDetailsAsync(6, default)).ReturnsAsync(cancelada);

        var act = async () => await _sut.CancelAsync(6, new CancelInvoiceDto("X"));

        await act.Should().ThrowAsync<InvoiceCancelledException>();
    }

    [Fact]
    public async Task CancelAsync_FacturaNoExiste_RetornaNull()
    {
        _invoiceRepoMock.Setup(r => r.GetWithDetailsAsync(99, default)).ReturnsAsync((Invoice?)null);

        var resultado = await _sut.CancelAsync(99, new CancelInvoiceDto("X"));

        resultado.Should().BeNull();
    }
}
