using Ardalis.Result;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SalesService.Application.Features.Orders.Commands.PayWithWallet;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Tests.Features.Orders;

public class PayOrderWithWalletCommandHandlerTest
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IWalletRepository> _walletRepo = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepo = new();
    private readonly PayOrderWithWalletCommandHandler _handler;

    private static readonly Guid SHOP_OWNER_ID = Guid.Parse("98767548-91c3-42ba-94f6-1d21f2292ca6");

    public PayOrderWithWalletCommandHandlerTest()
    {
        _handler = new PayOrderWithWalletCommandHandler(
            _orderRepo.Object,
            _walletRepo.Object,
            _transactionRepo.Object
        );
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_Order_NotExist()
    {
        var orderId = Guid.NewGuid();
        _orderRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _handler.Handle(new PayOrderWithWalletCommand(orderId, Guid.NewGuid()), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnForbidden_When_Customer_NotOwner()
    {
        var order = new Order { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid() };
        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(new PayOrderWithWalletCommand(order.Id, Guid.NewGuid()), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_Order_Completed()
    {
        var custId = Guid.NewGuid();
        var order = new Order { Id = Guid.NewGuid(), CustomerId = custId, Status = OrderStatus.Completed };
        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(new PayOrderWithWalletCommand(order.Id, custId), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_InsufficientBalance()
    {
        var custId = Guid.NewGuid();
        var order = new Order { Id = Guid.NewGuid(), CustomerId = custId, TotalAmount = 1000, Status = OrderStatus.PaymentPending };
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = custId, Balance = 500, Status = WalletStatus.Active };

        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _walletRepo.Setup(x => x.GetByUserIdAsync(custId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(new PayOrderWithWalletCommand(order.Id, custId), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(x => x.Identifier == "InsufficientBalance");
    }

    [Fact]
    public async Task Handle_Should_Success_When_Valid()
    {
        var custId = Guid.NewGuid();
        var order = new Order { Id = Guid.NewGuid(), CustomerId = custId, TotalAmount = 500, Status = OrderStatus.PaymentPending, Code = "ORD1" };
        var custWallet = new Wallet { Id = Guid.NewGuid(), UserId = custId, Balance = 1000, Status = WalletStatus.Active, RowVersion = new byte[] { 1 } };
        var shopWallet = new Wallet { Id = Guid.NewGuid(), UserId = SHOP_OWNER_ID, Balance = 5000, Status = WalletStatus.Active, RowVersion = new byte[] { 2 } };

        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _walletRepo.Setup(x => x.GetByUserIdAsync(custId, It.IsAny<CancellationToken>())).ReturnsAsync(custWallet);
        _walletRepo.Setup(x => x.GetByUserIdAsync(SHOP_OWNER_ID, It.IsAny<CancellationToken>())).ReturnsAsync(shopWallet);
        _walletRepo.Setup(x => x.GetForUpdateAsync(custWallet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(custWallet);
        _walletRepo.Setup(x => x.GetForUpdateAsync(shopWallet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(shopWallet);
        
        _walletRepo.Setup(x => x.UpdateBalanceAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(new PayOrderWithWalletCommand(order.Id, custId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerBalanceAfter.Should().Be(500);
        result.Value.ShopBalanceAfter.Should().Be(5500);
        
        _orderRepo.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_Order_Cancelled()
    {
        var custId = Guid.NewGuid();
        var order = new Order { Id = Guid.NewGuid(), CustomerId = custId, Status = OrderStatus.Cancelled };
        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(new PayOrderWithWalletCommand(order.Id, custId), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_CustomerWallet_Inactive()
    {
        var custId = Guid.NewGuid();
        var order = new Order { Id = Guid.NewGuid(), CustomerId = custId, Status = OrderStatus.PaymentPending };
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = custId, Balance = 1000, Status = WalletStatus.Locked };

        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _walletRepo.Setup(x => x.GetByUserIdAsync(custId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        var result = await _handler.Handle(new PayOrderWithWalletCommand(order.Id, custId), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_ShopWallet_Missing()
    {
        var custId = Guid.NewGuid();
        var order = new Order { Id = Guid.NewGuid(), CustomerId = custId, Status = OrderStatus.PaymentPending, TotalAmount = 100 };
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = custId, Balance = 1000, Status = WalletStatus.Active };

        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _walletRepo.Setup(x => x.GetByUserIdAsync(custId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _walletRepo.Setup(x => x.GetByUserIdAsync(SHOP_OWNER_ID, It.IsAny<CancellationToken>())).ReturnsAsync((Wallet?)null);

        var result = await _handler.Handle(new PayOrderWithWalletCommand(order.Id, custId), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_ShopWallet_Inactive()
    {
        var custId = Guid.NewGuid();
        var order = new Order { Id = Guid.NewGuid(), CustomerId = custId, Status = OrderStatus.PaymentPending, TotalAmount = 100 };
        var wallet = new Wallet { Id = Guid.NewGuid(), UserId = custId, Balance = 1000, Status = WalletStatus.Active };
        var shopWallet = new Wallet { Id = Guid.NewGuid(), UserId = SHOP_OWNER_ID, Status = WalletStatus.Locked };

        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _walletRepo.Setup(x => x.GetByUserIdAsync(custId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _walletRepo.Setup(x => x.GetByUserIdAsync(SHOP_OWNER_ID, It.IsAny<CancellationToken>())).ReturnsAsync(shopWallet);

        var result = await _handler.Handle(new PayOrderWithWalletCommand(order.Id, custId), CancellationToken.None);
        
        result.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_UpdateFails()
    {
        var custId = Guid.NewGuid();
        var order = new Order { Id = Guid.NewGuid(), CustomerId = custId, TotalAmount = 500, Status = OrderStatus.PaymentPending };
        var custWallet = new Wallet { Id = Guid.NewGuid(), UserId = custId, Balance = 1000, Status = WalletStatus.Active, RowVersion = new byte[] { 1 } };
        var shopWallet = new Wallet { Id = Guid.NewGuid(), UserId = SHOP_OWNER_ID, Balance = 5000, Status = WalletStatus.Active, RowVersion = new byte[] { 2 } };

        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _walletRepo.Setup(x => x.GetByUserIdAsync(custId, It.IsAny<CancellationToken>())).ReturnsAsync(custWallet);
        _walletRepo.Setup(x => x.GetByUserIdAsync(SHOP_OWNER_ID, It.IsAny<CancellationToken>())).ReturnsAsync(shopWallet);
        _walletRepo.Setup(x => x.GetForUpdateAsync(custWallet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(custWallet);
        _walletRepo.Setup(x => x.GetForUpdateAsync(shopWallet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(shopWallet);
        
        _walletRepo.Setup(x => x.UpdateBalanceAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(new PayOrderWithWalletCommand(order.Id, custId), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public async Task Handle_Should_Handle_ConcurrencyException()
    {
        var custId = Guid.NewGuid();
        var order = new Order { Id = Guid.NewGuid(), CustomerId = custId, TotalAmount = 500, Status = OrderStatus.PaymentPending };
        var custWallet = new Wallet { Id = Guid.NewGuid(), UserId = custId, Balance = 1000, Status = WalletStatus.Active, RowVersion = new byte[] { 1 } };
        var shopWallet = new Wallet { Id = Guid.NewGuid(), UserId = SHOP_OWNER_ID, Balance = 5000, Status = WalletStatus.Active, RowVersion = new byte[] { 2 } };

        _orderRepo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _walletRepo.Setup(x => x.GetByUserIdAsync(custId, It.IsAny<CancellationToken>())).ReturnsAsync(custWallet);
        _walletRepo.Setup(x => x.GetByUserIdAsync(SHOP_OWNER_ID, It.IsAny<CancellationToken>())).ReturnsAsync(shopWallet);
        _walletRepo.Setup(x => x.GetForUpdateAsync(custWallet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(custWallet);
        _walletRepo.Setup(x => x.GetForUpdateAsync(shopWallet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(shopWallet);

        _walletRepo.Setup(x => x.UpdateBalanceAsync(custWallet.Id, It.IsAny<decimal>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var result = await _handler.Handle(new PayOrderWithWalletCommand(order.Id, custId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _transactionRepo.Verify(x => x.FailTransactionAsync(It.IsAny<Guid>(), "Concurrency conflict", It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
