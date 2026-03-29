using FluentAssertions;
using SalesService.Application.Authorization;
using SalesService.Application.Features.RefundTickets.Command.CreateRefund;
using SalesService.Application.Common.Models;
using System;

namespace SalesService.Application.Tests.Authorization;

public class RefundAuthorizationServiceTests
{
    private readonly RefundAuthorizationService _service = new();

    [Theory]
    [InlineData("Unknown")]
    [InlineData("Admin")]
    public void ValidateInternalUserOverridePermissions_ShouldReturnForbidden_ForInvalidRole(string role)
    {
        var result = _service.ValidateInternalUserOverridePermissions(role, new InternalUserOverrideInfo(), 100, 100, DateTimeOffset.UtcNow);
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Forbidden);
    }

    [Fact]
    public void ValidateInternalUserOverridePermissions_Staff_AutoApproveLimit()
    {
        var overrideInfo = new InternalUserOverrideInfo { AutoApprove = true };
        var result = _service.ValidateInternalUserOverridePermissions("Staff", overrideInfo, 2000000, 2000000, DateTimeOffset.UtcNow);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainMatch("*Staff can only auto-approve refunds up to 1,000,000*");
    }

    [Fact]
    public void ValidateInternalUserOverridePermissions_Staff_BypassTimeLimit()
    {
        var overrideInfo = new InternalUserOverrideInfo { BypassTimeLimit = true };
        var result = _service.ValidateInternalUserOverridePermissions("Staff", overrideInfo, 500000, 500000, DateTimeOffset.UtcNow.AddDays(-61));
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainMatch("*Staff can only bypass time limit within 60 days*");
    }
    
    [Fact]
    public void ValidateInternalUserOverridePermissions_Staff_BypassAmountValidation()
    {
        var overrideInfo = new InternalUserOverrideInfo { BypassAmountValidation = true };
        var result = _service.ValidateInternalUserOverridePermissions("Staff", overrideInfo, 500000, 500000, DateTimeOffset.UtcNow);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainMatch("*Staff cannot bypass amount validation*");
    }

    [Fact]
    public void ValidateInternalUserOverridePermissions_Staff_MaxRefundAmount()
    {
        var overrideInfo = new InternalUserOverrideInfo { AutoApprove = false };
        var result = _service.ValidateInternalUserOverridePermissions("Staff", overrideInfo, 11000000, 11000000, DateTimeOffset.UtcNow);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainMatch("*Staff can only create refunds up to 10,000,000*");
    }

    [Fact]
    public void ValidateInternalUserOverridePermissions_Manager_BypassTimeLimit()
    {
        var overrideInfo = new InternalUserOverrideInfo { BypassTimeLimit = true };
        var result = _service.ValidateInternalUserOverridePermissions("Manager", overrideInfo, 500000, 500000, DateTimeOffset.UtcNow.AddDays(-366));
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainMatch("*Managers can only bypass time limit within 1 year*");
    }

    [Fact]
    public void ValidateInternalUserOverridePermissions_Manager_BypassAmountValidationLimit()
    {
        var overrideInfo = new InternalUserOverrideInfo { BypassAmountValidation = true, InternalNotes = "Notes" };
        var result = _service.ValidateInternalUserOverridePermissions("Manager", overrideInfo, 200000, 100000, DateTimeOffset.UtcNow);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainMatch("*Managers can only over-refund up to 150% of original amount*");
    }

    [Fact]
    public void ValidateInternalUserOverridePermissions_Manager_BypassAmountValidationNotesRequired()
    {
        var overrideInfo = new InternalUserOverrideInfo { BypassAmountValidation = true };
        var result = _service.ValidateInternalUserOverridePermissions("Manager", overrideInfo, 120000, 100000, DateTimeOffset.UtcNow);
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(x => x.ErrorMessage.Contains("InternalNotes is required"));
    }

    [Fact]
    public void ValidateInternalUserOverridePermissions_InternalUser_NotesRequired()
    {
        var overrideInfo = new InternalUserOverrideInfo { BypassAmountValidation = true };
        var result = _service.ValidateInternalUserOverridePermissions("InternalUser", overrideInfo, 500000, 500000, DateTimeOffset.UtcNow);
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(x => x.ErrorMessage.Contains("InternalNotes is required"));
    }

    [Fact]
    public void ValidateInternalUserOverridePermissions_InternalUser_Success()
    {
        var overrideInfo = new InternalUserOverrideInfo { BypassAmountValidation = true, InternalNotes = "Audit Note" };
        var result = _service.ValidateInternalUserOverridePermissions("InternalUser", overrideInfo, 5000000, 500000, DateTimeOffset.UtcNow.AddYears(-5));
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("Cashier")]
    [InlineData("Staff")]
    [InlineData("Manager")]
    [InlineData("InternalUser")]
    public void GetUserPermissions_ShouldReturnPermissions_ForValidRoles(string role)
    {
        var result = _service.GetUserPermissions(role);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void GetUserPermissions_ShouldReturnForbidden_ForInvalidRole()
    {
        var result = _service.GetUserPermissions("InvalidRole");
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Forbidden);
    }
}
