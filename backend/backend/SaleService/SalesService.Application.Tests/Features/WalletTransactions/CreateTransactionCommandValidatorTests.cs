using FluentValidation.TestHelper;
using SalesService.Application.Features.WalletTransactions.Commands.CreateTransaction;
using SalesService.Application.Features.WalletTransactions.Validators;
using SalesService.Domain.Enums;
using Xunit;
using System;

namespace SalesService.Application.Tests.Features.WalletTransactions;

public class CreateTransactionCommandValidatorTests
{
    private readonly CreateTransactionCommandValidator _validator = new();

    [Fact]
    public void Should_HaveError_When_UserId_Is_Empty()
    {
        var model = new CreateTransactionCommand(Guid.Empty, null!);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_HaveError_When_Request_Is_Null()
    {
        var model = new CreateTransactionCommand(Guid.NewGuid(), null!);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Request);
    }

    [Fact]
    public void Should_HaveError_When_Amount_Is_Zero_Or_Negative()
    {
        var request = new CreateTransactionCommand.CreateTransactionRequest { Amount = 0 };
        var model = new CreateTransactionCommand(Guid.NewGuid(), request);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Request.Amount);
    }

    [Fact]
    public void Should_HaveError_When_TransactionType_Is_Payment_And_Reference_Missing()
    {
        var request = new CreateTransactionCommand.CreateTransactionRequest 
        { 
            TransactionType = TransactionType.Payment, 
            Amount = 100,
            WalletId = Guid.NewGuid()
        };
        var model = new CreateTransactionCommand(Guid.NewGuid(), request);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Request.ReferenceId);
        result.ShouldHaveValidationErrorFor(x => x.Request.ReferenceType);
    }

    [Fact]
    public void Should_HaveError_When_Transfer_Target_Is_Same_As_Source()
    {
        var walletId = Guid.NewGuid();
        var request = new CreateTransactionCommand.CreateTransactionRequest 
        { 
            TransactionType = TransactionType.Transfer, 
            Amount = 100,
            WalletId = walletId,
            DestinationWalletId = walletId
        };
        var model = new CreateTransactionCommand(Guid.NewGuid(), request);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Request.DestinationWalletId);
    }

    [Fact]
    public void Should_HaveError_When_AdminOverride_InternalNotes_Missing()
    {
        var request = new CreateTransactionCommand.CreateTransactionRequest { Amount = 100, WalletId = Guid.NewGuid() };
        var adminOverride = new CreateTransactionCommand.HasAdminOverride 
        { 
            AdminId = Guid.NewGuid(), 
            BypassBalanceCheck = true 
        };
        var model = new CreateTransactionCommand(Guid.NewGuid(), request, adminOverride);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.AdminOverride!.InternalNotes);
    }

    [Fact]
    public void Should_NotHaveError_When_ValidRequest()
    {
        var request = new CreateTransactionCommand.CreateTransactionRequest 
        { 
            TransactionType = TransactionType.Deposit, 
            Amount = 100,
            WalletId = Guid.NewGuid(),
            PaymentMethod = PaymentMethod.VNPay
        };
        var model = new CreateTransactionCommand(Guid.NewGuid(), request);
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
