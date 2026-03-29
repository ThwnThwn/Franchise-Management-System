namespace WarehouseService.Application.Tests.Features.WarehouseItems;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.WarehouseItems.Command.DeleteWarehouseItem;

public class DeleteWarehouseItemValidatorTests
{
    private readonly DeleteWarehouseItemCommandValidator _validator;

    public DeleteWarehouseItemValidatorTests()
    {
        _validator = new DeleteWarehouseItemCommandValidator();
    }

    [Fact]
    public void Should_HaveError_WhenIdIsEmpty()
    {
        var command = CreateCommand(id: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Warehouse item ID must not be empty.");
    }

    [Fact]
    public void Should_NotHaveAnyErrors_WhenCommandIsValid()
    {
        // Kịch bản hợp lệ: Xóa một món hàng khỏi kho (ví dụ: nguyên liệu đã ngừng kinh doanh)
        var validCommand = CreateCommand(id: Guid.NewGuid());

        var result = _validator.TestValidate(validCommand);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper method: Giúp test case sạch sẽ và dễ bảo trì nếu Record gốc thêm field mới
    private static DeleteWarehouseItemCommand CreateCommand(Guid? id = null)
    {
        // Lưu ý: Nếu Record DeleteWarehouseItemCommand của bro có thêm field (như DeletedBy),
        // bro chỉ cần bổ sung vào Helper này là xong.
        return new DeleteWarehouseItemCommand(
            Id: id ?? Guid.NewGuid()
        );
    }
}