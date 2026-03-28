using EngagementService.Application.Features.MembershipTier.Queries.GetAll;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EngagementService.Application.Tests.Features.MembershipTierTests;

public class GetAllMembershipTierHandlerTests
{
    private readonly Mock<IMembershipTierRepository> _repoMock;
    private readonly Mock<IMembershipTierCache> _cacheMock;
    private readonly GetAllMembershipTierHandler _handler;

    public GetAllMembershipTierHandlerTests()
    {
        _repoMock = new Mock<IMembershipTierRepository>();
        _cacheMock = new Mock<IMembershipTierCache>();
        _handler = new GetAllMembershipTierHandler(_repoMock.Object, _cacheMock.Object);
    }

    // 1. Phủ nhánh: Không tìm thấy hạng thành viên nào (Dòng 34-35)
    [Fact]
    public async Task Handle_ShouldThrowDirectoryNotFoundException_WhenTiersListIsEmpty()
    {
        // Arrange
        var query = new GetAllMembershipTierQuery();

        // Giả lập cache trả về list rỗng
        _cacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<MembershipTier>>>>()))
            .ReturnsAsync(new List<MembershipTier>());

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>()
            .WithMessage("Không tìm thấy hạng thành viên nào");
    }

    // 2. Phủ xanh Happy Path & Thực thi Callback (Dòng 24-32 và 37-46)
    [Fact]
    public async Task Handle_ShouldReturnMembershipTierList_WhenDataExists()
    {
        // Arrange
        var query = new GetAllMembershipTierQuery();
        var tiersFromDb = new List<MembershipTier>
        {
            new MembershipTier
            {
                Id = Guid.NewGuid(),
                Name = "Bronze",
                MinSpendRequired = 0,
                DiscountRate = 0,
                AmountPerPoint = 1000,
                ValuePerPoint = 10
            }
        };

        // BẪY COVERAGE: Thực thi hàm callback để phủ dòng 24-32
        _cacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<MembershipTier>>>>()))
            .Returns<Func<Task<List<MembershipTier>>>>(async (func) => await func());

        // Fix lỗi CS0854: Truyền null, null cho GetAllAsync
        _repoMock.Setup(x => x.GetAllAsync(null, null))
            .ReturnsAsync(tiersFromDb);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Bronze");
        result[0].Id.Should().Be(tiersFromDb[0].Id);

        _repoMock.Verify(x => x.GetAllAsync(null, null), Times.Once);
    }

    // 3. Phủ nhánh: allTiers bị null (Dòng 34)
    [Fact]
    public async Task Handle_ShouldThrowDirectoryNotFoundException_WhenTiersListIsNull()
    {
        // Arrange
        var query = new GetAllMembershipTierQuery();

        _cacheMock.Setup(x => x.GetOrSetAllAsync(It.IsAny<Func<Task<List<MembershipTier>>>>()))
            .ReturnsAsync((List<MembershipTier>)null!);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>();
    }
}