using EngagementService.Application.Constants;
using EngagementService.Application.Features.MembershipTier.Commands.Delete;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using FluentAssertions;
using MediatR;
using Moq;

namespace EngagementService.Application.Tests.Features.MembershipTierTests;

public class DeleteMembershipTierHandlerTests
{
    private readonly Mock<IMembershipTierRepository> _repoMock;
    private readonly Mock<IMembershipTierCache> _cacheMock;
    private readonly DeleteMembershipTierHandler _handler;

    public DeleteMembershipTierHandlerTests()
    {
        _repoMock = new Mock<IMembershipTierRepository>();
        _cacheMock = new Mock<IMembershipTierCache>();
        _handler = new DeleteMembershipTierHandler(_repoMock.Object, _cacheMock.Object);
    }

    // 1. Phủ nhánh: ID rỗng (Dòng 26-29)
    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenIdIsEmpty()
    {
        // Arrange
        var command = new DeleteMembershipTierCommand(Guid.Empty);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage(CommonMsg.InvalidId);

        // Đảm bảo không gọi vào DB hay Cache
        _repoMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    // 2. Phủ xanh Happy Path: Xóa thành công và Clear Cache (Dòng 30-33)
    [Fact]
    public async Task Handle_ShouldDeleteSuccessfully_AndClearCache()
    {
        // Arrange
        var tierId = Guid.NewGuid();
        var command = new DeleteMembershipTierCommand(tierId);

        _repoMock.Setup(x => x.DeleteAsync(tierId)).Returns(Task.CompletedTask);
        _cacheMock.Setup(x => x.ClearAllMembershipTierCacheAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        // Verify gọi đúng ID cần xóa
        _repoMock.Verify(x => x.DeleteAsync(tierId), Times.Once);

        // Verify lệnh Clear Cache được thực hiện
        _cacheMock.Verify(x => x.ClearAllMembershipTierCacheAsync(), Times.Once);
    }
}