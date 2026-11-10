namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.WarehouseTransfers.Command.RejectWarehouseTransfer;

public class RejectWarehouseTransferValidatorTests
{
    private readonly RejectWarehouseTransferCommandValidator _validator;

    public RejectWarehouseTransferValidatorTests()
    {
        _validator = new RejectWarehouseTransferCommandValidator();
    }

    [Fact]
    public void Should_HaveError_WhenIdIsEmpty()
    {
        var command = CreateCommand(id: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Warehouse transfer ID must not be empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_HaveError_WhenReasonIsEmpty(string? invalidReason)
    {
        // Phủ sạch các kịch bản chuỗi rỗng, null hoặc chỉ toàn dấu cách
        var command = CreateCommand(reason: invalidReason!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Rejection reason must not be empty.");
    }

    [Fact]
    public void Should_HaveError_WhenReasonExceeds500Characters()
    {
        // Tạo một chuỗi dài 501 ký tự để phá vỡ rule MaximumLength
        var longReason = new string('R', 501);
        var command = CreateCommand(reason: longReason);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Rejection reason must not exceed 500 characters.");
    }

    [Fact]
    public void Should_NotHaveAnyErrors_WhenCommandIsValid()
    {
        // Kịch bản hợp lệ: Từ chối lô hàng vì chất lượng không đạt chuẩn
        var validCommand = new RejectWarehouseTransferCommand(
            Id: Guid.NewGuid(),
            Reason: "Chất lượng hạt cà phê không đạt chuẩn, tỷ lệ hạt vỡ vụn vượt quá 5%.",
            RejectedBy: Guid.NewGuid()
        );

        var result = _validator.TestValidate(validCommand);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper method gom tham số, giúp các Test Case ngắn gọn, dễ đọc và dễ bảo trì
    private static RejectWarehouseTransferCommand CreateCommand(
        Guid? id = null,
        string reason = "Sai số lượng so với phiếu xuất",
        Guid? rejectedBy = null)
    {
        return new RejectWarehouseTransferCommand(
            Id: id ?? Guid.NewGuid(),
            Reason: reason,
            RejectedBy: rejectedBy ?? Guid.NewGuid()
        );
    }
}