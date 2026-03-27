using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.Voucher;
using EngagementService.Application.Features.Voucher.Commands.Create;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
using FluentAssertions;
using Moq;

namespace EngagementService.Application.Tests.Features.VoucherTests;

public class CreateVoucherHandlerTests
{
    private readonly Mock<IVoucherRepository> _voucherRepoMock;
    private readonly CreateVoucherHandler _handler;

    public CreateVoucherHandlerTests()
    {
        _voucherRepoMock = new Mock<IVoucherRepository>();
        _handler = new CreateVoucherHandler(_voucherRepoMock.Object);
    }

    // 1. Test Validation: Code đã tồn tại (Dòng 25)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenCodeAlreadyExists()
    {
        var request = new CreateVoucherRequest { Code = "EXISTING", Type = VoucherType.DISCOUNT_FIXED_AMOUNT };
        _voucherRepoMock.Setup(x => x.GetVoucherByCodeAsync(request.Code))
            .ReturnsAsync(new Domain.Entities.Voucher());

        var act = () => _handler.Handle(new CreateVoucherCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage(VoucherMsg.CodeExisted);
    }

    // 2. Test Auto Generate Code: Code trống (Dòng 29)
    [Fact]
    public async Task Handle_ShouldGenerateCode_WhenCodeIsWhiteSpace()
    {
        var request = new CreateVoucherRequest
        {
            Code = "",
            Type = VoucherType.DISCOUNT_FIXED_AMOUNT,
            InitQuantity = 10,
            MaxUsagePerUser = 1,
            DiscountValue = 1000,
            EndDate = DateTimeOffset.UtcNow.AddDays(1)
        };

        var result = await _handler.Handle(new CreateVoucherCommand(request), CancellationToken.None);

        result.Code.Should().NotBeNullOrWhiteSpace();
        result.Code.Length.Should().Be(10);
    }

    // 3. Test Validation: Số lượng không hợp lệ (Dòng 31-35)
    [Theory]
    [InlineData(-1, 1, "InvalidInitQuantity")]
    [InlineData(10, 0, "InvalidMaxUsagePerUser")]
    public async Task Handle_ShouldThrowException_WhenQuantitiesAreInvalid(int initQty, int maxUsage, string expectedMsgKey)
    {
        var request = new CreateVoucherRequest { Code = "NEW", InitQuantity = initQty, MaxUsagePerUser = maxUsage };
        var expectedMsg = expectedMsgKey == "InvalidInitQuantity" ? VoucherMsg.InvalidInitQuantity : VoucherMsg.InvalidMaxUsagePerUser;

        var act = () => _handler.Handle(new CreateVoucherCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage(expectedMsg);
    }

    // 4. Test Switch Case: DISCOUNT_PERCENTAGE hợp lệ và không hợp lệ (Dòng 46-47)
    [Theory]
    [InlineData(101)] // Quá 100%
    [InlineData(0)]   // Bằng 0
    [InlineData(-1)]  // Nhỏ hơn 0
    public async Task Handle_ShouldThrowException_WhenDiscountPercentageIsInvalid(decimal val)
    {
        var request = new CreateVoucherRequest
        {
            Type = VoucherType.DISCOUNT_PERCENTAGE,
            DiscountValue = val,
            InitQuantity = 10,
            MaxUsagePerUser = 1,
            EndDate = DateTimeOffset.UtcNow.AddDays(1)
        };

        var act = () => _handler.Handle(new CreateVoucherCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage(VoucherMsg.InvalidDiscountValue);
    }

    // 5. Test Switch Case: GIFT Voucher - Thiếu Item hoặc Item Quantity <= 0 (Dòng 55-60)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenGiftVoucherHasNoItems()
    {
        var request = new CreateVoucherRequest
        {
            Type = VoucherType.GIFT,
            VoucherItems = new List<CreateVoucherItemReq>(),
            InitQuantity = 10,
            MaxUsagePerUser = 1,
            EndDate = DateTimeOffset.UtcNow.AddDays(1)
        };

        var act = () => _handler.Handle(new CreateVoucherCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage(VoucherMsg.EmptyVoucherItems);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenGiftVoucherItemQuantityIsInvalid()
    {
        var request = new CreateVoucherRequest
        {
            Type = VoucherType.GIFT,
            EndDate = DateTimeOffset.UtcNow.AddDays(1),
            VoucherItems = new List<CreateVoucherItemReq> {
                new CreateVoucherItemReq { Type = VoucherItemType.ITEM_GIFT, Quantity = 0 }
            }
        };

        var act = () => _handler.Handle(new CreateVoucherCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage(VoucherMsg.InvalidVoucherItemQuantity);
    }

    // 6. Test Happy Path: GIFT Voucher thành công (Dòng 63-78)
    [Fact]
    public async Task Handle_ShouldCreateGiftVoucher_WhenRequestIsValid()
    {
        var request = new CreateVoucherRequest
        {
            Type = VoucherType.GIFT,
            InitQuantity = 10,
            MaxUsagePerUser = 1,
            EndDate = DateTimeOffset.UtcNow.AddDays(1),
            VoucherItems = new List<CreateVoucherItemReq> {
                new CreateVoucherItemReq { Sku = "GIFT_01", Quantity = 1, Type = VoucherItemType.ITEM_GIFT }
            }
        };

        var result = await _handler.Handle(new CreateVoucherCommand(request), CancellationToken.None);

        result.Type.Should().Be(VoucherType.GIFT);
        result.VoucherItems.Should().HaveCount(1);
        _voucherRepoMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Voucher>()), Times.Once);
    }

    // 7. Test Private Method: CreateSimpleVoucher - EndDate quá khứ (Dòng 90-91)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenEndDateIsInPast()
    {
        var request = new CreateVoucherRequest
        {
            Type = VoucherType.DISCOUNT_FIXED_AMOUNT,
            DiscountValue = 1000,
            EndDate = DateTimeOffset.UtcNow.AddMinutes(-10), // Quá khứ
            InitQuantity = 10,
            MaxUsagePerUser = 1
        };

        var act = () => _handler.Handle(new CreateVoucherCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage(VoucherMsg.InvalidEndDate);
    }

    // 8. Test Switch Case: Default (Dòng 81-82)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenTypeIsInvalid()
    {
        var request = new CreateVoucherRequest { Type = (VoucherType)999 };

        var act = () => _handler.Handle(new CreateVoucherCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage(VoucherMsg.InvalidType);
    }

    // 9. Test Case: DiscountValue null (Dòng 43)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenDiscountValueIsNull()
    {
        var request = new CreateVoucherRequest
        {
            Type = VoucherType.DISCOUNT_FIXED_AMOUNT,
            DiscountValue = null,
            InitQuantity = 10,
            MaxUsagePerUser = 1,
            EndDate = DateTimeOffset.UtcNow.AddDays(1)
        };

        var act = () => _handler.Handle(new CreateVoucherCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage(VoucherMsg.InvalidDiscountValue);
    }
}