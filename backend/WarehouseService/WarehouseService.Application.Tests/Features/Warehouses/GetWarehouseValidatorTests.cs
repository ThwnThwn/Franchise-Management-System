namespace WarehouseService.Application.Tests.Features.Warehouses;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.Warehouses.Queries.GetById;

public class GetWarehouseValidatorTests
{
    private readonly GetWarehouseQueryValidator _validator;

    public GetWarehouseValidatorTests()
    {
        _validator = new GetWarehouseQueryValidator();
    }

    [Fact]
    public void Should_HaveError_WhenIdIsEmpty()
    {
        var query = CreateQuery(id: Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Warehouse ID must not be empty.");
    }

    [Fact]
    public void Should_NotHaveAnyErrors_WhenQueryIsValid()
    {
        // Kịch bản hợp lệ: Lấy thông tin chi tiết của một kho bãi cụ thể
        var validQuery = CreateQuery(id: Guid.NewGuid());

        var result = _validator.TestValidate(validQuery);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper method: Giúp test case gọn gàng và dễ dàng bảo trì nếu Query Record thêm field
    private static GetWarehouseQuery CreateQuery(Guid? id = null)
    {
        // Giả định GetWarehouseQuery nhận Id làm tham số duy nhất
        return new GetWarehouseQuery(
            Id: id ?? Guid.NewGuid()
        );
    }
}