using FluentAssertions;
using Moq;
using SupplierService.Application.Features.Suppliers.Queries.GetSupplierById;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;

namespace SupplierService.Application.Tests.Features.Suppliers;

public class GetSupplierByIdHandlerTests
{
    private readonly Mock<ISupplierRepository> _repoMock;
    private readonly GetSupplierByIdHandler _handler;

    public GetSupplierByIdHandlerTests()
    {
        _repoMock = new Mock<ISupplierRepository>();
        _handler = new GetSupplierByIdHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenIdIsNotValidGuid()
    {
        var query = new GetSupplierByIdQuery("chuoi-id-khong-hop-le");

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid Supplier ID.");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenSupplierDoesNotExist()
    {
        var id = Guid.NewGuid();
        var query = new GetSupplierByIdQuery(id.ToString());

        _repoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier?)null);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Supplier {id} not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedDto_WhenSupplierExistsWithFullData()
    {
        var id = Guid.NewGuid();
        var query = new GetSupplierByIdQuery(id.ToString());
        var createdAt = DateTimeOffset.UtcNow.AddMonths(-2);
        var updatedAt = DateTimeOffset.UtcNow;

        var supplier = new Supplier
        {
            Id = id,
            Code = "NCC-CF-888",
            Name = "Xưởng Rang Xay Cà Phê Mộc",
            ContactPerson = "Chú Bảy",
            Phone = "0912345678",
            Email = "chubay@cafemoc.vn",
            Address = "Lâm Hà, Lâm Đồng",
            TaxCode = "0304050607",
            IsActive = true,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        _repoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(id.ToString());
        result.Code.Should().Be("NCC-CF-888");
        result.Name.Should().Be("Xưởng Rang Xay Cà Phê Mộc");
        result.ContactPerson.Should().Be("Chú Bảy");
        result.Phone.Should().Be("0912345678");
        result.Email.Should().Be("chubay@cafemoc.vn");
        result.Address.Should().Be("Lâm Hà, Lâm Đồng");
        result.TaxCode.Should().Be("0304050607");
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().Be(createdAt.ToString("o"));
        result.UpdatedAt.Should().Be(updatedAt.ToString("o"));
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedDto_WithFallbackStrings_WhenOptionalFieldsAreNull()
    {
        var id = Guid.NewGuid();
        var query = new GetSupplierByIdQuery(id.ToString());

        var supplier = new Supplier
        {
            Id = id,
            Code = "NCC-TEMP-001",
            Name = "Đại lý Đường, Sữa lẻ",
            ContactPerson = null,
            Phone = null,
            Email = null,
            Address = null,
            TaxCode = null,
            IsActive = false
        };

        _repoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ContactPerson.Should().Be("");
        result.Phone.Should().Be("");
        result.Email.Should().Be("");
        result.Address.Should().Be("");
        result.TaxCode.Should().Be("");
        result.IsActive.Should().BeFalse();
    }
}