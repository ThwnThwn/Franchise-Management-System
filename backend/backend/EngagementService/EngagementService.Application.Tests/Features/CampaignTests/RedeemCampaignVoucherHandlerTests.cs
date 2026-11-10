using AutoMapper;
using EngagementService.Application.AppExceptions;
using EngagementService.Application.Dtos.Campaign.Request;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Features.Campaign.Commands.RedeemCampaignVoucher;
using EngagementService.Application.Persistences.Repository;
using FluentAssertions;
using IdentityService.Application.Abstrations;
using Moq;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EngagementService.Application.Tests.Features.CampaignTests;

public class RedeemCampaignVoucherHandlerTests
{
    private readonly Mock<ICampaignRepository> _campaignRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICampaignCache> _campaignCacheMock;
    private readonly Mock<ILoyaltyAccountRepository> _loyaltyAccountRepoMock;
    private readonly Mock<IVoucherRepository> _voucherRepoMock;
    private readonly Mock<IKafkaProducer> _kafkaProducerMock;
    private readonly RedeemCampaignVoucherHandler _handler;

    public RedeemCampaignVoucherHandlerTests()
    {
        _campaignRepoMock = new Mock<ICampaignRepository>();
        _mapperMock = new Mock<IMapper>();
        _campaignCacheMock = new Mock<ICampaignCache>();
        _loyaltyAccountRepoMock = new Mock<ILoyaltyAccountRepository>();
        _voucherRepoMock = new Mock<IVoucherRepository>();
        _kafkaProducerMock = new Mock<IKafkaProducer>();

        _handler = new RedeemCampaignVoucherHandler(
            _campaignRepoMock.Object,
            _mapperMock.Object,
            _campaignCacheMock.Object,
            _loyaltyAccountRepoMock.Object,
            _voucherRepoMock.Object,
            _kafkaProducerMock.Object);
    }

