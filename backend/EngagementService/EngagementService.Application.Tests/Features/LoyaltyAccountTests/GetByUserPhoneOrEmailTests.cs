using EngagementService.Application.ClientInterface;
using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.Indentity;
using EngagementService.Application.Dtos.LoyaltyAccount;
using EngagementService.Application.Features.LoyaltyAccount.Queries.GetById;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EngagementService.Application.Tests.Features.LoyaltyAccountTests;

public class GetByPhoneOrEmailHandlerTests
{
    private readonly Mock<ILoyaltyAccountRepository> _loyaltyRepoMock;
    private readonly Mock<IIdentityServiceClient> _identityClientMock;
    private readonly GetByPhoneOrEmailHandler _handler;

    public GetByPhoneOrEmailHandlerTests()
    {
        _loyaltyRepoMock = new Mock<ILoyaltyAccountRepository>();
        _identityClientMock = new Mock<IIdentityServiceClient>();
        _handler = new GetByPhoneOrEmailHandler(_loyaltyRepoMock.Object, _identityClientMock.Object);
    }

    // 1. Phủ nhánh: Tìm theo Phone (Dòng 30-33)
    [Fact]
    public async Task Handle_ShouldReturnLoyaltyAccounts_WhenSearchingByPhone()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetByUserPhoneOrEmailRequest { PhoneNumber = "0123456789" };
        var query = new GetByPhoneOrEmailQuery(request);

        var identityResponse = new UserSimpleResp
        {
            UserIDNameResp = new List<UserIDNameResp> { new UserIDNameResp { UserId = userId, UserName = "DuyLe" } }
        };

        var loyaltyAccounts = new List<LoyaltyAccount?>
        {
            new LoyaltyAccount { Id = userId, AvailablePoints = 100, TotalSpent = 500 }
        };

        _identityClientMock.Setup(x => x.GetUserIdByPhone(It.IsAny<GetUserIdByPhoneRequest>()))
            .ReturnsAsync(identityResponse);

        _loyaltyRepoMock.Setup(x => x.GetUserByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(loyaltyAccounts);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].UserName.Should().Be("DuyLe");
        _identityClientMock.Verify(x => x.GetUserIdByPhone(It.Is<GetUserIdByPhoneRequest>(r => r.PhoneNumber == "0123456789")), Times.Once);
    }

    // 2. Phủ nhánh: Tìm theo Email (Dòng 34-37)
    [Fact]
    public async Task Handle_ShouldReturnLoyaltyAccounts_WhenSearchingByEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetByUserPhoneOrEmailRequest { Email = "duy@fpt.edu.vn" };
        var query = new GetByPhoneOrEmailQuery(request);

        var identityResponse = new UserSimpleResp
        {
            UserIDNameResp = new List<UserIDNameResp> { new UserIDNameResp { UserId = userId, UserName = "DuyEmail" } }
        };

        _identityClientMock.Setup(x => x.GetUserIdByEmail(It.IsAny<GetUserIdByEmailRequest>()))
            .ReturnsAsync(identityResponse);

        _loyaltyRepoMock.Setup(x => x.GetUserByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<LoyaltyAccount?> { new LoyaltyAccount { Id = userId } });

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _identityClientMock.Verify(x => x.GetUserIdByEmail(It.Is<GetUserIdByEmailRequest>(r => r.Email == "duy@fpt.edu.vn")), Times.Once);
    }

    // 3. Phủ nhánh: Không tìm thấy tài khoản loyalty (Dòng 41-42)
    [Fact]
    public async Task Handle_ShouldThrowDirectoryNotFoundException_WhenNoLoyaltyAccountExists()
    {
        // Arrange
        var request = new GetByUserPhoneOrEmailRequest { PhoneNumber = "0999" };
        _identityClientMock.Setup(x => x.GetUserIdByPhone(It.IsAny<GetUserIdByPhoneRequest>()))
            .ReturnsAsync(new UserSimpleResp { UserIDNameResp = new List<UserIDNameResp>() });

        _loyaltyRepoMock.Setup(x => x.GetUserByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<LoyaltyAccount?>());

        // Act
        var act = () => _handler.Handle(new GetByPhoneOrEmailQuery(request), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>().WithMessage(LoyaltyAccountMsg.NotFound);
    }

    // 4. Phủ nhánh: Danh sách chứa account null hoặc không khớp userInfo (Dòng 48-52)
    [Fact]
    public async Task Handle_ShouldSkip_WhenAccountIsNull()
    {
        // Arrange
        var request = new GetByUserPhoneOrEmailRequest { PhoneNumber = "0111" };
        var userId = Guid.NewGuid();

        var identityResponse = new UserSimpleResp
        {
            UserIDNameResp = new List<UserIDNameResp> { new UserIDNameResp { UserId = userId, UserName = "ValidUser" } }
        };

        // Trả về list có 1 cái null, 1 cái ID không khớp trong identityResponse
        var loyaltyAccounts = new List<LoyaltyAccount?>
        {
            null,
            new LoyaltyAccount { Id = Guid.NewGuid() } // Không có trong UserIDNameResp
        };

        _identityClientMock.Setup(x => x.GetUserIdByPhone(It.IsAny<GetUserIdByPhoneRequest>())).ReturnsAsync(identityResponse);
        _loyaltyRepoMock.Setup(x => x.GetUserByIdsAsync(It.IsAny<List<Guid>>())).ReturnsAsync(loyaltyAccounts);

        // Act
        var result = await _handler.Handle(new GetByPhoneOrEmailQuery(request), CancellationToken.None);

        // Assert
        result.Should().BeEmpty(); // Cả 2 đều bị bỏ qua bởi lệnh continue và if(userInfo != null)
    }
}