using Moq;
using FluentAssertions;
using SalesService.Application.Features.Payments.GetAll;
using SalesService.Application.Interfaces.Payments;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using BuildingBlocks.Core.Pagination;

namespace SalesService.Application.Tests.Features.Payments;

public class GetAllPaymentsHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly GetAllPaymentsHandler _handler;

    public GetAllPaymentsHandlerTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _handler = new GetAllPaymentsHandler(_paymentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedPayments_WhenDataExists()
    {
        // Arrange
        var query = new GetAllPaymentsQuery { Page = 1, PageSize = 10 };

        var payments = new List<Payment>
        {
            new Payment { Id = Guid.NewGuid(), OrderId = Guid.NewGuid(), Code = "PAY1", Amount = 100, Currency = "VND", Method = (int)PaymentMethod.Cash, Status = (int)PaymentStatus.Paid },
            new Payment { Id = Guid.NewGuid(), OrderId = Guid.NewGuid(), Code = "PAY2", Amount = 200, Currency = "VND", Method = (int)PaymentMethod.VNPay, Status = (int)PaymentStatus.Pending }
        };

        var pagedData = PagedResult<Payment>.Create(payments, 2, 1, 10);

        _paymentRepositoryMock.Setup(x => x.GetAllAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedData);

        // Act aaa
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.First().Code.Should().Be("PAY1");
        result.Items.Last().Code.Should().Be("PAY2");

        _paymentRepositoryMock.Verify(x => x.GetAllAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoDataExists()
    {
        // Arrange
        var query = new GetAllPaymentsQuery { Page = 1, PageSize = 10 };
        var pagedData = PagedResult<Payment>.Create(new List<Payment>(), 0, 1, 10);

        _paymentRepositoryMock.Setup(x => x.GetAllAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedData);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
}
