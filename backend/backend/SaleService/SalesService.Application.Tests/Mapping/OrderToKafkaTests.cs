using FluentAssertions;
using SalesService.Application.Mapping.Orders;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SharedKernel.Events;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SalesService.Application.Tests.Mapping;

public class OrderToKafkaTests
{
    [Fact]
    public void MapOrderStatus_Should_Map_All_Statuses()
    {
        OrderToKafka.MapOrderStatus(OrderStatus.PaymentPending).Should().Be(OrderStatusEvent.OrderStatus.PaymentPending);
        OrderToKafka.MapOrderStatus(OrderStatus.ConfirmationPending).Should().Be(OrderStatusEvent.OrderStatus.ConfirmationPending);
        OrderToKafka.MapOrderStatus(OrderStatus.Confirmed).Should().Be(OrderStatusEvent.OrderStatus.Confirmed);
        OrderToKafka.MapOrderStatus(OrderStatus.Preparing).Should().Be(OrderStatusEvent.OrderStatus.Preparing);
        OrderToKafka.MapOrderStatus(OrderStatus.ReadyForPickup).Should().Be(OrderStatusEvent.OrderStatus.ReadyForPickup);
        OrderToKafka.MapOrderStatus(OrderStatus.Delivering).Should().Be(OrderStatusEvent.OrderStatus.Delivering);
        OrderToKafka.MapOrderStatus(OrderStatus.Completed).Should().Be(OrderStatusEvent.OrderStatus.Completed);
        OrderToKafka.MapOrderStatus(OrderStatus.Cancelled).Should().Be(OrderStatusEvent.OrderStatus.Cancelled);
    }

    [Fact]
    public void MapOrderStatus_Should_Throw_On_Invalid_Status()
    {
        Action act = () => OrderToKafka.MapOrderStatus((OrderStatus)999);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void MapToOrderPaidEvent_Should_Map_Correctly()
    {
        var orderId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var order = new Order 
        { 
            Id = orderId, 
            StoreId = storeId,
            OrderItems = new List<OrderItem> { new OrderItem { VariantId = variantId, Quantity = 2 } }
        };

        var result = OrderToKafka.MapToOrderPaidEvent(order);

        result.OrderId.Should().Be(orderId);
        result.StoreId.Should().Be(storeId);
        result.Items.Should().HaveCount(1);
        result.Items.First().VariantId.Should().Be(variantId);
        result.Items.First().Quantity.Should().Be(2);
    }

    [Fact]
    public void MapToOrderStatusEvent_Should_Map_Correctly()
    {
        var order = new Order 
        { 
            Id = Guid.NewGuid(), 
            Code = "ORD-1", 
            Status = OrderStatus.Completed,
            OrderItems = new List<OrderItem> { new OrderItem { Sku = "SKU-1", Quantity = 1, UnitPrice = 10 } }
        };

        var result = OrderToKafka.MapToOrderStatusEvent(order);

        result.OrderId.Should().Be(order.Id);
        result.OrderCode.Should().Be("ORD-1");
        result.NewOrderStatus.Should().Be(OrderStatusEvent.OrderStatus.Completed);
        result.Items.Should().HaveCount(1);
        result.Items.First().Sku.Should().Be("SKU-1");
    }
}
