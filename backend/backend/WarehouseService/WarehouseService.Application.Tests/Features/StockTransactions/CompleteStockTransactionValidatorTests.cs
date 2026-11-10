namespace WarehouseService.Application.Tests.Features.StockTransactions;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.StockTransactions.Command.CompleteStockTransaction;

public class CompleteStockTransactionValidatorTests
{
    private readonly CompleteStockTransactionCommandValidator _validator;

    public CompleteStockTransactionValidatorTests()
    {
        _validator = new CompleteStockTransactionCommandValidator();
    }

    [Fact]
    public void Should_HaveError_WhenTransactionIdsAreEmpty()
    {
        // Khởi tạo an toàn mảng rỗng []
        var command = CreateCommand(transactionIds: []);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TransactionIds)
            .WithErrorMessage("Transaction IDs must not be empty.");
    }

    [Fact]
    public void Should_HaveError_WhenTransactionIdsExceed100()
    {
        // Tạo 101 ID để test phá vỡ giới hạn
        var tooManyIds = Enumerable.Range(1, 101).Select(_ => Guid.NewGuid()).ToList();
        var command = CreateCommand(transactionIds: tooManyIds);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TransactionIds)
            .WithErrorMessage("Cannot complete more than 100 transactions at once.");
    }

    [Fact]
    public void Should_HaveError_WhenAnyTransactionIdIsEmpty()
    {
        // Cố tình nhét 1 cái Guid.Empty vào mảng
        var command = CreateCommand(transactionIds: [Guid.Empty]);

        var result = _validator.TestValidate(command);

        // Với RuleForEach cho tập hợp kiểu dữ liệu nguyên thủy (Guid), FV sẽ map lỗi kèm index
        result.ShouldHaveValidationErrorFor("TransactionIds[0]")
            .WithErrorMessage("Transaction ID must not be empty.");
    }

    [Fact]
    public void Should_HaveError_WhenCompletedByIsEmpty()
    {
        var command = CreateCommand(completedBy: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CompletedBy)
            .WithErrorMessage("CompletedBy must not be empty.");
    }

    [Fact]
    public void Should_NotHaveAnyErrors_WhenCommandIsValid()
    {
        // Kịch bản hợp lệ: Hoàn thành 2 lệnh xuất/nhập kho cùng lúc
        var validCommand = new CompleteStockTransactionCommand(
            TransactionIds: [Guid.NewGuid(), Guid.NewGuid()],
            CompletedBy: Guid.NewGuid()
        );

        var result = _validator.TestValidate(validCommand);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper method gom tham số, giúp các Test Case ngắn gọn và dễ bảo trì
    private static CompleteStockTransactionCommand CreateCommand(
        List<Guid>? transactionIds = null,
        Guid? completedBy = null)
    {
        return new CompleteStockTransactionCommand(
            TransactionIds: transactionIds ?? [Guid.NewGuid(), Guid.NewGuid()],
            CompletedBy: completedBy ?? Guid.NewGuid()
        );
    }
}