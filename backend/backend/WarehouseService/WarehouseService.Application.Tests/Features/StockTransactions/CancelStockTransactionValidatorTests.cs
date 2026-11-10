namespace WarehouseService.Application.Tests.Features.StockTransactions;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.StockTransactions.Command.CancelStockTransaction;

public class CancelStockTransactionValidatorTests
{
    private readonly CancelStockTransactionCommandValidator _validator;

    public CancelStockTransactionValidatorTests()
    {
        _validator = new CancelStockTransactionCommandValidator();
    }

    [Fact]
    public void Should_HaveError_WhenIdIsEmpty()
    {
        var command = CreateCommand(id: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Stock transaction ID must not be empty.");
    }

    [Fact]
    public void Should_NotHaveAnyErrors_WhenCommandIsValid()
    {
        // Kịch bản hợp lệ: Hủy một giao dịch xuất/nhập kho
        var validCommand = CreateCommand(id: Guid.NewGuid());

        var result = _validator.TestValidate(validCommand);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper method: Luôn giữ lại để lỡ sau này có thêm field (như Reason hay CancelledBy)
    // thì chỉ cần sửa ở đây thay vì đi sửa từng Test Case.
    private static CancelStockTransactionCommand CreateCommand(
        Guid? id = null)
    {
        // Lưu ý: Nếu Record CancelStockTransactionCommand của bro có thêm các tham số khác
        // (VD: string Reason, Guid CancelledBy), bro cứ thêm giá trị mặc định vào đây nhé!
        return new CancelStockTransactionCommand(
            Id: id ?? Guid.NewGuid(),
            UpdatedBy: Guid.NewGuid()
        );
    }
}