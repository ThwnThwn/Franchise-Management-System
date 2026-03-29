using FluentAssertions;
using Moq;
using SalesService.Application.Features.WithdrawalRequests.Commands.ApproveWithdrawal;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Tests.Features.WithdrawalRequests;

public class ApproveWithdrawalHandlerTest
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IWithdrawalRequestRepository> _repo = new();
    private readonly ApproveWithdrawalHandler _handler;

    public ApproveWithdrawalHandlerTest()
    {
        _handler = new ApproveWithdrawalHandler(_uow.Object, _repo.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenRequestDoesNotExist()
    {
        var requestId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var cmd = new ApproveWithdrawalCommand(requestId, adminId, "Notes");

        _repo.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WithdrawalRequest?)null);

        var act = () => _handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenStatusNotPending()
    {
        var requestId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var cmd = new ApproveWithdrawalCommand(requestId, adminId, "Notes");
        var withdrawal = new WithdrawalRequest { Id = requestId, Status = WithdrawalStatus.Completed };

        _repo.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withdrawal);

        var act = () => _handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldApprove_WhenValid()
    {
        var requestId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var cmd = new ApproveWithdrawalCommand(requestId, adminId, "Notes");
        var withdrawal = new WithdrawalRequest { Id = requestId, Status = WithdrawalStatus.Pending };

        _repo.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withdrawal);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Should().BeTrue();
        _repo.Verify(x => x.ApplyApprove(withdrawal, adminId, "Notes"), Times.Once);
        _uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
