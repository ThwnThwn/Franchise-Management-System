using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Features.Campaign.Queries.GetAll;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EngagementService.Application.Tests.Features.CampaignTests;

public class GetAllCampaignHandlerTests
{
    private readonly Mock<IVoucherRepository> _voucherRepoMock;
    private readonly Mock<ICampaignRepository> _campaignRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetActiveHandler _handler; // Lưu ý: Tên class trong file Duy gửi là GetActiveHandler

    public GetAllCampaignHandlerTests()
    {
        _voucherRepoMock = new Mock<IVoucherRepository>();
        _campaignRepoMock = new Mock<ICampaignRepository>();
        _mapperMock = new Mock<IMapper>();

        _handler = new GetActiveHandler(
            _campaignRepoMock.Object,
            _mapperMock.Object);
    }

    // 1. Phủ xanh Happy Path: Lấy toàn bộ danh sách thành công
    [Fact]
    public async Task Handle_ShouldReturnAllCampaigns_WhenExist()
    {
        // Arrange
        var query = new GetAllQuery();
        var campaignEntities = new List<Campaign>
        {
            new Campaign { Id = Guid.NewGuid(), Name = "Campaign 1" },
            new Campaign { Id = Guid.NewGuid(), Name = "Campaign 2" }
        };

        var campaignResponses = new List<CampaignRes>
        {
            new CampaignRes { Id = campaignEntities[0].Id, Name = "Campaign 1" },
            new CampaignRes { Id = campaignEntities[1].Id, Name = "Campaign 2" }
        };

        _campaignRepoMock.Setup(x => x.GetAllCampaign(null))
            .ReturnsAsync(campaignEntities);

        _mapperMock.Setup(x => x.Map<IEnumerable<CampaignRes>>(campaignEntities))
            .Returns(campaignResponses);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Campaign 1");

        _campaignRepoMock.Verify(x => x.GetAllCampaign(null), Times.Once);
        _mapperMock.Verify(x => x.Map<IEnumerable<CampaignRes>>(campaignEntities), Times.Once);
    }

    // 2. Phủ nhánh: Danh sách rỗng
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCampaignsExist()
    {
        // Arrange
        var query = new GetAllQuery();
        var emptyEntities = new List<Campaign>();

        _campaignRepoMock.Setup(x => x.GetAllCampaign(null))
            .ReturnsAsync(emptyEntities);

        _mapperMock.Setup(x => x.Map<IEnumerable<CampaignRes>>(emptyEntities))
            .Returns(new List<CampaignRes>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}