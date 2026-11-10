using Ardalis.Result;
using FluentAssertions;
using Moq;
using SalesService.Application.Features.RefundTickets.Queries.GetById;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Tests.Features.RefundTickets;

public class GetRefundByIdQueryHandlerTest
{
    private readonly Mock<IRefundTicketRepository> _refundRepo = new();
    private readonly Mock<IIdentityServiceClient> _identityClient = new();
    private readonly GetRefundByIdQueryHandler _handler;

    public GetRefundByIdQueryHandlerTest()
    {
        _handler = new GetRefundByIdQueryHandler(_refundRepo.Object, _identityClient.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_Ticket_NotExist()
    {
        var ticketId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _refundRepo.Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefundTicket?)null);

        var query = new GetRefundByIdQuery(ticketId, userId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ThrowArgumentException_When_Ticket_Invalid()
    {
        var ticketId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ticket = new RefundTicket { Id = ticketId, WalletId = null }; // Invalid because WalletId is null
        _refundRepo.Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var query = new GetRefundByIdQuery(ticketId, userId);
        
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_Valid()
    {
        var ticketId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var origTransId = Guid.NewGuid();
        var ticket = new RefundTicket 
        { 
            Id = ticketId, 
            TicketCode = "REF1", 
            Amount = 100, 
            WalletId = walletId, 
            OriginalTransactionId = origTransId,
            Status = RefundTicketStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _refundRepo.Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var query = new GetRefundByIdQuery(ticketId, userId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TicketCode.Should().Be("REF1");
        result.Value.WalletId.Should().Be(walletId);
    }
}
