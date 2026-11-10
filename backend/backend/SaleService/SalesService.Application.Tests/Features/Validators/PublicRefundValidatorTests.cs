using FluentValidation.TestHelper;
using SalesService.Application.Features.RefundTickets.Command.CancelRefund;
using SalesService.Application.Features.RefundTickets.Command.RejectRefundCommandValidator;
using SalesService.Application.Features.RefundTickets.Command.ApproveRefund;
using SalesService.Application.Features.RefundTickets.Queries.GetById;
using SalesService.Application.Features.RefundTickets.Command.RejectRefundByAdmin;
using SalesService.Application.Features.RefundTickets.Command.CustomerCreateRefund;
using SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer;
using Xunit;
using System;
using static SalesService.Application.Features.RefundTickets.Command.RejectRefundByAdmin.RejectRefundCommand;
using static SalesService.Application.Features.RefundTickets.Command.ApproveRefund.ApproveRefundCommand;

using CreateRefundRequest = SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer.CustomerCreateRefundTicketCommand.CustomerCreateRefundTicketRequest;

namespace SalesService.Application.Tests.Features.Validators;

public class PublicRefundValidatorTests
{
    [Fact]
    public void CancelRefundCommandValidator_Test()
    {
        var validator = new CancelRefundCommandValidator();
        validator.TestValidate(new CancelRefundCommand(Guid.Empty, null)).ShouldHaveValidationErrorFor(x => x.RefundTicketId);
    }

    [Fact]
    public void CustomerCreateRefundTicketCommandValidator_Test()
    {
        var validator = new CustomerCreateRefundTicketCommandValidator();
        var req = new CreateRefundRequest { Amount = 100, Reason = "test" };
        var command = new CustomerCreateRefundTicketCommand(Guid.NewGuid(), req);
        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RejectRefundCommandValidator_Test()
    {
        var validator = new RejectRefundCommandValidator();
        var req = new RejectRefundRequest { RejectNotes = "No" };
        validator.TestValidate(new RejectRefundCommand(Guid.Empty, req)).ShouldHaveValidationErrorFor(x => x.RefundTicketId);
        validator.TestValidate(new RejectRefundCommand(Guid.NewGuid(), req)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ApproveRefundCommandValidator_Test()
    {
        var validator = new ApproveRefundCommandValidator();
        var req = new ApproveRefundRequest { ApprovalNotes = "OK" };
        validator.TestValidate(new ApproveRefundCommand(Guid.Empty, req)).ShouldHaveValidationErrorFor(x => x.RefundTicketId);
        validator.TestValidate(new ApproveRefundCommand(Guid.NewGuid(), req)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void GetRefundByIdQueryValidator_Test()
    {
        var validator = new GetRefundByIdQueryValidator();
        validator.TestValidate(new GetRefundByIdQuery(Guid.Empty, Guid.NewGuid())).ShouldHaveValidationErrorFor(x => x.RefundTicketId);
        validator.TestValidate(new GetRefundByIdQuery(Guid.NewGuid(), Guid.NewGuid())).ShouldNotHaveAnyValidationErrors();
    }
}
