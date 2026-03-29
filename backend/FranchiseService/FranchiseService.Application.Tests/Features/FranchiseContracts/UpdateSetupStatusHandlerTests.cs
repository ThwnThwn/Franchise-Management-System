using FluentAssertions;
using FranchiseService.Application.Features.FranchiseContracts.Commands.UpdateSetupStatus;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace FranchiseService.Application.Tests.Features.FranchiseContracts;

public class UpdateSetupStatusHandlerTests
{
    private readonly Mock<IFranchiseContractRepository> _repoMock = new();
    private readonly Mock<ILogger<UpdateSetupStatusHandler>> _loggerMock = new();

    [Fact]
    public async Task Handle_ShouldThrow_WhenContractNotFound()
    {
        var contractId = Guid.NewGuid();
        _repoMock.Setup(x => x.GetByIdAsync(contractId)).ReturnsAsync((FranchiseContract?)null);

        var handler = new UpdateSetupStatusHandler(_repoMock.Object, _loggerMock.Object);
        var act = () => handler.Handle(
            new UpdateSetupStatusCommand
            {
                ContractId = contractId,
                SetupStatus = ContractSetupStatus.Construction,
                UpdatedBy = Guid.NewGuid()
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenContractIsNotActive()
    {
        var contract = BuildContract(ContractStatus.AwaitingPayment);
        _repoMock.Setup(x => x.GetByIdAsync(contract.Id)).ReturnsAsync(contract);

        var handler = new UpdateSetupStatusHandler(_repoMock.Object, _loggerMock.Object);
        var act = () => handler.Handle(
            new UpdateSetupStatusCommand
            {
                ContractId = contract.Id,
                SetupStatus = ContractSetupStatus.Inspection,
                UpdatedBy = Guid.NewGuid()
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Active*");
    }

    [Fact]
    public async Task Handle_ShouldUpdateSetupStatus_WhenContractIsActive()
    {
        var contract = BuildContract(ContractStatus.Active);

        _repoMock.Setup(x => x.GetByIdAsync(contract.Id)).ReturnsAsync(contract);
        _repoMock.Setup(x => x.UpdateAsync(contract)).ReturnsAsync(1);

        var handler = new UpdateSetupStatusHandler(_repoMock.Object, _loggerMock.Object);
        var result = await handler.Handle(
            new UpdateSetupStatusCommand
            {
                ContractId = contract.Id,
                SetupStatus = ContractSetupStatus.Ready,
                UpdatedBy = Guid.NewGuid()
            },
            CancellationToken.None);

        result.Should().BeTrue();
        contract.SetupStatus.Should().Be(ContractSetupStatus.Ready);
        _repoMock.Verify(x => x.UpdateAsync(contract), Times.Once);
    }

    private static FranchiseContract BuildContract(ContractStatus status)
    {
        return new FranchiseContract
        {
            Id = Guid.NewGuid(),
            Code = "FR-SETUP-2026",
            Status = status,
            SetupStatus = ContractSetupStatus.Design,
            Application = new FranchiseApplication { Contracts = new List<FranchiseContract>() }
        };
    }
}
