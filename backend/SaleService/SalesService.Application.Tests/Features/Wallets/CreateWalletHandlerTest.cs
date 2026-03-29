using Moq;
using FluentAssertions;
using SalesService.Application.Features.Wallets.Command.Create;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.Wallets.Command.Create.CreateWalletCommand;

namespace SalesService.Application.Tests.Features.Wallets;

public class CreateWalletCommandHandlerTests
{
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly CreateWalletCommandHandler _handler;

    public CreateWalletCommandHandlerTests()
    {
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _handler = new CreateWalletCommandHandler(_walletRepositoryMock.Object);
    }

    // 1. THÀNH CÔNG - Tạo ví mới
    [Fact]
    public async Task Handle_ShouldCreateWallet_WhenUserDoesNotHaveWallet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateWalletCommand(userId);
        var walletId = Guid.NewGuid();

        _walletRepositoryMock
            .Setup(x => x.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _walletRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(walletId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.Balance.Should().Be(0);
        result.Value.Status.Should().Be(WalletStatus.Active);

        _walletRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Wallet>(w => w.UserId == userId && w.Balance == 0), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // 2. LỖI - Ví đã tồn tại → trả về Result.Error
    [Fact]
    public async Task Handle_ShouldReturnError_WhenWalletAlreadyExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateWalletCommand(userId);

        _walletRepositoryMock
            .Setup(x => x.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain($"Wallet already exists for user {userId}");

        // Xác nhận không tạo ví mới
        _walletRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // 3. LỖI - UserId rỗng → trả về Result.Invalid
    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new CreateWalletCommand(Guid.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.ErrorMessage == "UserId cannot be empty");

        // Xác nhận không gọi repository
        _walletRepositoryMock.Verify(
            x => x.ExistsByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _walletRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // 4. LỖI - AddAsync trả về Guid.Empty → tạo ví thất bại
    [Fact]
    public async Task Handle_ShouldReturnError_WhenAddAsyncReturnEmptyGuid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateWalletCommand(userId);

        _walletRepositoryMock
            .Setup(x => x.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _walletRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.Empty); // Giả lập tạo thất bại

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Failed to create wallet");
    }
}
