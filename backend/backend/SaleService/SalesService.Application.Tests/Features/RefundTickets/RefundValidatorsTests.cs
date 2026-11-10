using FluentValidation.TestHelper;
using SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer;
using SalesService.Application.Features.RefundTickets.Command.CancelRefund;
using SalesService.Domain.Enums;
using Xunit;
using System;

namespace SalesService.Application.Tests.Features.RefundTickets;

public class RefundValidatorsTests
{
    private readonly SalesService.Application.Features.RefundTickets.Command.CustomerCreateRefund.CustomerCreateRefundTicketCommandValidator _createValidator = new();
    private readonly CancelRefundCommandValidator _cancelValidator = new();
    private readonly SalesService.Application.Features.RefundTickets.Validators.GetRefundByStatusQueryValidator _getByStatusValidator = new();

    [Fact]
    public void Create_Should_HaveError_When_CustomerId_Empty()
    {
        var model = new CustomerCreateRefundTicketCommand(Guid.Empty, new CustomerCreateRefundTicketCommand.CustomerCreateRefundTicketRequest());
        var result = _createValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Create_Should_HaveError_When_Amount_Zero()
    {
        var request = new CustomerCreateRefundTicketCommand.CustomerCreateRefundTicketRequest { Amount = 0, Reason = "Test" };
        var model = new CustomerCreateRefundTicketCommand(Guid.NewGuid(), request);
        var result = _createValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Request.Amount);
    }

    [Fact]
    public void Cancel_Should_HaveError_When_RefundTicketId_Empty()
    {
        var model = new CancelRefundCommand(Guid.Empty, null);
        var result = _cancelValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.RefundTicketId);
    }

    [Fact]
    public void GetByStatus_Should_HaveError_When_PageIndex_Zero()
    {
        var query = new SalesService.Application.Features.RefundTickets.Queries.GetByStatus.GetRefundByStatusQuery(null, RefundTicketStatus.Pending, 0, 10);
        var result = _getByStatusValidator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageIndex);
    }
}
