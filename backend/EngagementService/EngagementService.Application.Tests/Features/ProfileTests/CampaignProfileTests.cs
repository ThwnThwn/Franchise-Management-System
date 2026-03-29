//using AutoMapper;
//using EngagementService.Application.Dtos.Campaign.Request;
//using EngagementService.Application.Dtos.Campaign.Response;
//using EngagementService.Application.Mapper.Campaign;
//using EngagementService.Domain.Entities;
//using FluentAssertions;
//using Xunit;
//using System;

//namespace EngagementService.Application.Tests.MapperTests;

//public class CampaignProfileTests
//{
//    private readonly IConfigurationProvider _configuration;
//    private readonly IMapper _mapper;

//    public CampaignProfileTests()
//    {
//        _configuration = new MapperConfiguration(cfg =>
//        {
//            cfg.AddProfile<CampaignProfile>();
//        });

//        _mapper = _configuration.CreateMapper();
//    }

//    // 1. Phủ 100% config: Kiểm tra xem tất cả các mapping rule có hợp lệ không
//    [Fact]
//    public void ShouldHaveValidConfiguration()
//    {
//        _configuration.AssertConfigurationIsValid();
//    }

//    // 2. Test thực tế: Map từ Request sang Entity (CreateMap)
//    [Fact]
//    public void Map_CreateCampaignReq_To_CampaignEntity_ShouldBeValid()
//    {
//        // Arrange
//        var request = new CreateCampaginReq
//        {
//            Name = "Tet Holiday 2026",
//            Description = "Big Sale for Tet"
//            // Thêm các property khác của Duy vào đây
//        };

//        // Act
//        var result = _mapper.Map<Domain.Entities.Campaign>(request);

//        // Assert
//        result.Should().NotBeNull();
//        result.Name.Should().Be(request.Name);
//        result.Description.Should().Be(request.Description);
//    }

//    // 3. Test thực tế: Map từ Entity sang Response (CampaignRes)
//    [Fact]
//    public void Map_CampaignEntity_To_CampaignRes_ShouldBeValid()
//    {
//        // Arrange
//        var entity = new Domain.Entities.Campaign
//        {
//            Id = Guid.NewGuid(),
//            Name = "Membership Gift"
//        };

//        // Act
//        var result = _mapper.Map<CampaignRes>(entity);

//        // Assert
//        result.Should().NotBeNull();
//        result.Id.Should().Be(entity.Id);
//        result.Name.Should().Be(entity.Name);
//    }
//}