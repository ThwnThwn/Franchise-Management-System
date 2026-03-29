using FluentAssertions;
using FranchiseService.Application.Features.FranchiseContracts.Commands.UpdateContractStatus;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace FranchiseService.Application.Tests.Features.FranchiseContracts;

public class UpdateContractStatusHandlerTests
{
    private readonly Mock<IFranchiseContractRepository> _repoMock = new();
    private readonly Mock<ILogger<UpdateContractStatusHandler>> _loggerMock = new();

    [Fact]
    public async Task Handle_ShouldThrow_WhenContractNotFound()
    {
        var contractId = Guid.NewGuid();
        _repoMock.Setup(x => x.GetByIdAsync(contractId)).ReturnsAsync((FranchiseContract?)null);

        var handler = new UpdateContractStatusHandler(_repoMock.Object, _loggerMock.Object);
        var act = () => handler.Handle(
            new UpdateContractStatusCommand { ContractId = contractId, Status = ContractStatus.Suspended },
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCurrentStatusIsEnded()
    {
        var contract = BuildContract(ContractStatus.Terminated);
        _repoMock.Setup(x => x.GetByIdAsync(contract.Id)).ReturnsAsync(contract);

        var handler = new UpdateContractStatusHandler(_repoMock.Object, _loggerMock.Object);
        var act = () => handler.Handle(
            new UpdateContractStatusCommand { ContractId = contract.Id, Status = ContractStatus.Active },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenSuspendNonActiveContract()
    {
        var contract = BuildContract(ContractStatus.AwaitingPayment);
        _repoMock.Setup(x => x.GetByIdAsync(contract.Id)).ReturnsAsync(contract);

        var handler = new UpdateContractStatusHandler(_repoMock.Object, _loggerMock.Object);
        var act = () => handler.Handle(
            new UpdateContractStatusCommand { ContractId = contract.Id, Status = ContractStatus.Suspended },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Active*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenActivateNonSuspendedContract()
    {
        var contract = BuildContract(ContractStatus.Active);
        _repoMock.Setup(x => x.GetByIdAsync(contract.Id)).ReturnsAsync(contract);

        var handler = new UpdateContractStatusHandler(_repoMock.Object, _loggerMock.Object);
        var act = () => handler.Handle(
            new UpdateContractStatusCommand { ContractId = contract.Id, Status = ContractStatus.Active },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Suspended*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTerminateFromInvalidStatus()
    {
        var contract = BuildContract(ContractStatus.AwaitingSignature);
        _repoMock.Setup(x => x.GetByIdAsync(contract.Id)).ReturnsAsync(contract);

        var handler = new UpdateContractStatusHandler(_repoMock.Object, _loggerMock.Object);
        var act = () => handler.Handle(
            new UpdateContractStatusCommand { ContractId = contract.Id, Status = ContractStatus.Terminated },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Active hoặc Suspended*");
    }

    [Fact]
    public async Task Handle_ShouldUpdateStatus_WhenTransitionIsValid()
    {
        var contract = BuildContract(ContractStatus.Suspended);

        _repoMock.Setup(x => x.GetByIdAsync(contract.Id)).ReturnsAsync(contract);
        _repoMock.Setup(x => x.UpdateAsync(contract)).ReturnsAsync(1);

        var handler = new UpdateContractStatusHandler(_repoMock.Object, _loggerMock.Object);
        var result = await handler.Handle(
            new UpdateContractStatusCommand
            {
                ContractId = contract.Id,
                Status = ContractStatus.Active,
                Reason = "Da xu ly xong vi pham ca phe"
            },
            CancellationToken.None);

        result.Should().BeTrue();
        contract.Status.Should().Be(ContractStatus.Active);
        _repoMock.Verify(x => x.UpdateAsync(contract), Times.Once);
    }

    private static FranchiseContract BuildContract(ContractStatus status)
    {
        return new FranchiseContract
        {
            Id = Guid.NewGuid(),
            Code = "FR-STATUS-001",
            Status = status,
            Application = new FranchiseApplication { Contracts = new List<FranchiseContract>() }
        };
    }
}


