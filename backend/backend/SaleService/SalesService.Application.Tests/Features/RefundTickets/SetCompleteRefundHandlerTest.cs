using Moq;
using FluentAssertions;
using SalesService.Application.Features.RefundTickets.Command.SetCompleteRefund;
using SalesService.Application.Persisitences;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.SetCompleteRefund.SetCompleteRefundCommand;

namespace SalesService.Application.Tests.Features.RefundTickets;

public class SetCompleteRefundHandlerTests
{
    private readonly Mock<IRefundTicketRepository> _refundRepoMock = new();
    private readonly SetCompleteRefundHandler _handler;

    public SetCompleteRefundHandlerTests()
    {
        _handler = new SetCompleteRefundHandler(_refundRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTicketNotFound()
    {
        var cmd = new SetCompleteRefundCommand(new SetCompleteRefundRequest { RefundTicketId = Guid.NewGuid() });
        _refundRepoMock.Setup(x => x.GetByTicketIdAsync(cmd.request.RefundTicketId, It.IsAny<CancellationToken>())).ReturnsAsync((RefundTicket?)null);
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var ticket = new RefundTicket { Id = Guid.NewGuid(), Status = RefundTicketStatus.Approved };
        var request = new SetCompleteRefundRequest { RefundTicketId = ticket.Id, CompleteNotes = "Done" };
        var cmd = new SetCompleteRefundCommand(request);
        
        _refundRepoMock.Setup(x => x.GetByTicketIdAsync(ticket.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(RefundTicketStatus.Completed);
        result.Value.CompleteNotes.Should().Be("Done");
        _refundRepoMock.Verify(x => x.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
    }
}
