using EngagementService.Application.AppExceptions;
using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.MembershipTier;
using EngagementService.Application.Features.MembershipTier.Commands.Create;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using FluentAssertions;
using MediatR;
using Moq;

namespace EngagementService.Application.Tests.Features.MembershipTierTests;

public class CreateMembershipTierHandlerTests
{
    private readonly Mock<IMembershipTierRepository> _repoMock;
    private readonly Mock<IMembershipTierCache> _cacheMock;
    private readonly CreateMembershipTierHandler _handler;

    public CreateMembershipTierHandlerTests()
    {
        _repoMock = new Mock<IMembershipTierRepository>();
        _cacheMock = new Mock<IMembershipTierCache>();
        _handler = new CreateMembershipTierHandler(_repoMock.Object, _cacheMock.Object);
    }

    // 1. Phủ nhánh: Trùng tên (Dòng 33-34)
    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenNameAlreadyExists()
    {
        // Arrange
        var request = new CreateMembershipTierRequest { Name = "Gold", MinSpendRequired = 1000 };
        var command = new CreateMembershipTierCommand(request);

        var existingTiers = new List<Domain.Entities.MembershipTier>
        {
            new Domain.Entities.MembershipTier { Name = "GOLD" } // Trùng tên (case-insensitive)
        };

        // Mock Cache: Trả về list đã tồn tại
        _cacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<Domain.Entities.MembershipTier>>>>()))
            .ReturnsAsync(existingTiers);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictDuplicateException>()
            .WithMessage(MembershipTierMsg.NameExisted);
    }

    // 2. Phủ nhánh: Trùng MinSpendRequired (Dòng 37-38)
    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenMinSpendAlreadyExists()
    {
        // Arrange
        var request = new CreateMembershipTierRequest { Name = "Silver", MinSpendRequired = 500 };
        var command = new CreateMembershipTierCommand(request);

        var existingTiers = new List<Domain.Entities.MembershipTier>
        {
            new Domain.Entities.MembershipTier { Name = "Bronze", MinSpendRequired = 500 } // Trùng MinSpend
        };

        _cacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<Domain.Entities.MembershipTier>>>>()))
            .ReturnsAsync(existingTiers);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictDuplicateException>()
            .WithMessage(MembershipTierMsg.MinSpendRequiredExisted);
    }

    [Fact]
    public async Task Handle_ShouldCreateSuccessfully_AndClearCache()
    {
        // Arrange
        var request = new CreateMembershipTierRequest
        {
            Name = "Platinum",
            MinSpendRequired = 5000,
            DiscountRate = 0.1,
            AmountPerPoint = 1000,
            ValuePerPoint = 100
        };
        var command = new CreateMembershipTierCommand(request);

        var dbData = new List<Domain.Entities.MembershipTier>();

        // Mock Cache và thực thi callback
        _cacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<Domain.Entities.MembershipTier>>>>()))
            .Returns<Func<Task<List<Domain.Entities.MembershipTier>>>>(async (func) =>
            {
                return await func();
            });

        // FIX LỖI CS0854 TẠI ĐÂY:
        _repoMock.Setup(x => x.GetAllAsync(null, null))
            .ReturnsAsync(dbData);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _repoMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.MembershipTier>()), Times.Once);
        _cacheMock.Verify(x => x.ClearAllMembershipTierCacheAsync(), Times.Once);
    }
}