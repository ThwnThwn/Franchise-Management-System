using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
using EngagementService.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EngagementService.Application.Tests;

public class PointRuleSyncServiceTests
{
    private readonly Mock<IPointRuleRepository> _repoMock;
    private readonly Mock<IPointRuleCache> _cacheMock;
    private readonly PointRuleSyncService _service;

    public PointRuleSyncServiceTests()
    {
        _repoMock = new Mock<IPointRuleRepository>();
        _cacheMock = new Mock<IPointRuleCache>();
        _service = new PointRuleSyncService(_repoMock.Object, _cacheMock.Object);
    }

    // 1. Happy Path: Sync thành công với dữ liệu Entity thật (Dòng 34-52)
    [Fact]
    public async Task SyncAllAsync_ShouldClearCacheAndSyncAllEntities_WhenDbHasData()
    {
        // Arrange - Sử dụng Entity PointRule thật
        var rules = new List<PointRule>
        {
            new PointRule
            {
                Id = Guid.NewGuid(),
                ReasonCode = PointReasonCode.ORDER_COMPLETED,
                ExpiryDays = 180,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new PointRule
            {
                Id = Guid.NewGuid(),
                ReasonCode = PointReasonCode.BIRTHDAY_GIFT,
                ExpiryDays = 30,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        _cacheMock.Setup(x => x.ClearAllPointRuleCacheAsync()).Returns(Task.CompletedTask);

        // Fix CS0854: Khớp với signature GetAllAsync(null, null)
        _repoMock.Setup(x => x.GetAllAsync(null, null)).ReturnsAsync(rules);

        _cacheMock.Setup(x => x.SetPointRuleAsync(It.IsAny<Guid>(), It.IsAny<PointRule>()))
                  .Returns(Task.CompletedTask);

        // Act
        await _service.SyncAllAsync();

        // Assert
        // Bước 1: Phải dọn dẹp cache trước
        _cacheMock.Verify(x => x.ClearAllPointRuleCacheAsync(), Times.Once);

        // Bước 2: Lấy dữ liệu từ Repo
        _repoMock.Verify(x => x.GetAllAsync(null, null), Times.Once);

        // Bước 3: Đẩy từng Entity lên Cache (Verify đúng ID và đối tượng)
        foreach (var rule in rules)
        {
            _cacheMock.Verify(x => x.SetPointRuleAsync(rule.Id, rule), Times.Once);
        }

        _cacheMock.Verify(x => x.SetPointRuleAsync(It.IsAny<Guid>(), It.IsAny<PointRule>()), Times.Exactly(rules.Count));
    }

    // 2. Phủ nhánh: Danh sách rỗng hoặc null -> Thoát sớm (Dòng 39-42)
    [Theory]
    [InlineData(true)]  // Case list null
    [InlineData(false)] // Case list empty
    public async Task SyncAllAsync_ShouldReturnEarly_WhenNoDataToSync(bool isNull)
    {
        // Arrange
        List<PointRule>? rules = isNull ? null : new List<PointRule>();

        _cacheMock.Setup(x => x.ClearAllPointRuleCacheAsync()).Returns(Task.CompletedTask);
        _repoMock.Setup(x => x.GetAllAsync(null, null)).ReturnsAsync(rules!);

        // Act
        await _service.SyncAllAsync();

        // Assert
        _cacheMock.Verify(x => x.ClearAllPointRuleCacheAsync(), Times.Once);

        // Không được gọi lệnh Set cache nếu không có dữ liệu
        _cacheMock.Verify(x => x.SetPointRuleAsync(It.IsAny<Guid>(), It.IsAny<PointRule>()), Times.Never);
    }
}