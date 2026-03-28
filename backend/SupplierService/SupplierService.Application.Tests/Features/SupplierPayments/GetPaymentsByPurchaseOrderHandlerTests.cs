using FluentAssertions;
using Moq;
using SupplierService.Application.Features.SupplierPayments.Queries.GetPaymentsByPurchaseOrder;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Tests.Features.SupplierPayments;

public class GetPaymentsByPurchaseOrderHandlerTests
{
    private readonly Mock<ISupplierPaymentRepository> _paymentRepoMock;
    private readonly GetPaymentsByPurchaseOrderHandler _handler;

    public GetPaymentsByPurchaseOrderHandlerTests()
    {
        _paymentRepoMock = new Mock<ISupplierPaymentRepository>();
        _handler = new GetPaymentsByPurchaseOrderHandler(_paymentRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoPaymentsExistForPurchaseOrder()
    {
        var poId = Guid.NewGuid();
        var query = new GetPaymentsByPurchaseOrderQuery(poId);

        _paymentRepoMock
            .Setup(repo => repo.GetByPurchaseOrderIdAsync(poId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Payments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedPayments_WhenDataExists()
    {
        var poId = Guid.NewGuid();
        var query = new GetPaymentsByPurchaseOrderQuery(poId);

        var paymentId1 = Guid.NewGuid();
        var paymentId2 = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-2);
        var paidAt = DateTimeOffset.UtcNow.AddDays(-1);

        var mockPayments = new List<SupplierPayment>
        {
            new()
            {
                Id = paymentId1,
                PurchaseOrderId = poId,
                Code = "SPAY-CAFEDEN-001",
                Amount = 1500000m,
                Method = (int)SupplierPaymentMethod.Cash,
                Status = (int)PaymentStatus.Paid,
                Notes = "Thanh toán đợt 1 tiền cà phê hạt nguyên chất",
                PaidAt = paidAt,
                CreatedAt = createdAt
            },
            new()
            {
                Id = paymentId2,
                PurchaseOrderId = poId,
                Code = "SPAY-SUATUOI-002",
                Amount = 500000m,
                Method = (int)SupplierPaymentMethod.VNPay,
                Status = (int)PaymentStatus.Pending,
                Notes = null,
                PaidAt = null,
                CreatedAt = createdAt
            }
        };

        _paymentRepoMock
            .Setup(repo => repo.GetByPurchaseOrderIdAsync(poId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPayments);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Payments.Should().HaveCount(2);

        var firstPayment = result.Payments.First(p => p.Id == paymentId1.ToString());
        firstPayment.PurchaseOrderId.Should().Be(poId.ToString());
        firstPayment.Code.Should().Be("SPAY-CAFEDEN-001");
        firstPayment.Amount.Should().Be(1500000);
        firstPayment.Method.Should().Be((int)SupplierPaymentMethod.Cash);
        firstPayment.Status.Should().Be((int)PaymentStatus.Paid);
        firstPayment.Notes.Should().Be("Thanh toán đợt 1 tiền cà phê hạt nguyên chất");
        firstPayment.PaidAt.Should().Be(paidAt.ToString("O"));
        firstPayment.CreatedAt.Should().Be(createdAt.ToString("O"));

        var secondPayment = result.Payments.First(p => p.Id == paymentId2.ToString());
        secondPayment.PurchaseOrderId.Should().Be(poId.ToString());
        secondPayment.Code.Should().Be("SPAY-SUATUOI-002");
        secondPayment.Amount.Should().Be(500000);
        secondPayment.Method.Should().Be((int)SupplierPaymentMethod.VNPay);
        secondPayment.Status.Should().Be((int)PaymentStatus.Pending);
        secondPayment.Notes.Should().BeNull();
        secondPayment.PaidAt.Should().BeNull();
        secondPayment.CreatedAt.Should().Be(createdAt.ToString("O"));
    }
}