using Moq;
using Xunit;
using FluentAssertions;
using EngagementService.Application.Features.Voucher.Commands.Create;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Application.Constants;
using EngagementService.Domain.Constants;
using EngagementService.Application.Dtos.Voucher;
using EngagementService.Domain.Entities;

namespace EngagementService.Application.Tests.Features.Voucher;

public class CreateVoucherHandlerTests
{
    private readonly Mock<IVoucherRepository> _repositoryMock;
    private readonly CreateVoucherHandler _handler;

    public CreateVoucherHandlerTests()
    {
        _repositoryMock = new Mock<IVoucherRepository>();
        _handler = new CreateVoucherHandler(_repositoryMock.Object);

        // Mặc định cho Repository trả về null (nghĩa là code chưa tồn tại)
        _repositoryMock.Setup(x => x.GetVoucherByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((Domain.Entities.Voucher?)null);
    }

    // --- NHÓM 1: KIỂM TRA VALIDATION CƠ BẢN ---

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCodeAlreadyExists()
    {
        // Arrange
        var request = new CreateVoucherRequest { Code = "EXISTED" };
        var command = new CreateVoucherCommand(request);

        _repositoryMock.Setup(x => x.GetVoucherByCodeAsync("EXISTED"))
            .ReturnsAsync(new Domain.Entities.Voucher()); // Giả lập đã tồn tại

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(VoucherMsg.CodeExisted);
    }

    [Theory]
    [InlineData(-1, 1, VoucherMsg.InvalidInitQuantity)]
    [InlineData(10, 0, VoucherMsg.InvalidMaxUsagePerUser)]
    public async Task Handle_ShouldThrowException_WhenBasicValidationFails(int initQty, int maxUsage, string expectedMsg)
    {
        // Arrange
        var request = new CreateVoucherRequest
        {
            InitQuantity = initQty,
            MaxUsagePerUser = maxUsage,
            Type = VoucherType.DISCOUNT_FIXED_AMOUNT,
            DiscountValue = 1000 // Thêm vào để ko bị lỗi DiscountValue
        };
        var command = new CreateVoucherCommand(request);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(expectedMsg);
    }

    // --- NHÓM 2: KIỂM TRA GIÁ TRỊ CHIẾT KHẤU (FIXED TYPE ERROR) ---

    [Theory]
    // CỰC KỲ QUAN TRỌNG: Thêm .0 để xUnit hiểu là kiểu double chứ không phải int
    [InlineData(VoucherType.DISCOUNT_PERCENTAGE, 101.0)]
    [InlineData(VoucherType.SHIPPING_PERCENTAGE, 0.0)]
    [InlineData(VoucherType.DISCOUNT_FIXED_AMOUNT, null)]
    public async Task Handle_ShouldThrowException_WhenDiscountValueIsInvalid(VoucherType type, double? value)
    {
        // Arrange
        var request = new CreateVoucherRequest
        {
            Type = type,
            // Ép kiểu từ double sang decimal để khớp với DTO của Duy
            DiscountValue = value.HasValue ? (decimal)value.Value : null,
            InitQuantity = 1,
            MaxUsagePerUser = 1
        };
        var command = new CreateVoucherCommand(request);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(VoucherMsg.InvalidDiscountValue);
    }

    // --- NHÓM 3: KIỂM TRA GIFT VÀ NGÀY THÁNG ---

    [Fact]
    public async Task Handle_ShouldThrowException_WhenGiftVoucherHasNoItems()
    {
        // Arrange
        var request = new CreateVoucherRequest
        {
            Type = VoucherType.GIFT,
            VoucherItems = new List<CreateVoucherItemReq>(),
            InitQuantity = 1,
            MaxUsagePerUser = 1
        };
        var command = new CreateVoucherCommand(request);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(VoucherMsg.EmptyVoucherItems);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEndDateIsInThePast()
    {
        // Arrange
        var request = new CreateVoucherRequest
        {
            Type = VoucherType.DISCOUNT_FIXED_AMOUNT,
            DiscountValue = 1000,
            InitQuantity = 1,
            MaxUsagePerUser = 1,
            EndDate = DateTimeOffset.UtcNow.AddMinutes(-10)
        };
        var command = new CreateVoucherCommand(request);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, default))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(VoucherMsg.InvalidEndDate);
    }

    // --- NHÓM 4: LOGIC THÀNH CÔNG ---

    [Fact]
    public async Task Handle_ShouldGenerateCode_WhenRequestCodeIsEmpty()
    {
        // Arrange
        var request = new CreateVoucherRequest
        {
            Code = "",
            Type = VoucherType.DISCOUNT_FIXED_AMOUNT,
            DiscountValue = 5000,
            InitQuantity = 10,
            MaxUsagePerUser = 1,
            EndDate = DateTimeOffset.UtcNow.AddDays(1)
        };
        var command = new CreateVoucherCommand(request);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Code.Should().NotBeNullOrWhiteSpace();
        result.Code.Length.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldCreateGiftVoucherWithItems_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateVoucherRequest
        {
            Code = "GIFT2026",
            Type = VoucherType.GIFT,
            InitQuantity = 50,
            MaxUsagePerUser = 1,
            EndDate = DateTimeOffset.UtcNow.AddMonths(1),
            VoucherItems = new List<CreateVoucherItemReq>
            {
                new() { Sku = "COFFEE_MUG", Quantity = 1, Type = VoucherItemType.ITEM_GIFT }
            }
        };
        var command = new CreateVoucherCommand(request);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.VoucherItems.Should().NotBeNull();
        result.VoucherItems.Should().HaveCount(1);

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Voucher>()), Times.Once);
    }
}