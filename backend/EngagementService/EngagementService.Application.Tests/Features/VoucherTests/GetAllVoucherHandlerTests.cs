using EngagementService.Application.Dtos.Voucher;
using EngagementService.Application.Features.Voucher.Queries.GetAll;
using EngagementService.Application.Persistences.Repository;
using FluentAssertions;
using Moq;

namespace EngagementService.Application.Tests.Features.VoucherTests;

public class GetAllVoucherHandlerTests
{
    private readonly Mock<IVoucherRepository> _voucherRepoMock;
    private readonly Mock<IVoucherQueueRepository> _voucherQueueRepoMock;
    private readonly GetAllVoucherHandler _handler;

    public GetAllVoucherHandlerTests()
    {
        _voucherRepoMock = new Mock<IVoucherRepository>();
        _voucherQueueRepoMock = new Mock<IVoucherQueueRepository>();

        _handler = new GetAllVoucherHandler(
            _voucherRepoMock.Object,
            _voucherQueueRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCoverAllBranches_InVoucherLoop()
    {
        // Arrange
        var voucherId1 = Guid.NewGuid();
        var voucherId2 = Guid.NewGuid();

        // Tạo 2 voucher để test 2 nhánh của IF bên trong vòng lặp
        var vouchers = new List<Domain.Entities.Voucher>
        {
            new Domain.Entities.Voucher { Id = voucherId1, CurrentQuantity = 100 }, // Nhánh IF TRUE
            new Domain.Entities.Voucher { Id = voucherId2, CurrentQuantity = 50 }   // Nhánh IF FALSE (không có trong queue)
        };

        // Chỉ đưa voucherId1 vào queue
        var voucherQueues = new List<CountVoucherQueueRes>
        {
            new CountVoucherQueueRes { VoucherId = voucherId1, Count = 5 }
        };

        _voucherRepoMock.Setup(x => x.GetAllVoucherAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(vouchers);

        _voucherQueueRepoMock.Setup(x => x.GetVouchersAndAmountInQueueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(voucherQueues);

        // Act
        var result = await _handler.Handle(new GetAllVoucherQuery(), CancellationToken.None);

        // Assert
        var resultList = result.ToList();

        // Kiểm tra nhánh IF TRUE (Voucher 1 được trừ tiền)
        resultList.First(v => v.Id == voucherId1).CurrentQuantity.Should().Be(95);

        // Kiểm tra nhánh IF FALSE (Voucher 2 giữ nguyên - dòng này cực kỳ quan trọng để phủ branch)
        resultList.First(v => v.Id == voucherId2).CurrentQuantity.Should().Be(50);

        _voucherRepoMock.Verify(x => x.GetAllVoucherAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoVouchersExist()
    {
        // Arrange
        // Case này giúp phủ nhánh khi vòng lặp foreach không chạy (vouchers empty)
        _voucherRepoMock.Setup(x => x.GetAllVoucherAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Voucher>());

        _voucherQueueRepoMock.Setup(x => x.GetVouchersAndAmountInQueueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CountVoucherQueueRes>());

        // Act
        var result = await _handler.Handle(new GetAllVoucherQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}