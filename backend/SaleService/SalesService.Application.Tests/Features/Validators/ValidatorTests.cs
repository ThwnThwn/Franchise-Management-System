using FluentValidation.TestHelper;
using SalesService.Application.Features.Wallets.Queries.GetByUserId;
using SalesService.Application.Features.Wallets.Validators;
using SalesService.Application.Features.WalletTransactions.Queries.GetById;
using SalesService.Application.Features.Wallets.Command.AddBalance;
using SalesService.Application.Features.Orders.Commands.Create;
using SalesService.Application.Features.Orders.Commands.Update;
using SalesService.Application.Features.WalletTransactions.Queries.GetByCode;
using SalesService.Application.Features.WalletTransactions.Querries.GetByOrderId;
using SalesService.Application.Features.WalletTransactions.Queries.GetByUserId;
using SalesService.Application.Features.WalletTransactions.Querries.GetByUserId;
using Xunit;
using System;
using System.Collections.Generic;

using AddBalanceRequest = SalesService.Application.Features.Wallets.Command.AddBalance.AddBalanceCommand.AddBalanceCommandRequest;

namespace SalesService.Application.Tests.Features.Validators;

public class ValidatorTests
{
    [Fact]
    public void GetWalletByUserIdQueryValidator_Test()
    {
        var validator = new GetWalletByUserIdQueryValidator();
        validator.TestValidate(new GetWalletByUserIdQuery(Guid.Empty)).ShouldHaveAnyValidationError();
        validator.TestValidate(new GetWalletByUserIdQuery(Guid.NewGuid())).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AddBalanceCommandValidator_Test()
    {
        var validator = new AddBalanceCommandValidator();
        var command = new AddBalanceCommand(Guid.NewGuid(), Guid.NewGuid(), new AddBalanceRequest { Amount = 100, RowVersion = new byte[] { 1 } });
        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();

        validator.TestValidate(new AddBalanceCommand(Guid.Empty, Guid.NewGuid(), null!)).ShouldHaveValidationErrorFor(x => x.WalletId);
    }

    [Fact]
    public void GetWalletTransactionByIdQueryValidator_Test()
    {
        var validator = new GetWalletTransactionByIdQueryValidator();
        validator.TestValidate(new GetWalletTransactionByIdQuery(Guid.Empty, Guid.Empty)).ShouldHaveAnyValidationError();
    }

    [Fact]
    public void CreateWalletCommandValidator_Test()
    {
        var validator = new SalesService.Application.Features.Wallets.Command.Create.CreateWalletCommandValidator();
        validator.TestValidate(new SalesService.Application.Features.Wallets.Command.Create.CreateWalletCommand(Guid.Empty)).ShouldHaveValidationErrorFor(x => x.UserId);
        validator.TestValidate(new SalesService.Application.Features.Wallets.Command.Create.CreateWalletCommand(Guid.NewGuid())).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateOrderCommandValidator_Test()
    {
        var validator = new CreateOrderCommandValidator();
        var req = new CreateOrderRequestDto { Items = new List<CartItemDto> { new CartItemDto { Sku = "", Quantity = 0 } } };
        validator.TestValidate(new CreateOrderCommand(req)).ShouldHaveAnyValidationError();
    }

    [Fact]
    public void UpdateOrderCommandValidator_Test()
    {
        var validator = new UpdateOrderCommandValidator();
        var req = new UpdateOrderReq { Items = new List<SalesService.Application.Features.Orders.Commands.Update.OrderItem> { new SalesService.Application.Features.Orders.Commands.Update.OrderItem { Sku = "", Quantity = 0 } } };
        validator.TestValidate(new UpdateOrderCommand(req)).ShouldHaveAnyValidationError();
    }

    [Fact]
    public void GetWalletTransactionByCodeQueryValidator_Test()
    {
        var validator = new GetWalletTransactionByCodeQueryValidator();
        validator.TestValidate(new GetWalletTransactionByCodeQuery("CODE", Guid.NewGuid())).ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new GetWalletTransactionByCodeQuery("", Guid.NewGuid())).ShouldHaveAnyValidationError();
    }

    [Fact]
    public void GetWalletTransactionByOrderIdQueryValidator_Test()
    {
        var validator = new GetWalletTransactionByOrderIdQueryValidator();
        validator.TestValidate(new GetWalletTransactionByOrderIdQuery(Guid.NewGuid(), Guid.NewGuid())).ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new GetWalletTransactionByOrderIdQuery(Guid.Empty, Guid.NewGuid())).ShouldHaveAnyValidationError();
    }

    [Fact]
    public void GetWalletTransactionsByUserIdQueryValidator_Test()
    {
        var validator = new GetWalletTransactionsByUserIdQueryValidator();
        validator.TestValidate(new GetWalletTransactionsByUserIdQuery(Guid.NewGuid(), Guid.NewGuid())).ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new GetWalletTransactionsByUserIdQuery(Guid.Empty, Guid.NewGuid())).ShouldHaveAnyValidationError();
    }
}
