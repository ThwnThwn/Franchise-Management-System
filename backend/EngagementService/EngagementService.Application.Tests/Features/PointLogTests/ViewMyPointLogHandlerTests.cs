using BuildingBlocks.Core.Pagination;
using EngagementService.Application.Features.PointLog.Queries;
using EngagementService.Application.Persistences.Repository;
using FluentAssertions;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EngagementService.Application.Tests.Features.PointLogTests;

public class ViewMyPointLogHandlerTests
{
    private readonly Mock<IPointLogRepository> _pointLogRepoMock;
    private readonly ViewMyPointLogHandler _handler;

    public ViewMyPointLogHandlerTests()
    {
        _pointLogRepoMock = new Mock<IPointLogRepository>();
        _handler = new ViewMyPointLogHandler(_pointLogRepoMock.Object);
    }

    // 1. Phủ xanh Happy Path: Trả về kết quả phân trang thành công
    [Fact]
    public async Task Handle_ShouldReturnPagedPointLogs_WhenRequestIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new ViewMyPointLogQuery
        {
            UserId = userId,
            Page = 1,
            PageSize = 10,
            FromDate = DateTimeOffset.UtcNow.AddDays(-7),
            ToDate = DateTimeOffset.UtcNow
        };

        var pointLogs = new List<Domain.Entities.PointLog>
        {
            new Domain.Entities.PointLog { Id = Guid.NewGuid(), UserId = userId, Amount = 100 }
        };

        var pagedResult = PagedResult<Domain.Entities.PointLog>.Create(pointLogs, 1, 1, 10);

        _pointLogRepoMock.Setup(x => x.GetPagedPointLogsAsync(
                query.UserId,
                query.FromDate,
                query.ToDate,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);

        _pointLogRepoMock.Verify(x => x.GetPagedPointLogsAsync(
            query.UserId,
            query.FromDate,
            query.ToDate,
            query.Page,
            query.PageSize,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // 2. Phủ xanh trường hợp danh sách rỗng (Empty Result)
    [Fact]
    public async Task Handle_ShouldReturnEmptyPagedResult_WhenNoLogsFound()
    {
        // Arrange
        var query = new ViewMyPointLogQuery { UserId = Guid.NewGuid(), Page = 1, PageSize = 10 };
        var emptyPagedResult = PagedResult<Domain.Entities.PointLog>.Create(new List<Domain.Entities.PointLog>(), 0, 1, 10);

        _pointLogRepoMock.Setup(x => x.GetPagedPointLogsAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.HasNextPage.Should().BeFalse();
    }
}