using FluentAssertions;
using FranchiseService.Application.Features.FranchiseContracts.Commands.RenewContract;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace FranchiseService.Application.Tests.Features.FranchiseContracts;

public class RenewContractHandlerTests
{
    private readonly Mock<IFranchiseContractRepository> _repoMock = new();
    private readonly Mock<ILogger<RenewContractHandler>> _loggerMock = new();

    [Fact]
    public async Task Handle_ShouldThrow_WhenExtensionYearsIsNotPositive()
    {
        var handler = new RenewContractHandler(_repoMock.Object, _loggerMock.Object);

        var act = () => handler.Handle(
            new RenewContractCommand { ContractId = Guid.NewGuid(), ExtensionYears = 0 },
            CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenContractNotFound()
    {
        var contractId = Guid.NewGuid();
        _repoMock
            .Setup(x => x.GetByIdAsync(contractId))
            .ReturnsAsync((FranchiseContract?)null);

        var handler = new RenewContractHandler(_repoMock.Object, _loggerMock.Object);
        var act = () => handler.Handle(
            new RenewContractCommand { ContractId = contractId, ExtensionYears = 2 },
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenContractIsTerminated()
    {
        var contract = BuildContract(ContractStatus.Terminated, DateTimeOffset.UtcNow.AddMonths(2));
        _repoMock.Setup(x => x.GetByIdAsync(contract.Id)).ReturnsAsync(contract);

        var handler = new RenewContractHandler(_repoMock.Object, _loggerMock.Object);
        var act = () => handler.Handle(
            new RenewContractCommand { ContractId = contract.Id, ExtensionYears = 1 },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenContractHasNoValidUntil()
    {
        var contract = BuildContract(ContractStatus.Active, null);
        _repoMock.Setup(x => x.GetByIdAsync(contract.Id)).ReturnsAsync(contract);

        var handler = new RenewContractHandler(_repoMock.Object, _loggerMock.Object);
        var act = () => handler.Handle(
            new RenewContractCommand { ContractId = contract.Id, ExtensionYears = 1 },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldExtendAndReactivate_WhenContractIsExpired()
    {
        var oldValidUntil = new DateTimeOffset(2026, 6, 30, 0, 0, 0, TimeSpan.Zero);
        var contract = BuildContract(ContractStatus.Expired, oldValidUntil);

        _repoMock.Setup(x => x.GetByIdAsync(contract.Id)).ReturnsAsync(contract);
        _repoMock.Setup(x => x.UpdateAsync(contract)).ReturnsAsync(1);

        var handler = new RenewContractHandler(_repoMock.Object, _loggerMock.Object);

        var result = await handler.Handle(
            new RenewContractCommand
            {
                ContractId = contract.Id,
                ExtensionYears = 2,
                Note = "Gia han them cho cua hang Arabica"
            },
            CancellationToken.None);

        result.Should().BeTrue();
        contract.Status.Should().Be(ContractStatus.Active);
        contract.ValidUntil.Should().Be(oldValidUntil.AddYears(2));
        _repoMock.Verify(x => x.UpdateAsync(contract), Times.Once);
    }

    private static FranchiseContract BuildContract(ContractStatus status, DateTimeOffset? validUntil)
    {
        return new FranchiseContract
        {
            Id = Guid.NewGuid(),
            Code = "FR-ROBUSTA-2026",
            Status = status,
            ValidUntil = validUntil,
            Application = new FranchiseApplication { Contracts = new List<FranchiseContract>() }
        };
    }
}
