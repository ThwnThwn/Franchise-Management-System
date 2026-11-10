using FluentValidation.TestHelper;
using SalesService.Application.Features.RefundTickets.Command.InternalUserCreateRefund;
using SalesService.Application.Features.RefundTickets.Validators;
using SalesService.Application.Common.Models;
using SalesService.Domain.Enums;
using Xunit;
using System;

namespace SalesService.Application.Tests.Features.RefundTickets;

public class InternalUserCreateRefundTicketCommandValidatorTests
{
    private readonly InternalUserCreateRefundTicketCommandValidator _validator = new();

    [Fact]
    public void Should_HaveError_When_Override_Missing()
    {
        var model = new InternalUserCreateRefundTicketCommand(Guid.NewGuid(), new InternalUserCreateRefundTicketCommand.InternalUserCreateRefundTicketRequest(), null);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.InternalUserOverride);
    }

    [Fact]
    public void Should_HaveError_When_Amount_Is_Zero()
    {
        var request = new InternalUserCreateRefundTicketCommand.InternalUserCreateRefundTicketRequest { Amount = 0 };
        var model = new InternalUserCreateRefundTicketCommand(Guid.NewGuid(), request, new InternalUserOverrideInfo());
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Request.Amount);
    }

    [Fact]
    public void Should_HaveError_When_Bypass_And_NormalProcess()
    {
        var overrideInfo = new InternalUserOverrideInfo 
        { 
            BypassAmountValidation = true, 
            RefundReason = RefundReasonType.NormalProcess 
        };
        var model = new InternalUserCreateRefundTicketCommand(Guid.NewGuid(), new InternalUserCreateRefundTicketCommand.InternalUserCreateRefundTicketRequest { Amount = 100 }, overrideInfo);
        
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.InternalUserOverride!.RefundReason);
    }

    [Fact]
    public void Should_NotHaveError_When_ValidRequest()
    {
        var overrideInfo = new InternalUserOverrideInfo 
        { 
            BypassAmountValidation = true, 
            RefundReason = RefundReasonType.SystemError 
        };
        var model = new InternalUserCreateRefundTicketCommand(Guid.NewGuid(), new InternalUserCreateRefundTicketCommand.InternalUserCreateRefundTicketRequest { Amount = 100 }, overrideInfo);
        
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
