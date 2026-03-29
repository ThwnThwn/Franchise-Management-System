using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SalesService.Application.Features.Orders.Commands.AssignStaff;
using SalesService.Application.Interfaces.Franchise;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences.Orders;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Tests.Features.Orders;

public class AssignStaffToDeliveryCommandHandlerTest
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IIdentityServiceClient> _identityClient = new();
    private readonly Mock<IFranchiseServiceClient> _franchiseClient = new();
    private readonly Mock<ILogger<AssignStaffToDeliveryCommandHandler>> _logger = new();
    private readonly AssignStaffToDeliveryCommandHandler _handler;

    public AssignStaffToDeliveryCommandHandlerTest()
    {
        _handler = new AssignStaffToDeliveryCommandHandler(
            _orderRepo.Object,
            _identityClient.Object,
            _franchiseClient.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_OrderMissing()
    {
        _orderRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);
        var result = await _handler.Handle(new AssignStaffToDeliveryCommand(Guid.NewGuid(), null), CancellationToken.None);
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_OrderAlreadyDelivering()
    {
        var order = new Order { Status = OrderStatus.Delivering };
        _orderRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(order);
        var result = await _handler.Handle(new AssignStaffToDeliveryCommand(Guid.NewGuid(), null), CancellationToken.None);
        result.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_StaffNotFound()
    {
        var order = new Order { Status = OrderStatus.Confirmed };
        var staffId = Guid.NewGuid();
        _orderRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _identityClient.Setup(x => x.GetUserByIdAsync(staffId)).ReturnsAsync((UserInfo?)null);
        var result = await _handler.Handle(new AssignStaffToDeliveryCommand(Guid.NewGuid(), staffId), CancellationToken.None);
        result.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_StaffNotDeliverer()
    {
        var order = new Order { Status = OrderStatus.Confirmed };
        var staffId = Guid.NewGuid();
        _orderRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _identityClient.Setup(x => x.GetUserByIdAsync(staffId)).ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "admin" } });
        var result = await _handler.Handle(new AssignStaffToDeliveryCommand(Guid.NewGuid(), staffId), CancellationToken.None);
        result.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_StaffCheckedOut()
    {
        var order = new Order { Status = OrderStatus.Confirmed };
        var staffId = Guid.NewGuid();
        _orderRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _identityClient.Setup(x => x.GetUserByIdAsync(staffId)).ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "deliverer" } });
        _franchiseClient.Setup(x => x.GetUserCurrentShiftAsync(staffId)).ReturnsAsync(new UserShiftInfo { HasShiftToday = true, CheckOutAt = "2024-01-01" });
        var result = await _handler.Handle(new AssignStaffToDeliveryCommand(Guid.NewGuid(), staffId), CancellationToken.None);
        result.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_ExplicitStaffValid()
    {
        var order = new Order { Status = OrderStatus.Confirmed, Code = "O1" };
        var staffId = Guid.NewGuid();
        _orderRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _identityClient.Setup(x => x.GetUserByIdAsync(staffId)).ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "deliverer" } });
        _franchiseClient.Setup(x => x.GetUserCurrentShiftAsync(staffId)).ReturnsAsync(new UserShiftInfo 
        { 
            HasShiftToday = true, 
            StartTime = "00:00", 
            EndTime = "23:59" 
        });

        var result = await _handler.Handle(new AssignStaffToDeliveryCommand(Guid.NewGuid(), staffId), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Delivering);
    }

    [Fact]
    public async Task Handle_Should_AutoSelectStaff_When_NoStaffIdProvided()
    {
        var storeId = Guid.NewGuid();
        var order = new Order { Status = OrderStatus.Confirmed, StoreId = storeId, Code = "O2" };
        var staffId = Guid.NewGuid();
        _orderRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _franchiseClient.Setup(x => x.GetActiveStaffsAtStoreAsync(storeId)).ReturnsAsync(new List<Guid> { staffId });
        _identityClient.Setup(x => x.GetUserByIdAsync(staffId)).ReturnsAsync(new UserInfo { Role = new UserRole { Slug = "deliverer" } });

        var result = await _handler.Handle(new AssignStaffToDeliveryCommand(Guid.NewGuid(), null), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        order.DelivererId.Should().Be(staffId);
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_NoStaffFound_In_AutoSelect()
    {
        var storeId = Guid.NewGuid();
        var order = new Order { Status = OrderStatus.Confirmed, StoreId = storeId };
        _orderRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _franchiseClient.Setup(x => x.GetActiveStaffsAtStoreAsync(storeId)).ReturnsAsync(new List<Guid> { Guid.NewGuid() });
        _identityClient.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>())).ReturnsAsync((UserInfo?)null);

        var result = await _handler.Handle(new AssignStaffToDeliveryCommand(Guid.NewGuid(), null), CancellationToken.None);
        result.Status.Should().Be(ResultStatus.Error);
    }
}
