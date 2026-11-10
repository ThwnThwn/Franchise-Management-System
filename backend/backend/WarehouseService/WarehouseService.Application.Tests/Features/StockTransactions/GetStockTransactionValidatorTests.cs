namespace WarehouseService.Application.Tests.Features.StockTransactions;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.StockTransactions.Query.GetById;

public class GetStockTransactionValidatorTests
{
    private readonly GetStockTransactionQueryValidator _validator;

    public GetStockTransactionValidatorTests()
    {
        _validator = new GetStockTransactionQueryValidator();
    }

    [Fact]
    public void Should_HaveError_WhenIdIsEmpty()
    {
        var query = CreateQuery(id: Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Stock transaction ID must not be empty.");
    }

    [Fact]
    public void Should_NotHaveAnyErrors_WhenQueryIsValid()
    {
        // Kịch bản hợp lệ: Lấy thông tin chi tiết của một giao dịch kho
        var validQuery = CreateQuery(id: Guid.NewGuid());

        var result = _validator.TestValidate(validQuery);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper method gom tham số: Giúp file test gọn gàng và dễ bảo trì
    private static GetStockTransactionQuery CreateQuery(Guid? id = null)
    {
        // Nếu Record GetStockTransactionQuery của bro có thêm field,
        // chỉ cần bổ sung vào Helper này là xong.
        return new GetStockTransactionQuery(
            Id: id ?? Guid.NewGuid()
        );
    }
}