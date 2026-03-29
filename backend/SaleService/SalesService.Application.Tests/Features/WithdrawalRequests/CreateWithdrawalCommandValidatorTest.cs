using FluentAssertions;
using SalesService.Application.Features.WithdrawalRequests.Commands.CreateWithdrawal;
using Xunit;
using System;

namespace SalesService.Application.Tests.Features.WithdrawalRequests;

public class CreateWithdrawalCommandValidatorTest
{
    private readonly CreateWithdrawalCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Success_When_Amount_Valid()
    {
        var cmd = new CreateWithdrawalCommand(Guid.NewGuid(), "100.50", "BANK", "BANK-NAME", "ACC-NAME", "123456", "Notes");
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_Amount_Empty()
    {
        var cmd = new CreateWithdrawalCommand(Guid.NewGuid(), "", null, "BANK-NAME", "ACC-NAME", "123456", null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Amount" && x.ErrorMessage == "Số tiền không được để trống");
    }

    [Fact]
    public void Validate_Should_Fail_When_Amount_Invalid()
    {
        var cmd = new CreateWithdrawalCommand(Guid.NewGuid(), "not-a-decimal", null, "BANK-NAME", "ACC-NAME", "123456", null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Định dạng số tiền không hợp lệ");
    }

    [Fact]
    public void Validate_Should_Fail_When_Amount_NonPositive()
    {
        var cmd = new CreateWithdrawalCommand(Guid.NewGuid(), "0", null, "BANK-NAME", "ACC-NAME", "123456", null);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Số tiền phải lớn hơn 0");
    }
}