    // 1. Phủ nhánh: Voucher không tồn tại (Dòng 26-27)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenVoucherNotFound()
    {
        var request = new RedeemCampaignVoucherReq { VoucherId = Guid.NewGuid() };
        _voucherRepoMock.Setup(x => x.GetByIdAsync(request.VoucherId)).ReturnsAsync((Domain.Entities.Voucher)null!);

        var act = () => _handler.Handle(new RedeemCampaignVoucherCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>().WithMessage($"voucher does not exit *");
    }

    // 2. Phủ nhánh: Voucher không thuộc Campaign nào (Dòng 29-32)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenVoucherHasNoCampaign()
    {
        var request = new RedeemCampaignVoucherReq { VoucherId = Guid.NewGuid() };
        var voucher = new Domain.Entities.Voucher { Id = request.VoucherId, CampaignId = null };
        _voucherRepoMock.Setup(x => x.GetByIdAsync(request.VoucherId)).ReturnsAsync(voucher);

        var act = () => _handler.Handle(new RedeemCampaignVoucherCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>().WithMessage($"voucher does not has this campaign *");
    }

    // 3. Phủ nhánh: Loyalty Account không tồn tại (Dòng 34-35)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenLoyaltyAccountNotFound()
    {
        var request = new RedeemCampaignVoucherReq { UserId = Guid.NewGuid(), VoucherId = Guid.NewGuid() };
        var voucher = new Domain.Entities.Voucher { Id = request.VoucherId, CampaignId = Guid.NewGuid() };
        _voucherRepoMock.Setup(x => x.GetByIdAsync(request.VoucherId)).ReturnsAsync(voucher);
        _loyaltyAccountRepoMock.Setup(x => x.GetByUserIdAsync(request.UserId)).ReturnsAsync((Domain.Entities.LoyaltyAccount)null!);

        var act = () => _handler.Handle(new RedeemCampaignVoucherCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>().WithMessage($"loyalty account does not exit *");
    }

    // 4. Phủ nhánh: Không đủ điểm (Dòng 37-43)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenNotEnoughPoints()
    {
        var request = new RedeemCampaignVoucherReq { UserId = Guid.NewGuid(), VoucherId = Guid.NewGuid() };
        var voucher = new Domain.Entities.Voucher { Id = request.VoucherId, CampaignId = Guid.NewGuid(), PointCost = 100 };
        var loyalty = new Domain.Entities.LoyaltyAccount { AvailablePoints = 50 }; // Chỉ có 50 điểm

        _voucherRepoMock.Setup(x => x.GetByIdAsync(request.VoucherId)).ReturnsAsync(voucher);
        _loyaltyAccountRepoMock.Setup(x => x.GetByUserIdAsync(request.UserId)).ReturnsAsync(loyalty);

        var act = () => _handler.Handle(new RedeemCampaignVoucherCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>().WithMessage($"not enough point *");
    }

    // 5. Phủ nhánh: Redis - User đã sở hữu voucher (Dòng 49)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserAlreadyHasVoucherInCache()
    {
        var request = new RedeemCampaignVoucherReq { UserId = Guid.NewGuid(), VoucherId = Guid.NewGuid() };
        SetupValidCommonMocks(request, out _);
        _campaignCacheMock.Setup(x => x.RedeemVoucherAsync(request.VoucherId, request.UserId)).ReturnsAsync(-1);

        var act = () => _handler.Handle(new RedeemCampaignVoucherCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>().WithMessage("User already has this voucher.");
    }

    // 6. Phủ nhánh: Redis - Hết hàng (Dòng 50)
    [Fact]
    public async Task Handle_ShouldThrowException_WhenVoucherOutOfStockInCache()
    {
        var request = new RedeemCampaignVoucherReq { UserId = Guid.NewGuid(), VoucherId = Guid.NewGuid() };
        SetupValidCommonMocks(request, out _);
        _campaignCacheMock.Setup(x => x.RedeemVoucherAsync(request.VoucherId, request.UserId)).ReturnsAsync(0);

        var act = () => _handler.Handle(new RedeemCampaignVoucherCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>().WithMessage("Voucher out of stock.");
    }

    // 7. Happy Path: Redeem thành công và có trừ điểm (Dòng 53-76)
    [Fact]
    public async Task Handle_ShouldRedeemSuccessfully_AndPublishKafkaEvents()
    {
        // Arrange
        var request = new RedeemCampaignVoucherReq { UserId = Guid.NewGuid(), VoucherId = Guid.NewGuid() };
        SetupValidCommonMocks(request, out var campaignId);

        var voucher = new Domain.Entities.Voucher { Id = request.VoucherId, CampaignId = campaignId, PointCost = 10 };
        _voucherRepoMock.Setup(x => x.GetByIdAsync(request.VoucherId)).ReturnsAsync(voucher);
        _campaignCacheMock.Setup(x => x.RedeemVoucherAsync(request.VoucherId, request.UserId)).ReturnsAsync(1);

        var campaignEntity = new Domain.Entities.Campaign { Id = campaignId };
        _campaignRepoMock.Setup(x => x.GetCampaignById(campaignId)).ReturnsAsync(campaignEntity);
        _mapperMock.Setup(x => x.Map<CampaignRes>(campaignEntity)).Returns(new CampaignRes());

        // Act
        await _handler.Handle(new RedeemCampaignVoucherCommand(request), CancellationToken.None);

        // Assert
        // Verify Kafka gọi 2 lần: 1 cho Redeem, 1 cho UpdatePoint
        _kafkaProducerMock.Verify(x => x.PublishAsync(KafkaTopics.RedeemCampaignVoucher, It.IsAny<RedeemCampaignVoucherEvent>()), Times.Once);
        _kafkaProducerMock.Verify(x => x.PublishAsync(KafkaTopics.UpdateUserPoint, It.IsAny<UpdatePointEvent>()), Times.Once);
        _campaignRepoMock.Verify(x => x.GetCampaignById(campaignId), Times.Once);
    }

    // Helper để giảm code lặp
    private void SetupValidCommonMocks(RedeemCampaignVoucherReq request, out Guid campaignId)
    {
        campaignId = Guid.NewGuid();
        var voucher = new Domain.Entities.Voucher { Id = request.VoucherId, CampaignId = campaignId, PointCost = 0 };
        var loyalty = new Domain.Entities.LoyaltyAccount { AvailablePoints = 1000 };

        _voucherRepoMock.Setup(x => x.GetByIdAsync(request.VoucherId)).ReturnsAsync(voucher);
        _loyaltyAccountRepoMock.Setup(x => x.GetByUserIdAsync(request.UserId)).ReturnsAsync(loyalty);
    }
}