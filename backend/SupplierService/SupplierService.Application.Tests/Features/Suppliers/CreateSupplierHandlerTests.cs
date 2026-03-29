using FluentAssertions;
using Moq;
using SupplierService.Application.Features.Suppliers.Commands.CreateSupplier;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;

namespace SupplierService.Application.Tests.Features.Suppliers;

public class CreateSupplierHandlerTests
{
    private readonly Mock<ISupplierRepository> _repoMock;
    private readonly CreateSupplierHandler _handler;

    public CreateSupplierHandlerTests()
    {
        _repoMock = new Mock<ISupplierRepository>();
        _handler = new CreateSupplierHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenTaxCodeAlreadyExists()
    {
        var command = new CreateSupplierCommand
        {
            Name = "Công ty TNHH Sữa Tươi Thanh Trùng",
            TaxCode = "0312345678"
        };

        _repoMock
            .Setup(x => x.TaxCodeExistsAsync(command.TaxCode, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Supplier tax code '{command.TaxCode}' already exists.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ShouldCreateSupplier_AndSkipTaxCodeCheck_WhenTaxCodeIsNullOrWhiteSpace(string? emptyTaxCode)
    {
        var command = new CreateSupplierCommand
        {
            Name = "Đại lý Đá lạnh Nhựt Trí",
            ContactPerson = "Anh Trí",
            Phone = "0909123456",
            TaxCode = emptyTaxCode
        };

        Supplier? savedSupplier = null;
        _repoMock
            .Setup(x => x.AddAsync(It.IsAny<Supplier>(), It.IsAny<CancellationToken>()))
            .Callback<Supplier, CancellationToken>((s, _) => savedSupplier = s);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Code.Should().StartWith("NCC-");

        savedSupplier.Should().NotBeNull();
        savedSupplier!.Name.Should().Be(command.Name);
        savedSupplier.TaxCode.Should().Be(emptyTaxCode);
        savedSupplier.IsActive.Should().BeTrue();

        _repoMock.Verify(x => x.TaxCodeExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
        _repoMock.Verify(x => x.AddAsync(It.IsAny<Supplier>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateSupplier_WhenTaxCodeIsValidAndDoesNotExist()
    {
        var command = new CreateSupplierCommand
        {
            Name = "Vựa Trà Oolong Bảo Lộc",
            ContactPerson = "Cô Năm",
            Phone = "0987654321",
            Email = "conam@traoolong.vn",
            Address = "Bảo Lộc, Lâm Đồng",
            TaxCode = "0101234567"
        };

        _repoMock
            .Setup(x => x.TaxCodeExistsAsync(command.TaxCode, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Supplier? savedSupplier = null;
        _repoMock
            .Setup(x => x.AddAsync(It.IsAny<Supplier>(), It.IsAny<CancellationToken>()))
            .Callback<Supplier, CancellationToken>((s, _) => savedSupplier = s);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Code.Should().StartWith("NCC-");

        savedSupplier.Should().NotBeNull();
        savedSupplier!.Id.Should().Be(result.Id);
        savedSupplier.Code.Should().Be(result.Code);
        savedSupplier.Name.Should().Be(command.Name);
        savedSupplier.ContactPerson.Should().Be(command.ContactPerson);
        savedSupplier.Phone.Should().Be(command.Phone);
        savedSupplier.Email.Should().Be(command.Email);
        savedSupplier.Address.Should().Be(command.Address);
        savedSupplier.TaxCode.Should().Be(command.TaxCode);
        savedSupplier.IsActive.Should().BeTrue();

        _repoMock.Verify(x => x.AddAsync(It.IsAny<Supplier>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}