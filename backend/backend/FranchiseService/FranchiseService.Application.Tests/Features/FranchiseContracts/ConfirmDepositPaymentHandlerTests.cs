using FluentAssertions;
using FranchiseService.Application.Features.FranchiseContracts.Commands.ConfirmDepositPayment;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace FranchiseService.Application.Tests.Features.FranchiseContracts;

public class ConfirmDepositPaymentHandlerTests
{
    private readonly Mock<IFranchiseContractRepository> _contractRepoMock;
    private readonly Mock<ILogger<ConfirmDepositPaymentHandler>> _loggerMock;
    private readonly ConfirmDepositPaymentHandler _handler;

    public ConfirmDepositPaymentHandlerTests()
    {
        _contractRepoMock = new Mock<IFranchiseContractRepository>();
        _loggerMock = new Mock<ILogger<ConfirmDepositPaymentHandler>>();

        _handler = new ConfirmDepositPaymentHandler(
            _contractRepoMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenContractIsNotFound_ThrowsKeyNotFoundException()
    {
        var command = new ConfirmDepositPaymentCommand
        {
            ContractId = Guid.NewGuid(),
            ConfirmedBy = Guid.NewGuid(),
            Note = "Thu tiền cọc nhượng quyền chi nhánh Arabica Premium"
        };

        _contractRepoMock
            .Setup(x => x.GetByIdAsync(command.ContractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FranchiseContract?)null);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{command.ContractId}*");
    }

    [Fact]
    public async Task Handle_WhenContractStatusIsNotAwaitingPayment_ThrowsInvalidOperationException()
    {
        var contract = new FranchiseContract
        {
            Id = Guid.NewGuid(),
            Status = ContractStatus.AwaitingSignature,
            DepositStatus = DepositStatus.AwaitingPayment
        };

        var command = new ConfirmDepositPaymentCommand
        {
            ContractId = contract.Id,
            ConfirmedBy = Guid.NewGuid(),
            Note = "Thanh toán phí nhượng quyền quán Cà Phê Muối"
        };

        _contractRepoMock
            .Setup(x => x.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*chỉ có thể xác nhận thanh toán khi ở trạng thái Chờ thanh toán*");
    }

    [Fact]
    public async Task Handle_WhenValidRequest_UpdatesContractStatusAndReturnsTrue()
    {
        var contract = new FranchiseContract
        {
            Id = Guid.NewGuid(),
            Status = ContractStatus.AwaitingPayment,
            DepositStatus = DepositStatus.AwaitingPayment
        };

        var command = new ConfirmDepositPaymentCommand
        {
            ContractId = contract.Id,
            ConfirmedBy = Guid.NewGuid(),
            Note = "Đã thu đủ 50tr tiền cọc mở quán Trà Đào Cam Sả"
        };

        _contractRepoMock
            .Setup(x => x.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        contract.Status.Should().Be(ContractStatus.Active);
        contract.DepositStatus.Should().Be(DepositStatus.Paid);

        _contractRepoMock.Verify(x => x.UpdateAsync(It.Is<FranchiseContract>(c =>
            c.Id == contract.Id &&
            c.Status == ContractStatus.Active &&
            c.DepositStatus == DepositStatus.Paid
        )), Times.Once);
    }
}