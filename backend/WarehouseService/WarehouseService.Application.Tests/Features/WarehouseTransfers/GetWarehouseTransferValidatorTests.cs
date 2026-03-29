namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.WarehouseTransfers.Query.GetById;

public class GetWarehouseTransferValidatorTests
{
    private readonly GetWarehouseTransferQueryValidator _validator;

    public GetWarehouseTransferValidatorTests()
    {
        _validator = new GetWarehouseTransferQueryValidator();
    }

    [Fact]
    public void Should_HaveError_WhenIdIsEmpty()
    {
        var query = CreateQuery(id: Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Warehouse transfer ID must not be empty.");
    }

    [Fact]
    public void Should_NotHaveAnyErrors_WhenQueryIsValid()
    {
        // Kịch bản hợp lệ: Truy vấn chi tiết một lệnh điều phối kho
        var validQuery = CreateQuery(id: Guid.NewGuid());

        var result = _validator.TestValidate(validQuery);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper method: Giúp test case gọn gàng và dễ dàng bảo trì nếu Query Record thêm field
    private static GetWarehouseTransferQuery CreateQuery(Guid? id = null)
    {
        return new GetWarehouseTransferQuery(
            Id: id ?? Guid.NewGuid()
        );
    }
}