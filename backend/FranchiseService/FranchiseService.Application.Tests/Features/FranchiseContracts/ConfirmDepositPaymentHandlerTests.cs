using FluentAssertions;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.FranchiseContracts.Commands.ConfirmDepositPayment;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using FranchiseService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace FranchiseService.Application.Tests.Features.FranchiseContracts;

public class ConfirmDepositPaymentHandlerTests
{
    private readonly Mock<IFranchiseUow> _uowMock = new();
    private readonly Mock<IFranchiseContractRepository> _contractRepoMock = new();
    private readonly Mock<IStoreRepository> _storeRepoMock = new();
    private readonly Mock<IIdentityServiceClient> _identityClientMock = new();
    private readonly Mock<ILogger<ConfirmDepositPaymentHandler>> _loggerMock = new();

    [Fact]
    public async Task Handle_ShouldThrow_WhenContractNotFound()
    {
        var command = new ConfirmDepositPaymentCommand
        {
            ContractId = Guid.NewGuid(),
            ConfirmedBy = Guid.NewGuid(),
            Note = "Thu tien coc Arabica"
        };

        _contractRepoMock
            .Setup(x => x.GetByIdAsync(command.ContractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FranchiseContract?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenContractStatusIsNotAwaitingPayment()
    {
        var contract = BuildContract(ContractStatus.AwaitingSignature);

        _contractRepoMock
            .Setup(x => x.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var handler = CreateHandler();
        var command = new ConfirmDepositPaymentCommand
        {
            ContractId = contract.Id,
            ConfirmedBy = Guid.NewGuid()
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*AwaitingPayment*");
    }

    [Fact]
    public async Task Handle_ShouldActivateContractAndCreateStore_WhenPaymentConfirmed()
    {
        var contract = BuildContract(ContractStatus.AwaitingPayment);
        Store? createdStore = null;
        UserInfo? updatedUser = null;

        _contractRepoMock
            .Setup(x => x.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        _uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _storeRepoMock
            .Setup(x => x.AddOnlyAsync(It.IsAny<Store>()))
            .Callback<Store>(s => createdStore = s)
            .Returns(Task.CompletedTask);

        _identityClientMock
            .Setup(x => x.UpdateUserAsync(It.IsAny<UserInfo>(), It.IsAny<CancellationToken>()))
            .Callback<UserInfo, CancellationToken>((user, _) => updatedUser = user)
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();
        var command = new ConfirmDepositPaymentCommand
        {
            ContractId = contract.Id,
            ConfirmedBy = Guid.NewGuid(),
            Note = "Da nhan coc sua da"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        contract.Status.Should().Be(ContractStatus.Active);
        contract.DepositStatus.Should().Be(DepositStatus.Paid);

        createdStore.Should().NotBeNull();
        createdStore!.Name.Should().Be(contract.Application.StoreName);
        createdStore.Address.Should().Be(contract.Application.StoreAddress);
        createdStore.Type.Should().Be((int)StoreType.Franchise);
        createdStore.Status.Should().Be((int)StoreStatus.ComingSoon);

        contract.Application.StoreId.Should().Be(createdStore.Id);

        updatedUser.Should().NotBeNull();
        updatedUser!.Id.Should().Be(contract.Application.ManagerId);
        updatedUser.RoleSlug.Should().Be("manager");
        updatedUser.HomeStoreId.Should().Be(createdStore.Id);

        _uowMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRollbackAndThrow_WhenUpdateUserFails()
    {
        var contract = BuildContract(ContractStatus.AwaitingPayment);

        _contractRepoMock
            .Setup(x => x.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        _uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _storeRepoMock
            .Setup(x => x.AddOnlyAsync(It.IsAny<Store>()))
            .Returns(Task.CompletedTask);

        _identityClientMock
            .Setup(x => x.UpdateUserAsync(It.IsAny<UserInfo>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Identity service unavailable"));

        var handler = CreateHandler();
        var command = new ConfirmDepositPaymentCommand
        {
            ContractId = contract.Id,
            ConfirmedBy = Guid.NewGuid()
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Identity service unavailable*");

        _uowMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private ConfirmDepositPaymentHandler CreateHandler()
    {
        return new ConfirmDepositPaymentHandler(
            _uowMock.Object,
            _contractRepoMock.Object,
            _storeRepoMock.Object,
            _identityClientMock.Object,
            _loggerMock.Object);
    }

    private static FranchiseContract BuildContract(ContractStatus status)
    {
        return new FranchiseContract
        {
            Id = Guid.NewGuid(),
            Status = status,
            DepositStatus = DepositStatus.AwaitingPayment,
            Application = new FranchiseApplication
            {
                Id = Guid.NewGuid(),
                ManagerId = Guid.NewGuid(),
                StoreName = "Arabica Signature",
                StoreAddress = "21 Nguyen Robusta",
                Phone = "0909333444",
                Email = "manager@arabica.vn",
                StoreFrontUrl = "https://cdn.cafe.vn/store/arabica-signature.jpg",
                StoreLatitude = 10.77m,
                StoreLongitude = 106.68m,
                Contracts = new List<FranchiseContract>()
            }
        };
    }
}
