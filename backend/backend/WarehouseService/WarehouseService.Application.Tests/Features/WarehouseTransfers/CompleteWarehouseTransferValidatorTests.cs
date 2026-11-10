namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.WarehouseTransfers.Command.CompleteWarehouseTransfer;

public class CompleteWarehouseTransferValidatorTests
{
    private readonly CompleteWarehouseTransferCommandValidator _validator;

    public CompleteWarehouseTransferValidatorTests()
    {
        _validator = new CompleteWarehouseTransferCommandValidator();
    }

    [Fact]
    public void Should_HaveError_WhenIdIsEmpty()
    {
        var command = CreateCommand(id: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Warehouse transfer ID must not be empty.");
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
        // Kịch bản hợp lệ: Xác nhận hoàn thành lệnh điều phối kho
        var validCommand = new CompleteWarehouseTransferCommand(
            Id: Guid.NewGuid(),
            CompletedBy: Guid.NewGuid()
        );

        var result = _validator.TestValidate(validCommand);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper method gom tham số, giữ cho các file test luôn đồng bộ và sạch đẹp
    private static CompleteWarehouseTransferCommand CreateCommand(
        Guid? id = null,
        Guid? completedBy = null)
    {
        return new CompleteWarehouseTransferCommand(
            Id: id ?? Guid.NewGuid(),
            CompletedBy: completedBy ?? Guid.NewGuid()
        );
    }
}