using FluentAssertions;
using Moq;
using SupplierService.Application.Features.Suppliers.Commands.DeleteSupplier;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;

namespace SupplierService.Application.Tests.Features.Suppliers;

public class DeleteSupplierHandlerTests
{
    private readonly Mock<ISupplierRepository> _repoMock;
    private readonly DeleteSupplierHandler _handler;

    public DeleteSupplierHandlerTests()
    {
        _repoMock = new Mock<ISupplierRepository>();
        _handler = new DeleteSupplierHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenSupplierNotFound()
    {
        var supplierId = Guid.NewGuid();
        var command = new DeleteSupplierCommand(supplierId);

        _repoMock
            .Setup(repo => repo.GetByIdAsync(supplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Supplier {supplierId} not found.");
    }

    [Fact]
    public async Task Handle_ShouldSetIsDeletedToTrue_AndSaveChanges_WhenSupplierExists()
    {
        var supplierId = Guid.NewGuid();
        var command = new DeleteSupplierCommand(supplierId);

        var supplier = new Supplier
        {
            Id = supplierId,
            Name = "Trang trại Cà phê Cầu Đất",
            IsDeleted = false
        };

        _repoMock
            .Setup(repo => repo.GetByIdAsync(supplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        await _handler.Handle(command, CancellationToken.None);

        supplier.IsDeleted.Should().BeTrue();
        _repoMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}