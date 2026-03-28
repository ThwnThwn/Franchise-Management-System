using FluentAssertions;
using Moq;
using SupplierService.Application.Features.SupplierPayments.Queries.GetAllSupplierPayments;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Tests.Features.SupplierPayments;

public class GetAllSupplierPaymentsHandlerTests
{
    private readonly Mock<ISupplierPaymentRepository> _paymentRepoMock;
    private readonly GetAllSupplierPaymentsHandler _handler;

    public GetAllSupplierPaymentsHandlerTests()
    {
        _paymentRepoMock = new Mock<ISupplierPaymentRepository>();
        _handler = new GetAllSupplierPaymentsHandler(_paymentRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldApplyDefaultPagination_AndMapCorrectly_WhenInputsAreEmpty()
    {
        var query = new GetAllSupplierPaymentsQuery(0, -5, null, null);

        var paymentId = Guid.NewGuid();
        var poId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var paidAt = DateTimeOffset.UtcNow;

        var mockPayments = new List<SupplierPayment>
        {
            new()
            {
                Id = paymentId,
                PurchaseOrderId = poId,
                PurchaseOrder = new PurchaseOrder { Id = poId, Code = "PO-CAFEDEN-001", Items = [] },
                Code = "SPAY-001",
                Amount = 1500000m,
                Method = (int)SupplierPaymentMethod.Cash,
                Status = (int)PaymentStatus.Paid,
                PaidAt = paidAt,
                CreatedAt = createdAt
            },
            new()
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = Guid.NewGuid(),
                PurchaseOrder = null,
                Code = "SPAY-002",
                Amount = 500000m,
                Method = (int)SupplierPaymentMethod.VNPay,
                Status = (int)PaymentStatus.Pending,
                PaidAt = null,
                CreatedAt = createdAt
            }
        };

        _paymentRepoMock
            .Setup(repo => repo.GetAllAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockPayments, 2));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Payments.Should().HaveCount(2);

        var first = result.Payments.First();
        first.Id.Should().Be(paymentId.ToString());
        first.PurchaseOrderId.Should().Be(poId.ToString());
        first.PurchaseOrderCode.Should().Be("PO-CAFEDEN-001");
        first.Code.Should().Be("SPAY-001");
        first.Amount.Should().Be(1500000);
        first.Method.Should().Be((int)SupplierPaymentMethod.Cash);
        first.Status.Should().Be((int)PaymentStatus.Paid);
        first.PaidAt.Should().Be(paidAt.ToString("O"));
        first.CreatedAt.Should().Be(createdAt.ToString("O"));

        var second = result.Payments.Last();
        second.PurchaseOrderCode.Should().BeEmpty();
        second.PaidAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectParameters_WhenValidInputsProvided()
    {
        var supplierId = Guid.NewGuid();
        var poId = Guid.NewGuid();
        var query = new GetAllSupplierPaymentsQuery(3, 20, supplierId.ToString(), poId.ToString());

        _paymentRepoMock
            .Setup(repo => repo.GetAllAsync(3, 20, supplierId, poId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(([], 0));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Payments.Should().BeEmpty();

        _paymentRepoMock.Verify(repo => repo.GetAllAsync(3, 20, supplierId, poId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFallbackToNullIds_WhenStringsAreInvalidGuids()
    {
        var query = new GetAllSupplierPaymentsQuery(1, 10, "khong-phai-guid", "chuoi-loi");

        _paymentRepoMock
            .Setup(repo => repo.GetAllAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(([], 0));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Payments.Should().BeEmpty();

        _paymentRepoMock.Verify(repo => repo.GetAllAsync(1, 10, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}