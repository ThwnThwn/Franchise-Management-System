namespace WarehouseService.Application.Tests.Features.WarehouseTransfers;

using FluentValidation.TestHelper;
using WarehouseService.Application.Features.WarehouseTransfers.Command.CreateWarehouseTransfer;

public class CreateWarehouseTransferValidatorTests
{
    private readonly CreateWarehouseTransferCommandValidator _validator;

    public CreateWarehouseTransferValidatorTests()
    {
        _validator = new CreateWarehouseTransferCommandValidator();
    }

    [Fact]
    public void Should_HaveError_WhenFromWarehouseIdIsEmpty()
    {
        var command = CreateCommand(fromWarehouseId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FromWarehouseId)
            .WithErrorMessage("Source warehouse ID must not be empty.");
    }

    [Fact]
    public void Should_HaveError_WhenToWarehouseIdIsEmpty()
    {
        var command = CreateCommand(toWarehouseId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ToWarehouseId)
            .WithErrorMessage("Destination warehouse ID must not be empty.");
    }

    [Fact]
    public void Should_HaveError_WhenToWarehouseIdEqualsFromWarehouseId()
    {
        var sameId = Guid.NewGuid();
        var command = CreateCommand(fromWarehouseId: sameId, toWarehouseId: sameId);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ToWarehouseId)
            .WithErrorMessage("Source and destination warehouses must be different.");
    }

    [Fact]
    public void Should_HaveError_WhenItemsAreEmpty()
    {
        var command = CreateCommand(items: []);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("Transfer must contain at least one item.");
    }

    [Fact]
    public void Should_HaveError_WhenItemsExceed100()
    {
        var tooManyItems = Enumerable.Range(1, 101)
            .Select(_ => new TransferItemDto(Guid.NewGuid(), 10))
            .ToList();

        var command = CreateCommand(items: tooManyItems);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("Transfer cannot contain more than 100 items.");
    }

    [Fact]
    public void Should_HaveError_WhenItemWarehouseItemIdIsEmpty()
    {
        var invalidItem = new TransferItemDto(Guid.Empty, 50);
        var command = CreateCommand(items: [invalidItem]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].WarehouseItemId")
            .WithErrorMessage("Warehouse item ID must not be empty.");
    }

    [Fact]
    public void Should_HaveError_WhenItemQuantityIsZeroOrLess()
    {
        var zeroQuantityItem = new TransferItemDto(Guid.NewGuid(), 0);
        var negativeQuantityItem = new TransferItemDto(Guid.NewGuid(), -5);

        var commandZero = CreateCommand(items: [zeroQuantityItem]);
        var commandNegative = CreateCommand(items: [negativeQuantityItem]);

        _validator.TestValidate(commandZero)
            .ShouldHaveValidationErrorFor("Items[0].Quantity")
            .WithErrorMessage("Transfer quantity must be greater than 0.");

        _validator.TestValidate(commandNegative)
            .ShouldHaveValidationErrorFor("Items[0].Quantity")
            .WithErrorMessage("Transfer quantity must be greater than 0.");
    }

    [Fact]
    public void Should_NotHaveError_WhenCommandIsValid()
    {
        // Kịch bản hợp lệ: Điều phối 50kg hạt Arabica Cầu Đất
        var validItem = new TransferItemDto(Guid.NewGuid(), 50);
        var command = CreateCommand(items: [validItem]);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Hàm Helper giúp khởi tạo Command nhanh gọn cho các test case
    private static CreateWarehouseTransferCommand CreateCommand(
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        IReadOnlyList<TransferItemDto>? items = null)
    {
        var fromId = fromWarehouseId ?? Guid.NewGuid();
        var toId = toWarehouseId ?? Guid.NewGuid();

        // Đảm bảo default fromId và toId luôn khác nhau để không vướng rule NotEqual
        if (fromId == toId && toWarehouseId == null)
        {
            toId = Guid.NewGuid();
        }

        return new CreateWarehouseTransferCommand(
            FromWarehouseId: fromId,
            ToWarehouseId: toId,
            Items: items ?? [new TransferItemDto(Guid.NewGuid(), 10)],
            Reason: "Điều phối nguyên liệu pha chế cuối tháng",
            Note: "Giao bằng xe tải chuyên dụng",
            CreatedBy: Guid.NewGuid()
        );
    }
}