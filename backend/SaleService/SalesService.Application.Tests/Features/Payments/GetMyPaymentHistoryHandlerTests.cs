using Moq;
using FluentAssertions;
using SalesService.Application.Features.Payments.GetMyPaymentHistory;
using SalesService.Application.Interfaces.Payments;
using BuildingBlocks.Core.Pagination;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace SalesService.Application.Tests.Features.Payments.GetMyPaymentHistory;

public class GetMyPaymentHistoryHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock = new();
    private readonly GetMyPaymentHistoryHandler _handler;

    public GetMyPaymentHistoryHandlerTests()
    {
        _handler = new GetMyPaymentHistoryHandler(_paymentRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResults_WhenSuccessful()
    {
        var customerId = Guid.NewGuid();
        var query = new GetMyPaymentHistoryQuery 
        { 
            CustomerId = customerId, 
            Page = 1, 
            PageSize = 10 
        };

        var payments = new List<Payment>
        {
            new Payment 
            { 
                Id = Guid.NewGuid(), 
                OrderId = Guid.NewGuid(), 
                Amount = 100, 
                Currency = "VND", 
                Method = (int)PaymentMethod.VNPay, 
                Status = (int)PaymentStatus.Paid,
                PaidAt = DateTime.UtcNow,
                Order = new Order { Code = "ORD-001" }
            }
        };

        var pagedResult = PagedResult<Payment>.Create(payments, 1, 1, 10);

        _paymentRepoMock.Setup(x => x.GetMyPaymentHistoryAsync(
            customerId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        var item = result.Items.First();
        item.OrderCode.Should().Be("ORD-001");
        item.PaymentMethod.Should().Be("VNPay");
    }

    [Fact]
    public async Task Handle_ShouldUseDefaultPaging_WhenInvalidInput()
    {
        var customerId = Guid.NewGuid();
        var query = new GetMyPaymentHistoryQuery 
        { 
            CustomerId = customerId, 
            Page = 0, 
            PageSize = 0 
        };

        _paymentRepoMock.Setup(x => x.GetMyPaymentHistoryAsync(
            customerId, 1, 10, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult<Payment>.Create(new List<Payment>(), 0, 1, 10));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}
