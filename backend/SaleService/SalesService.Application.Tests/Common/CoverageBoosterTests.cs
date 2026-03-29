using FluentAssertions;
using SalesService.Application.Helpers;
using SalesService.Application.Features.Orders.Queries.GetMyOrders;
using SalesService.Application.Common.Models;
using SalesService.Application.Features.Orders.Commands.UpdateOrderStatus;
using SalesService.Application.Mapping.Orders;
using AutoMapper;
using SalesService.Application.Features.Orders.Commands.Create;
using SalesService.Domain.Entities;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Tests.Common;

public class CoverageBoosterTests
{
    [Fact]
    public async Task VNPayHelper_CallRefundApiAsync_Should_Handle_Timeout()
    {
        var parameters = new Dictionary<string, string> { { "vnp_Amount", "100" } };
        // Passing a URL that doesn't exist should trigger HttpRequestException (which is caught)
        var result = await VNPayHelper.CallRefundApiAsync("http://invalid.url.that.doesnot.exist", "secret", parameters);
        
        result.Should().ContainKey("vnp_ResponseCode");
        result["vnp_ResponseCode"].Should().Be("99");
        result["vnp_Message"].Should().Contain("HTTP error");
    }

    [Fact]
    public void OrderSummaryDto_Should_Be_Covered()
    {
        var id = Guid.NewGuid();
        var dto = new OrderSummaryDto(
            id, "ORD-1", 100, 10, 5, 115, 0, 1, 1, 1, DateTimeOffset.UtcNow, "Notes", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TimeOnly.MinValue, "Addr", 0, 0, 0
        );

        var dto2 = dto with { Code = "ORD-2" };

        dto.Id.Should().Be(id);
        dto.Code.Should().Be("ORD-1");
        dto.Equals(dto2).Should().BeFalse();
        dto.ToString().Should().NotBeNull();
        dto.GetHashCode().Should().NotBe(0);
    }

    [Fact]
    public void InternalUserOverrideInfo_Should_Be_Covered()
    {
        var info = new InternalUserOverrideInfo
        {
            InternalUserId = Guid.NewGuid(),
            AutoApprove = true,
            BypassTimeLimit = true,
            BypassAmountValidation = true,
            RefundReason = SalesService.Domain.Enums.RefundReasonType.Compensation,
            InternalNotes = "Notes"
        };

        info.InternalUserId.Should().NotBe(Guid.Empty);
        info.AutoApprove.Should().BeTrue();
        info.BypassTimeLimit.Should().BeTrue();
        info.BypassAmountValidation.Should().BeTrue();
        info.RefundReason.Should().Be(SalesService.Domain.Enums.RefundReasonType.Compensation);
        info.InternalNotes.Should().Be("Notes");
    }

    [Fact]
    public void DictionaryOrderTileEmail_Should_Be_Covered()
    {
        DictionaryOrderTileEmail.GetTitle(SalesService.Domain.Enums.OrderStatus.Completed).Should().Be("Thank you for choosing us!");
        DictionaryOrderTileEmail.GetTitle((SalesService.Domain.Enums.OrderStatus)999).Should().Be("Order Status update");
    }

    [Fact]
    public void OrderMappingProfile_Should_Be_Valid()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<OrderMappingProfile>());
        
        var mapper = configuration.CreateMapper();
        var order = new Order { Id = Guid.NewGuid(), Code = "ORD-TEST", TotalAmount = 100 };
        var result = mapper.Map<CreateOrderResponseDto>(order);
        
        result.OrderId.Should().Be(order.Id);
        result.OrderCode.Should().Be(order.Code);
    }
}
