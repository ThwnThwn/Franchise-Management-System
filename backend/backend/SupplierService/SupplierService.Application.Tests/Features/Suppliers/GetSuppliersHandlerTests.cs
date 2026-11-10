using FluentAssertions;
using MockQueryable;
using Moq;
using SupplierService.Application.Features.Suppliers.Queries.GetSuppliers;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;

namespace SupplierService.Application.Tests.Features.Suppliers;

public class GetSuppliersHandlerTests
{
    private readonly Mock<ISupplierRepository> _repoMock;
    private readonly GetSuppliersHandler _handler;

    public GetSuppliersHandlerTests()
    {
        _repoMock = new Mock<ISupplierRepository>();
        _handler = new GetSuppliersHandler(_repoMock.Object);
    }

    private void SetupMockData(List<Supplier> data)
    {
        var mockQueryable = data.BuildMock();
        _repoMock.Setup(repo => repo.GetQueryable()).Returns(mockQueryable);
    }

    private static List<Supplier> GetTestSuppliers()
    {
        return
        [
            new Supplier
            {
                Id = Guid.NewGuid(),
                Code = "NCC-CF-001",
                Name = "Cà phê Arabica Cầu Đất",
                ContactPerson = "Anh Hùng",
                IsActive = true,
                IsDeleted = false
            },
            new Supplier
            {
                Id = Guid.NewGuid(),
                Code = "NCC-CF-002",
                Name = "Cà phê Robusta Buôn Ma Thuột",
                IsActive = true,
                IsDeleted = false
            },
            new Supplier
            {
                Id = Guid.NewGuid(),
                Code = "NCC-ML-001",
                Name = "Sữa đặc Ngôi Sao Phương Nam",
                Phone = "0909123456",
                IsActive = true,
                IsDeleted = false
            },
            new Supplier
            {
                Id = Guid.NewGuid(),
                Code = "NCC-SG-001",
                Name = "Đường cát Biên Hòa",
                IsActive = false,
                IsDeleted = false
            },
            new Supplier
            {
                Id = Guid.NewGuid(),
                Code = "NCC-OLD-999",
                Name = "Nhà cung cấp đã nghỉ bán",
                IsActive = true,
                IsDeleted = true
            }
        ];
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyNonDeletedSuppliers_WithDefaultPagination()
    {
        var data = GetTestSuppliers();
        SetupMockData(data);
        var query = new GetSuppliersQuery { Page = 0, PageSize = 0 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(4);
        result.Suppliers.Should().HaveCount(4);
        result.Suppliers.Should().NotContain(s => s.Code == "NCC-OLD-999");
    }

    [Fact]
    public async Task Handle_ShouldFilterByCode_CaseInsensitive()
    {
        var data = GetTestSuppliers();
        SetupMockData(data);
        var query = new GetSuppliersQuery { Code = "cf" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Suppliers.All(s => s.Code.Contains("CF")).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFilterByName_CaseInsensitive()
    {
        var data = GetTestSuppliers();
        SetupMockData(data);
        var query = new GetSuppliersQuery { Name = "SỮA" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Suppliers.First().Name.Should().Be("Sữa đặc Ngôi Sao Phương Nam");
    }

    [Fact]
    public async Task Handle_ShouldFilterByIsActive()
    {
        var data = GetTestSuppliers();
        SetupMockData(data);
        var query = new GetSuppliersQuery { IsActive = false };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Suppliers.First().Name.Should().Be("Đường cát Biên Hòa");
    }

    [Fact]
    public async Task Handle_ShouldApplyPaginationCorrectly()
    {
        var data = GetTestSuppliers();
        SetupMockData(data);
        // Thứ tự OrderBy Name: Cà phê Arabica, Cà phê Robusta, Đường cát, Sữa đặc
        var query = new GetSuppliersQuery { Page = 2, PageSize = 2 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(4);
        result.Suppliers.Should().HaveCount(2);
        result.Suppliers.ElementAt(0).Name.Should().Be("Đường cát Biên Hòa");
        result.Suppliers.ElementAt(1).Name.Should().Be("Sữa đặc Ngôi Sao Phương Nam");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ShouldIgnoreEmptyFilters(string? emptyInput)
    {
        var data = GetTestSuppliers();
        SetupMockData(data);
        var query = new GetSuppliersQuery { Name = emptyInput, Code = emptyInput };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(4);
        result.Suppliers.Should().HaveCount(4);
    }

    [Fact]
    public async Task Handle_ShouldMapFieldsCorrectly()
    {
        var supplierId = Guid.NewGuid();
        var data = new List<Supplier>
        {
            new()
            {
                Id = supplierId,
                Code = "NCC-TEST",
                Name = "Nhà cung cấp Test",
                ContactPerson = "Người liên hệ",
                Phone = "0123456789",
                IsActive = true,
                IsDeleted = false
            }
        };
        SetupMockData(data);
        var query = new GetSuppliersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        var dto = result.Suppliers.First();
        dto.Id.Should().Be(supplierId.ToString());
        dto.Code.Should().Be("NCC-TEST");
        dto.Name.Should().Be("Nhà cung cấp Test");
        dto.ContactPerson.Should().Be("Người liên hệ");
        dto.Phone.Should().Be("0123456789");
        dto.IsActive.Should().BeTrue();
    }
}