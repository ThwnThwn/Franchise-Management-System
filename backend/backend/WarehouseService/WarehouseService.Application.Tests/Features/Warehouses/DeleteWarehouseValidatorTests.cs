namespace WarehouseService.Application.Tests.Features.Warehouses;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.Warehouses.Command.DeleteWarehouse;

public class DeleteWarehouseValidatorTests
{
    private readonly DeleteWarehouseCommandValidator _validator;

    public DeleteWarehouseValidatorTests()
    {
        _validator = new DeleteWarehouseCommandValidator();
    }

    [Fact]
    public void Should_HaveError_WhenIdIsEmpty()
    {
        var command = CreateCommand(id: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Warehouse ID must not be empty.");
    }

    [Fact]
    public void Should_NotHaveAnyErrors_WhenCommandIsValid()
    {
        // Kịch bản hợp lệ: Xóa một kho bãi không còn sử dụng
        var validCommand = CreateCommand(id: Guid.NewGuid());

        var result = _validator.TestValidate(validCommand);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper method gom tham số, giúp file test dễ bảo trì khi Record gốc thay đổi
    private static DeleteWarehouseCommand CreateCommand(Guid? id = null)
    {
        // Lưu ý: Nếu Record DeleteWarehouseCommand của bro có thêm các tham số khác
        // (VD: string Reason, Guid DeletedBy), bro cứ ném giá trị mặc định vào đây nhé!
        return new DeleteWarehouseCommand(
            Id: id ?? Guid.NewGuid()
        );
    }
}