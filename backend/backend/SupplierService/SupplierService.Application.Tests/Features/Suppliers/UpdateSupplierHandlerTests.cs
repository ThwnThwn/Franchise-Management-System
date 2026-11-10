using FluentAssertions;
using Moq;
using SupplierService.Application.Features.Suppliers.Commands.UpdateSupplier;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;

namespace SupplierService.Application.Tests.Features.Suppliers;

public class UpdateSupplierHandlerTests
{
    private readonly Mock<ISupplierRepository> _repoMock;
    private readonly UpdateSupplierHandler _handler;

    public UpdateSupplierHandlerTests()
    {
        _repoMock = new Mock<ISupplierRepository>();
        _handler = new UpdateSupplierHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenSupplierNotFound()
    {
        var supplierId = Guid.NewGuid();
        var command = new UpdateSupplierCommand { Id = supplierId };

        _repoMock
            .Setup(repo => repo.GetByIdAsync(supplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Supplier {supplierId} not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNewTaxCodeAlreadyExists()
    {
        var supplierId = Guid.NewGuid();
        var newTaxCode = "0312345678";
        var command = new UpdateSupplierCommand { Id = supplierId, TaxCode = newTaxCode };
        var existingSupplier = new Supplier { Id = supplierId, TaxCode = "OLD-TAX-CODE" };

        _repoMock
            .Setup(repo => repo.GetByIdAsync(supplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSupplier);

        _repoMock
            .Setup(repo => repo.TaxCodeExistsAsync(newTaxCode, supplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Supplier tax code '{newTaxCode}' already exists.");
    }

    [Fact]
    public async Task Handle_ShouldUpdateAllFields_WhenCommandHasAllValues()
    {
        var supplierId = Guid.NewGuid();
        var command = new UpdateSupplierCommand
        {
            Id = supplierId,
            Name = "Xưởng Rang Cà Phê Hữu Cơ",
            ContactPerson = "Anh Tú",
            Phone = "0912345678",
            Email = "anhtu@organiccoffee.vn",
            Address = "Đà Lạt, Lâm Đồng",
            TaxCode = "0102030405",
            IsActive = false
        };

        var supplier = new Supplier { Id = supplierId, Name = "Tên cũ", TaxCode = "Mã thuế cũ" };

        _repoMock.Setup(repo => repo.GetByIdAsync(supplierId, It.IsAny<CancellationToken>())).ReturnsAsync(supplier);
        _repoMock.Setup(repo => repo.TaxCodeExistsAsync(command.TaxCode!, supplierId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await _handler.Handle(command, CancellationToken.None);

        supplier.Name.Should().Be(command.Name);
        supplier.TaxCode.Should().Be(command.TaxCode);
        supplier.IsActive.Should().BeFalse();
        _repoMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotUpdateFields_WhenCommandValuesAreNull()
    {
        var supplierId = Guid.NewGuid();
        var command = new UpdateSupplierCommand { Id = supplierId };
        var supplier = new Supplier { Id = supplierId, Name = "Đại lý Sữa Ông Thọ", TaxCode = "1122334455", IsActive = true };

        _repoMock.Setup(repo => repo.GetByIdAsync(supplierId, It.IsAny<CancellationToken>())).ReturnsAsync(supplier);

        await _handler.Handle(command, CancellationToken.None);

        supplier.Name.Should().Be("Đại lý Sữa Ông Thọ");
        _repoMock.Verify(repo => repo.TaxCodeExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCheckTaxCode_WhenTaxCodeIsSameAsCurrent()
    {
        var supplierId = Guid.NewGuid();
        var currentTaxCode = "5566778899";
        var command = new UpdateSupplierCommand { Id = supplierId, TaxCode = currentTaxCode, Name = "Cập nhật tên" };
        var supplier = new Supplier { Id = supplierId, TaxCode = currentTaxCode };

        _repoMock.Setup(repo => repo.GetByIdAsync(supplierId, It.IsAny<CancellationToken>())).ReturnsAsync(supplier);

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(repo => repo.TaxCodeExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ShouldUpdateTaxCodeToEmpty_WithoutCheckingExistence_WhenTaxCodeIsWhitespace(string emptyTaxCode)
    {
        var supplierId = Guid.NewGuid();
        var command = new UpdateSupplierCommand { Id = supplierId, TaxCode = emptyTaxCode };
        var supplier = new Supplier { Id = supplierId, TaxCode = "TAX-123" };

        _repoMock.Setup(repo => repo.GetByIdAsync(supplierId, It.IsAny<CancellationToken>())).ReturnsAsync(supplier);

        await _handler.Handle(command, CancellationToken.None);

        supplier.TaxCode.Should().Be(emptyTaxCode);
        _repoMock.Verify(repo => repo.TaxCodeExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}