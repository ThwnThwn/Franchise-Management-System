using FluentAssertions;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.FranchiseContracts.Commands.UpdateSetupStatus;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace FranchiseService.Application.Tests.Features.FranchiseContracts;

public class UpdateSetupStatusHandlerTests
{
    private readonly Mock<IFranchiseUow> _uowMock;
    private readonly Mock<IFranchiseContractRepository> _contractRepoMock;
    private readonly Mock<IStoreRepository> _storeRepoMock;
    private readonly Mock<IIdentityServiceClient> _identityClientMock;
    private readonly Mock<ILogger<UpdateSetupStatusHandler>> _loggerMock;
    private readonly UpdateSetupStatusHandler _handler;

    public UpdateSetupStatusHandlerTests()
    {
        _uowMock = new Mock<IFranchiseUow>();
        _contractRepoMock = new Mock<IFranchiseContractRepository>();
        _storeRepoMock = new Mock<IStoreRepository>();
        _identityClientMock = new Mock<IIdentityServiceClient>();
        _loggerMock = new Mock<ILogger<UpdateSetupStatusHandler>>();

        _handler = new UpdateSetupStatusHandler(
            _uowMock.Object,
            _contractRepoMock.Object,
            _storeRepoMock.Object,
            _identityClientMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenContractIsNotFound_RollbacksAndThrowsKeyNotFoundException()
    {
        var contractId = Guid.NewGuid();
        var command = new UpdateSetupStatusCommand
        {
            ContractId = contractId,
            SetupStatus = ContractSetupStatus.Construction,
            UpdatedBy = Guid.NewGuid()
        };

        _uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _contractRepoMock.Setup(x => x.GetByIdAsync(contractId)).ReturnsAsync((FranchiseContract?)null);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{contractId}*");
        _uowMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenContractIsNotActive_RollbacksAndThrowsInvalidOperationException()
    {
        var contract = BuildContract(ContractStatus.AwaitingPayment);
        var command = new UpdateSetupStatusCommand
        {
            ContractId = contract.Id,
            SetupStatus = ContractSetupStatus.Inspection,
            UpdatedBy = Guid.NewGuid()
        };

        _uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _contractRepoMock.Setup(x => x.GetByIdAsync(contract.Id)).ReturnsAsync(contract);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*trạng thái Active*");
        _uowMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSetupStatusIsNotReady_UpdatesStatusWithoutCreatingStore()
    {
        var contract = BuildContract(ContractStatus.Active);
        var command = new UpdateSetupStatusCommand
        {
            ContractId = contract.Id,
            SetupStatus = ContractSetupStatus.Construction,
            UpdatedBy = Guid.NewGuid()
        };

        _uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _contractRepoMock.Setup(x => x.GetByIdAsync(contract.Id)).ReturnsAsync(contract);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        contract.SetupStatus.Should().Be(ContractSetupStatus.Construction);

        _storeRepoMock.Verify(x => x.AddOnlyAsync(It.IsAny<Store>()), Times.Never);
        _identityClientMock.Verify(x => x.UpdateUserAsync(It.IsAny<UserInfo>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSetupStatusIsReady_UpdatesStatusCreatesStoreAndAssignsManager()
    {
        var contract = BuildContract(ContractStatus.Active);
        var command = new UpdateSetupStatusCommand
        {
            ContractId = contract.Id,
            SetupStatus = ContractSetupStatus.Ready,
            UpdatedBy = Guid.NewGuid()
        };

        Store? createdStore = null;
        UserInfo? updatedUser = null;

        _uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _contractRepoMock.Setup(x => x.GetByIdAsync(contract.Id)).ReturnsAsync(contract);

        _storeRepoMock.Setup(x => x.AddOnlyAsync(It.IsAny<Store>()))
            .Callback<Store>(s => createdStore = s)
            .Returns(Task.CompletedTask);

        _identityClientMock.Setup(x => x.UpdateUserAsync(It.IsAny<UserInfo>(), It.IsAny<CancellationToken>()))
            .Callback<UserInfo, CancellationToken>((u, _) => updatedUser = u)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        contract.SetupStatus.Should().Be(ContractSetupStatus.Ready);

        createdStore.Should().NotBeNull();
        createdStore!.Name.Should().Be("Cà Phê Cold Brew Signature");
        createdStore.Address.Should().Be("123 Nguyễn Cà Phê, Quận 1");
        createdStore.Type.Should().Be((int)StoreType.Franchise);
        createdStore.Status.Should().Be((int)StoreStatus.Open);
        contract.Application.StoreId.Should().Be(createdStore.Id);

        updatedUser.Should().NotBeNull();
        updatedUser!.Id.Should().Be(contract.Application.ManagerId);
        updatedUser.RoleSlug.Should().Be("manager");
        updatedUser.HomeStoreId.Should().Be(createdStore.Id);

        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenIdentityServiceFails_RollbacksAndRethrowsException()
    {
        var contract = BuildContract(ContractStatus.Active);
        var command = new UpdateSetupStatusCommand
        {
            ContractId = contract.Id,
            SetupStatus = ContractSetupStatus.Ready,
            UpdatedBy = Guid.NewGuid()
        };

        _uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _contractRepoMock.Setup(x => x.GetByIdAsync(contract.Id)).ReturnsAsync(contract);

        _identityClientMock.Setup(x => x.UpdateUserAsync(It.IsAny<UserInfo>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Identity API Timeout"));

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>().WithMessage("Identity API Timeout");
        _uowMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static FranchiseContract BuildContract(ContractStatus status)
    {
        return new FranchiseContract
        {
            Id = Guid.NewGuid(),
            Code = "FR-SETUP-ROBUSTA",
            Status = status,
            SetupStatus = ContractSetupStatus.Design,
            Application = new FranchiseApplication
            {
                Id = Guid.NewGuid(),
                ManagerId = Guid.NewGuid(),
                StoreName = "Cà Phê Cold Brew Signature",
                StoreAddress = "123 Nguyễn Cà Phê, Quận 1",
                Phone = "0909123456",
                Email = "manager@coldbrew.vn",
                StoreFrontUrl = "https://coffee.com/store.jpg",
                StoreLatitude = 10.77m,
                StoreLongitude = 106.69m,
                Contracts = new List<FranchiseContract>()
            }
        };
    }
}