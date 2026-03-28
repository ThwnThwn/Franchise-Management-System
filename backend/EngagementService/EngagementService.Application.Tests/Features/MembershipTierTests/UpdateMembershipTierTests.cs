using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.MembershipTier;
using EngagementService.Application.Features.MembershipTier.Commands.Update;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using FluentAssertions;
using MediatR;
using Moq;

namespace EngagementService.Application.Tests.Features.MembershipTierTests;

public class UpdateMembershipTierHandlerTests
{
    private readonly Mock<IMembershipTierRepository> _repoMock;
    private readonly Mock<IMembershipTierCache> _cacheMock;
    private readonly UpdateMembershiptierHandler _handler;

    public UpdateMembershipTierHandlerTests()
    {
        _repoMock = new Mock<IMembershipTierRepository>();
        _cacheMock = new Mock<IMembershipTierCache>();
        _handler = new UpdateMembershiptierHandler(_repoMock.Object, _cacheMock.Object);
    }

    // 1. Phủ nhánh: Không tìm thấy Tier (Dòng 24)
    [Fact]
    public async Task Handle_ShouldThrowDirectoryNotFoundException_WhenTierNotFound()
    {
        var request = new UpdateMembershipTierRequest { Id = Guid.NewGuid() };
        _repoMock.Setup(x => x.GetByIdAsync(request.Id)).ReturnsAsync((MembershipTier)null!);

        var act = () => _handler.Handle(new UpdateMembershipTierCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<DirectoryNotFoundException>().WithMessage(MembershipTierMsg.NotFound);
    }

    // 2. Phủ nhánh: Trùng tên (Dòng 38)
    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNameAlreadyExists()
    {
        var tierId = Guid.NewGuid();
        var request = new UpdateMembershipTierRequest { Id = tierId, Name = "Gold" };
        var existingTier = new MembershipTier { Id = tierId, Name = "OldName" };

        var allTiers = new List<MembershipTier>
        {
            new MembershipTier { Id = Guid.NewGuid(), Name = "GOLD" } // Trùng tên nhưng khác ID
        };

        _repoMock.Setup(x => x.GetByIdAsync(tierId)).ReturnsAsync(existingTier);
        _cacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<MembershipTier>>>>())).ReturnsAsync(allTiers);

        var act = () => _handler.Handle(new UpdateMembershipTierCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(MembershipTierMsg.NameExisted);
    }

    // 3. Phủ nhánh: Trùng MinSpend (Dòng 46)
    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenMinSpendAlreadyExists()
    {
        var tierId = Guid.NewGuid();
        var request = new UpdateMembershipTierRequest { Id = tierId, MinSpendRequired = 500 };
        var existingTier = new MembershipTier { Id = tierId, MinSpendRequired = 100 };

        var allTiers = new List<MembershipTier>
        {
            new MembershipTier { Id = Guid.NewGuid(), MinSpendRequired = 500 }
        };

        _repoMock.Setup(x => x.GetByIdAsync(tierId)).ReturnsAsync(existingTier);
        _cacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<MembershipTier>>>>())).ReturnsAsync(allTiers);

        var act = () => _handler.Handle(new UpdateMembershipTierCommand(request), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(MembershipTierMsg.MinSpendRequiredExisted);
    }

    // 4. Phủ xanh Happy Path: Cập nhật tất cả các trường (Dòng 35 - 71)
    [Fact]
    public async Task Handle_ShouldUpdateSuccessfully_WhenAllFieldsProvided()
    {
        // Arrange
        var tierId = Guid.NewGuid();
        var request = new UpdateMembershipTierRequest
        {
            Id = tierId,
            Name = "New Platinum",
            MinSpendRequired = 1000,
            DiscountRate = 0.15,
            ValuePerPoint = 200,
            AmountPerPoint = 2000
        };
        var existingTier = new MembershipTier { Id = tierId, Name = "Old", MinSpendRequired = 100 };

        _repoMock.Setup(x => x.GetByIdAsync(tierId)).ReturnsAsync(existingTier);

        // Mock Cache thực thi callback để phủ dòng 26-32
        _cacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<MembershipTier>>>>()))
            .Returns<Func<Task<List<MembershipTier>>>>(async (func) => await func());

        _repoMock.Setup(x => x.GetAllAsync(null, null)).ReturnsAsync(new List<MembershipTier>());

        // Act
        var result = await _handler.Handle(new UpdateMembershipTierCommand(request), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        existingTier.Name.Should().Be(request.Name);
        existingTier.MinSpendRequired.Should().Be(request.MinSpendRequired.Value);
        existingTier.DiscountRate.Should().Be(request.DiscountRate.Value);

        _repoMock.Verify(x => x.UpdateAsync(existingTier), Times.Once);
        _cacheMock.Verify(x => x.ClearAllMembershipTierCacheAsync(), Times.Once);
    }
}