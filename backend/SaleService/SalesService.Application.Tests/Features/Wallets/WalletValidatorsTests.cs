using FluentValidation.TestHelper;
using SalesService.Application.Features.Wallets.Commands.UpdateStatus;
using SalesService.Application.Features.Wallets.Command.AddBalance;
using SalesService.Application.Features.Wallets.Command.DeductBalance;
using SalesService.Application.Features.Wallets.Validators;
using SalesService.Domain.Enums;
using Xunit;
using System;

namespace SalesService.Application.Tests.Features.Wallets;

public class WalletValidatorsTests
{
    private readonly UpdateWalletStatusCommandValidator _updateStatusValidator = new();
    private readonly AddBalanceCommandValidator _addBalanceValidator = new();
    private readonly DeductBalanceCommandValidator _deductBalanceValidator = new();

    [Fact]
    public void UpdateStatus_Should_HaveError_When_WalletId_Empty()
    {
        var model = new UpdateWalletStatusCommand(Guid.Empty, Guid.NewGuid(), new UpdateWalletStatusCommand.UpdateWalletStatusRequest());
        var result = _updateStatusValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.WalletId);
    }

    [Fact]
    public void UpdateStatus_Should_HaveError_When_Reason_Empty_For_Suspended()
    {
        var request = new UpdateWalletStatusCommand.UpdateWalletStatusRequest { NewStatus = WalletStatus.Suspended, Reason = "" };
        var model = new UpdateWalletStatusCommand(Guid.NewGuid(), Guid.NewGuid(), request);
        var result = _updateStatusValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Request.Reason);
    }

    [Fact]
    public void AddBalance_Should_HaveError_When_Amount_Zero()
    {
        var request = new AddBalanceCommand.AddBalanceCommandRequest { Amount = 0, RowVersion = new byte[] { 1 } };
        var model = new AddBalanceCommand(Guid.NewGuid(), Guid.NewGuid(), request);
        var result = _addBalanceValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Request.Amount);
    }

    [Fact]
    public void AddBalance_Should_HaveError_When_RowVersion_Empty()
    {
        var request = new AddBalanceCommand.AddBalanceCommandRequest { Amount = 100, RowVersion = Array.Empty<byte>() };
        var model = new AddBalanceCommand(Guid.NewGuid(), Guid.NewGuid(), request);
        var result = _addBalanceValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Request.RowVersion);
    }

    [Fact]
    public void DeductBalance_Should_HaveError_When_Amount_Zero()
    {
        var request = new DeductBalanceCommand.DeductBalanceCommandRequest { Amount = 0, RowVersion = new byte[] { 1 } };
        var model = new DeductBalanceCommand(Guid.NewGuid(), Guid.NewGuid(), request);
        var result = _deductBalanceValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Request.Amount);
    }
}
