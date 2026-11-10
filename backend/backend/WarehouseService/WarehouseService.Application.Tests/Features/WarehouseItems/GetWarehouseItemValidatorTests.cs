namespace WarehouseService.Application.Tests.Features.WarehouseItems;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.WarehouseItems.Query.GetById;

public class GetWarehouseItemValidatorTests
{
    private readonly GetWarehouseItemQueryValidator _validator;

    public GetWarehouseItemValidatorTests()
    {
        _validator = new GetWarehouseItemQueryValidator();
    }

    [Fact]
    public void Should_HaveError_WhenIdIsEmpty()
    {
        var query = CreateQuery(id: Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Warehouse item ID must not be empty.");
    }

    [Fact]
    public void Should_NotHaveAnyErrors_WhenQueryIsValid()
    {
        // Kịch bản hợp lệ: Lấy thông tin chi tiết của một món hàng cụ thể trong kho
        var validQuery = CreateQuery(id: Guid.NewGuid());

        var result = _validator.TestValidate(validQuery);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper method: Giúp test case gọn gàng và dễ dàng cập nhật nếu Query Record thêm field
    private static GetWarehouseItemQuery CreateQuery(Guid? id = null)
    {
        return new GetWarehouseItemQuery(
            Id: id ?? Guid.NewGuid()
        );
    }
}