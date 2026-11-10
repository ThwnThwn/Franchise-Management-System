using FluentAssertions;
using SalesService.Application.Features.RefundTickets.Queries.GetByUserId;
using SalesService.Application.Features.WalletTransactions.Querries.GetAll;
using SalesService.Application.Features.WalletTransactions.Queries.GetAll;
using SalesService.Application.Features.WithdrawalRequests.Commands.CreateWithdrawal;
using Xunit;
using System;

namespace SalesService.Application.Tests.Common;

public class ValidatorBoosterTests
{
    [Fact]
    public void GetRefundByUserIdValidator_Should_Work()
    {
        var validator = new GetRefundByUserIdValidator();
        var query = new GetRefundByUserIdQuery(Guid.Empty, Guid.Empty, 1, 10);
        var result = validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "UserIdToSearch");
    }

    [Fact]
    public void GetAllWalletTransactionsQueryValidator_Should_Work()
    {
        var validator = new GetAllWalletTransactionsQueryValidator();
        var query = new GetAllWalletTransactionsQuery(Guid.Empty, 1, 10);
        var result = validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "LoggedUserId");
    }
}
