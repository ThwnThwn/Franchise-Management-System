using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Features.Campaign.Queries.GetById;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EngagementService.Application.Tests.Features.CampaignTests;

public class GetCampaignByIdHandlerTests
{
    private readonly Mock<ICampaignRepository> _campaignRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetByIdHandler _handler;

    public GetCampaignByIdHandlerTests()
    {
        _campaignRepoMock = new Mock<ICampaignRepository>();
        _mapperMock = new Mock<IMapper>();

        _handler = new GetByIdHandler(
            _campaignRepoMock.Object,
            _mapperMock.Object);
    }

    // 1. Phủ nhánh: Thất bại khi không tìm thấy Campaign (Dòng 28-31)
    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenCampaignNotFound()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var query = new GetByIdQuery(campaignId);

        _campaignRepoMock.Setup(x => x.GetCampaignById(campaignId))
            .ReturnsAsync((Campaign)null!);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithMessage($"*campaign is null with id: {campaignId} - GetById*");

        _mapperMock.Verify(x => x.Map<CampaignRes>(It.IsAny<Campaign>()), Times.Never);
    }

    // 2. Phủ xanh Happy Path: Tìm thấy Campaign thành công (Dòng 26, 32)
    [Fact]
    public async Task Handle_ShouldReturnCampaign_WhenExist()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var query = new GetByIdQuery(campaignId);

        var campaignEntity = new Campaign { Id = campaignId, Name = "Flash Sale" };
        var campaignRes = new CampaignRes { Id = campaignId, Name = "Flash Sale" };

        _campaignRepoMock.Setup(x => x.GetCampaignById(campaignId))
            .ReturnsAsync(campaignEntity);

        _mapperMock.Setup(x => x.Map<CampaignRes>(campaignEntity))
            .Returns(campaignRes);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(campaignId);
        result.Name.Should().Be("Flash Sale");

        _campaignRepoMock.Verify(x => x.GetCampaignById(campaignId), Times.Once);
        _mapperMock.Verify(x => x.Map<CampaignRes>(campaignEntity), Times.Once);
    }
}