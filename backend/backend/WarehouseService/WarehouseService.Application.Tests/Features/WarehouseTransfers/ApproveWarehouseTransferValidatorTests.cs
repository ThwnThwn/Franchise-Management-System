namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.WarehouseTransfers.Command.ApproveWarehouseTransfer;

public class ApproveWarehouseTransferValidatorTests
{
    private readonly ApproveWarehouseTransferCommandValidator _validator;

    public ApproveWarehouseTransferValidatorTests()
    {
        _validator = new ApproveWarehouseTransferCommandValidator();
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
        // Kịch bản hợp lệ: Phê duyệt lệnh điều phối kho
        var validCommand = CreateCommand(id: Guid.NewGuid());

        var result = _validator.TestValidate(validCommand);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper method: Giữ cho file test luôn sạch sẽ và dễ mở rộng
    private static ApproveWarehouseTransferCommand CreateCommand(Guid? id = null)
    {
        // Note: Tôi đang giả định record của bro có tham số Id (và có thể là ApprovedBy).
        // Nếu Record có yêu cầu thêm tham số bắt buộc nào khác (CS7036),
        // bro chỉ cần bổ sung giá trị mặc định vào chỗ này là các test case tự động xanh hết!
        return new ApproveWarehouseTransferCommand(
            Id: id ?? Guid.NewGuid(),
            ApprovedBy: Guid.NewGuid()
        );
    }
}