namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.WarehouseTransfers.Command.CancelWarehouseTransfer;

public class CancelWarehouseTransferValidatorTests
{
    private readonly CancelWarehouseTransferCommandValidator _validator;

    public CancelWarehouseTransferValidatorTests()
    {
        _validator = new CancelWarehouseTransferCommandValidator();
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
    public void Should_NotHaveAnyErrors_WhenCommandIsValid()
    {
        // Kịch bản hợp lệ: Hủy lệnh điều phối kho do sai kế hoạch
        var validCommand = CreateCommand(id: Guid.NewGuid());

        var result = _validator.TestValidate(validCommand);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper method gom tham số, giúp file test dễ bảo trì khi Record gốc thay đổi
    private static CancelWarehouseTransferCommand CreateCommand(
        Guid? id = null,
        string? reason = "Thay đổi kế hoạch điều phối",
        Guid? cancelledBy = null)
    {
        return new CancelWarehouseTransferCommand(
            id ?? Guid.NewGuid(),
            reason,
            cancelledBy ?? Guid.NewGuid()
        );
    }
}