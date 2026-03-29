using FluentValidation.TestHelper;
using SalesService.Application.Features.WalletTransactions.Queries.GetById;
using Xunit;
using System;

namespace SalesService.Application.Tests.Features.WalletTransactions;

public class GetWalletTransactionByIdQueryValidatorTest
{
    private readonly GetWalletTransactionByIdQueryValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_TransactionId_Empty()
    {
        var query = new GetWalletTransactionByIdQuery(Guid.Empty, Guid.NewGuid());
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.TransactionId);
    }

    [Fact]
    public void Should_Have_Error_When_LoggedUserId_Empty()
    {
        var query = new GetWalletTransactionByIdQuery(Guid.NewGuid(), Guid.Empty);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.LoggedUserId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Valid()
    {
        var query = new GetWalletTransactionByIdQuery(Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
