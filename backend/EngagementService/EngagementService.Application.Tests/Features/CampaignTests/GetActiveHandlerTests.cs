using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Features.Campaign.Queries.GetActive;
using EngagementService.Application.Persistences.Repository;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;

namespace EngagementService.Application.Tests.Features.CampaignTests;

public class GetActiveHandlerTests
{
    private readonly Mock<ICampaignRepository> _campaignRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetActiveHandler _handler;

    public GetActiveHandlerTests()
    {
        _campaignRepoMock = new Mock<ICampaignRepository>();
        _mapperMock = new Mock<IMapper>();

        _handler = new GetActiveHandler(
            _campaignRepoMock.Object,
            _mapperMock.Object);
    }

    // 1. Phủ xanh Happy Path: Trả về danh sách campaign đang hoạt động
    [Fact]
    public async Task Handle_ShouldReturnActiveCampaigns_WhenExist()
    {
        // Arrange
        var query = new GetActiveQuery();
        var campaignEntities = new List<Domain.Entities.Campaign>
        {
            new Domain.Entities.Campaign { Id = Guid.NewGuid(), Name = "Active Campaign 1", IsActive = true },
            new Domain.Entities.Campaign { Id = Guid.NewGuid(), Name = "Active Campaign 2", IsActive = true }
        };

        var campaignResponses = new List<CampaignRes>
        {
            new CampaignRes { Id = campaignEntities[0].Id, Name = "Active Campaign 1" },
            new CampaignRes { Id = campaignEntities[1].Id, Name = "Active Campaign 2" }
        };

        // Mock repository trả về list entity
        _campaignRepoMock.Setup(x => x.GetAllCampaign(It.IsAny<Expression<Func<Domain.Entities.Campaign, bool>>>()))
            .ReturnsAsync(campaignEntities);

        // Mock mapper chuyển đổi từ Entity sang Response
        _mapperMock.Setup(x => x.Map<IEnumerable<CampaignRes>>(campaignEntities))
            .Returns(campaignResponses);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Active Campaign 1");

        _campaignRepoMock.Verify(x => x.GetAllCampaign(It.IsAny<Expression<Func<Domain.Entities.Campaign, bool>>>()), Times.Once);
        _mapperMock.Verify(x => x.Map<IEnumerable<CampaignRes>>(campaignEntities), Times.Once);
    }

    // 2. Phủ xanh trường hợp không có campaign nào thỏa điều kiện
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoActiveCampaignsExist()
    {
        // Arrange
        var query = new GetActiveQuery();
        var emptyEntities = new List<Domain.Entities.Campaign>();
        var emptyResponses = new List<CampaignRes>();

        _campaignRepoMock.Setup(x => x.GetAllCampaign(It.IsAny<Expression<Func<Domain.Entities.Campaign, bool>>>()))
            .ReturnsAsync(emptyEntities);

        _mapperMock.Setup(x => x.Map<IEnumerable<CampaignRes>>(emptyEntities))
            .Returns(emptyResponses);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _campaignRepoMock.Verify(x => x.GetAllCampaign(It.IsAny<Expression<Func<Domain.Entities.Campaign, bool>>>()), Times.Once);
    }
}