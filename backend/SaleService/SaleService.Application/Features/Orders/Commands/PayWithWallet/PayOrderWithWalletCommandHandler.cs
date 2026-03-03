using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Orders.Commands.PayWithWallet;

public sealed class PayOrderWithWalletCommandHandler
    : IRequestHandler<PayOrderWithWalletCommand, Result<PayOrderWithWalletCommand.PayOrderWithWalletResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private static readonly Guid SHOP_OWNER_WALLET_ID = Guid.Parse("645ca1f5-dec2-4fce-93cf-4edff3511300");

    public PayOrderWithWalletCommandHandler(
        IOrderRepository orderRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository)
    {
        _orderRepository = orderRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<PayOrderWithWalletCommand.PayOrderWithWalletResponse>> Handle(
        PayOrderWithWalletCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Validate order
        var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);

        if (order == null)
            return Result.NotFound($"Order {command.OrderId} not found");

        if (order.CustomerId != command.CustomerId)
            return Result.Forbidden("You don't have permission to pay this order");

        if (order.Status == OrderStatus.Completed)
            return Result.Invalid(new ValidationError("Order", "Order is already paid"));

        if (order.Status == OrderStatus.Cancelled)
            return Result.Invalid(new ValidationError("Order", "Order is cancelled"));

        var amountToPay = order.TotalAmount;

        // 2. Get customer wallet
        var customerWallet = await _walletRepository.GetByUserIdAsync(
            command.CustomerId,
            cancellationToken);

        if (customerWallet == null)
            return Result.NotFound("Customer wallet not found");

        if (customerWallet.Status != WalletStatus.Active)
            return Result.Invalid(new ValidationError(
                "Wallet",
                $"Customer wallet is not active. Status: {customerWallet.Status}"));

        // 3. Check sufficient balance
        if (customerWallet.Balance < amountToPay)
        {
            return Result.Invalid(new ValidationError(
                "InsufficientBalance",
                $"Insufficient balance. Current: {customerWallet.Balance:N0}, Required: {amountToPay:N0}"));
        }

        // 4. Get shop owner wallet
        var shopWallet = await _walletRepository.GetByIdAsync(
            SHOP_OWNER_WALLET_ID,
            cancellationToken);

        if (shopWallet == null)
            return Result.NotFound("Shop owner wallet not found");

        if (shopWallet.Status != WalletStatus.Active)
            return Result.Error("Shop owner wallet is not active");

        // 5. Lock wallets for update
        var lockedCustomerWallet = await _walletRepository.GetForUpdateAsync(
            customerWallet.Id,
            cancellationToken);

        var lockedShopWallet = await _walletRepository.GetForUpdateAsync(
            shopWallet.Id,
            cancellationToken);

        // 6. Calculate balances
        var customerBalanceBefore = lockedCustomerWallet.Balance;
        var customerBalanceAfter = customerBalanceBefore - amountToPay;

        var shopBalanceBefore = lockedShopWallet.Balance;
        var shopBalanceAfter = shopBalanceBefore + amountToPay;

        var paidAt = DateTimeOffset.UtcNow;

        // 7. Create customer transaction (DEDUCT)
        var customerTransaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = customerWallet.Id,
            UserId = command.CustomerId,
            OrderId = order.Id,
            TransactionCode = GenerateTransactionCode("PAY"),
            Amount = -amountToPay, // Negative for deduction
            BalanceBefore = customerBalanceBefore,
            BalanceAfter = customerBalanceAfter,
            TransactionType = TransactionType.Payment,
            Status = TransactionStatus.Completed,
            PaymentMethod = PaymentMethod.Wallet,
            ReferenceId = order.Id,
            ReferenceType = WalletReferenceType.Order,
            Description = $"Payment for order {order.Code}",
            CreatedAt = paidAt,
            UpdatedAt = paidAt,
            CompletedAt = paidAt
        };

        // 8. Create shop transaction (ADD)
        var shopTransaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = shopWallet.Id,
            UserId = shopWallet.UserId,
            OrderId = order.Id,
            ParentTransactionId = customerTransaction.Id, // Link transactions
            TransactionCode = GenerateTransactionCode("RCV"),
            Amount = amountToPay, // Positive for receipt
            BalanceBefore = shopBalanceBefore,
            BalanceAfter = shopBalanceAfter,
            TransactionType = TransactionType.Deposit,
            Status = TransactionStatus.Completed,
            PaymentMethod = PaymentMethod.Wallet,
            ReferenceId = order.Id,
            ReferenceType = WalletReferenceType.Order,
            Description = $"Received payment for order {order.Code} from customer {command.CustomerId}",
            CreatedAt = paidAt,
            UpdatedAt = paidAt,
            CompletedAt = paidAt
        };

        await _transactionRepository.AddAsync(customerTransaction, cancellationToken);
        await _transactionRepository.AddAsync(shopTransaction, cancellationToken);

        try
        {
            // 9. Update customer wallet balance
            var customerUpdated = await _walletRepository.UpdateBalanceAsync(
                customerWallet.Id,
                customerBalanceAfter,
                customerWallet.RowVersion,
                cancellationToken);

            if (!customerUpdated)
                return Result.Error("Failed to update customer wallet");

            // 10. Update shop wallet balance
            var shopUpdated = await _walletRepository.UpdateBalanceAsync(
                shopWallet.Id,
                shopBalanceAfter,
                shopWallet.RowVersion,
                cancellationToken);

            if (!shopUpdated)
                return Result.Error("Failed to update shop wallet");

            // 11. Update order status
            order.Status = OrderStatus.Completed;
            order.PaymentMethod = PaymentMethod.Wallet;
            order.PaidAmount = amountToPay;
            order.PaidAt = paidAt;
            order.UpdatedAt = paidAt;

            await _orderRepository.UpdateAsync(order, cancellationToken);

            // 12. Return response
            var response = new PayOrderWithWalletCommand.PayOrderWithWalletResponse
            {
                OrderId = order.Id,
                OrderCode = order.Code,
                CustomerWalletId = customerWallet.Id,
                ShopWalletId = shopWallet.Id,
                Amount = amountToPay,
                CustomerBalanceBefore = customerBalanceBefore,
                CustomerBalanceAfter = customerBalanceAfter,
                ShopBalanceBefore = shopBalanceBefore,
                ShopBalanceAfter = shopBalanceAfter,
                CustomerTransactionId = customerTransaction.Id,
                ShopTransactionId = shopTransaction.Id,
                CustomerTransactionCode = customerTransaction.TransactionCode,
                ShopTransactionCode = shopTransaction.TransactionCode,
                PaidAt = paidAt
            };

            return Result.Success(response);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Rollback transactions
            await _transactionRepository.FailTransactionAsync(
                customerTransaction.Id,
                "Concurrency conflict",
                cancellationToken);

            await _transactionRepository.FailTransactionAsync(
                shopTransaction.Id,
                "Concurrency conflict",
                cancellationToken);

            return Result.Error("Wallet was modified by another transaction. Please retry.");
        }
        catch (Exception ex)
        {
            // Rollback transactions
            await _transactionRepository.FailTransactionAsync(
                customerTransaction.Id,
                ex.Message,
                cancellationToken);

            await _transactionRepository.FailTransactionAsync(
                shopTransaction.Id,
                ex.Message,
                cancellationToken);

            return Result.Error($"Failed to process payment: {ex.Message}");
        }
    }

    private static string GenerateTransactionCode(string prefix)
    {
        var shortGuid = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"{prefix}-{timestamp}-{shortGuid}";
    }
}