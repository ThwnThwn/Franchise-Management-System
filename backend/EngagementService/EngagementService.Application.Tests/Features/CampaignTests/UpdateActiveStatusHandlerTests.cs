using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Request;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Features.Campaign.Commands.UpdateActiveStatus;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using FluentAssertions;
using IdentityService.Application.Abstrations;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EngagementService.Application.Tests.Features.CampaignTests;

public class UpdateActiveStatusHandlerTests
{
    private readonly Mock<ICampaignRepository> _campaignRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICampaignCache> _campaignCacheMock;
    private readonly UpdateActiveStatusHandler _handler;

    public UpdateActiveStatusHandlerTests()
    {
        _campaignRepoMock = new Mock<ICampaignRepository>();
        _mapperMock = new Mock<IMapper>();
        _campaignCacheMock = new Mock<ICampaignCache>();

        _handler = new UpdateActiveStatusHandler(
            _campaignRepoMock.Object,
            _mapperMock.Object,
            _campaignCacheMock.Object);
    }

    // 1. Phủ nhánh: Id rỗng (Dòng 32-35)
    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenIdIsEmpty()
    {
        var request = new UpdateActiveStatusReq { Id = Guid.Empty };
        var command = new UpdateActiveStatusCommand(request);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("*Id is null*");
    }

    // 2. Phủ nhánh: Campaign không tồn tại (Dòng 37-40)
    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenCampaignNotFound()
    {
        var campaignId = Guid.NewGuid();
        var request = new UpdateActiveStatusReq { Id = campaignId };
        var command = new UpdateActiveStatusCommand(request);

        _campaignRepoMock.Setup(x => x.GetCampaignById(campaignId)).ReturnsAsync((Campaign)null!);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithMessage($"*campign with Id {campaignId} is null*");
    }

    // 3. Phủ nhánh: IsActive = true (Warm-up Cache) (Dòng 45-56)
    [Fact]
    public async Task Handle_ShouldWarmUpCache_WhenIsActiveIsTrue()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var request = new UpdateActiveStatusReq { Id = campaignId, IsActive = true };
        var command = new UpdateActiveStatusCommand(request);

        var vouchers = new List<Domain.Entities.Voucher>
        {
            new Domain.Entities.Voucher { Id = Guid.NewGuid(), InitQuantity = 100 }
        };
        var campaign = new Campaign { Id = campaignId, Vouchers = vouchers, EndDate = DateTimeOffset.UtcNow.AddDays(1) };

        _campaignRepoMock.Setup(x => x.GetCampaignById(campaignId)).ReturnsAsync(campaign);
        _mapperMock.Setup(x => x.Map<CampaignRes>(campaign)).Returns(new CampaignRes());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        campaign.IsActive.Should().BeTrue();
        vouchers[0].IsActive.Should().BeTrue();

        _campaignCacheMock.Verify(x => x.WarmUpVoucherAsync(
            vouchers[0].Id,
            vouchers[0].InitQuantity!.Value,
            campaign.EndDate), Times.Once);

        _campaignRepoMock.Verify(x => x.UpdateAsync(campaign), Times.Once);
    }

    // 4. Phủ nhánh: IsActive = false (Cleanup Cache) (Dòng 57-65)
    [Fact]
    public async Task Handle_ShouldDeleteCache_WhenIsActiveIsFalse()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var request = new UpdateActiveStatusReq { Id = campaignId, IsActive = false };
        var command = new UpdateActiveStatusCommand(request);

        var vouchers = new List<Domain.Entities.Voucher> { new Domain.Entities.Voucher { Id = Guid.NewGuid() } };
        var campaign = new Campaign { Id = campaignId, Vouchers = vouchers };

        _campaignRepoMock.Setup(x => x.GetCampaignById(campaignId)).ReturnsAsync(campaign);
        _mapperMock.Setup(x => x.Map<CampaignRes>(campaign)).Returns(new CampaignRes());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        campaign.IsActive.Should().BeFalse();
        vouchers[0].IsActive.Should().BeFalse();

        _campaignCacheMock.Verify(x => x.DeleteVoucherCacheAsync(vouchers[0].Id), Times.Once);
        _campaignRepoMock.Verify(x => x.UpdateAsync(campaign), Times.Once);
    }
}