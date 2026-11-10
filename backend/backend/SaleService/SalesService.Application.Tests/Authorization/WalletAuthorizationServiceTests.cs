using Ardalis.Result;
using FluentAssertions;
using SalesService.Application.Authorization;
using SalesService.Domain.Enums;
using Xunit;

namespace SalesService.Application.Tests.Authorization;

public class WalletAuthorizationServiceTests
{
    private readonly WalletAuthorizationService _service = new();

    [Fact]
    public void ValidateStatusUpdate_Should_ReturnForbidden_When_User_Not_Admin_Or_Manager()
    {
        var result = _service.ValidateStatusUpdatePermission("Customer", WalletStatus.Active);
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public void ValidateStatusUpdate_Should_ReturnForbidden_When_Manager_Tries_To_Suspend()
    {
        var result = _service.ValidateStatusUpdatePermission("Manager", WalletStatus.Suspended);
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public void ValidateStatusUpdate_Should_ReturnSuccess_When_Admin_Suspends()
    {
        var result = _service.ValidateStatusUpdatePermission("Admin", WalletStatus.Suspended);
        result.Status.Should().Be(ResultStatus.Ok);
    }

    [Fact]
    public void ValidateStatusUpdate_Should_ReturnSuccess_When_Administrator_Updates()
    {
        var result = _service.ValidateStatusUpdatePermission("Administrator", WalletStatus.Active);
        result.Status.Should().Be(ResultStatus.Ok);
    }
}
