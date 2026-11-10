using Ardalis.Result;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SalesService.Application.Authorization;
using SalesService.Application.Features.Wallets.Commands.UpdateStatus;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SalesService.Application.Features.Wallets.Commands.UpdateStatus.UpdateWalletStatusCommand;

namespace SalesService.Application.Tests.Features.Wallets;

public class UpdateWalletStatusCommandHandlerTest
{
    private readonly Mock<IWalletRepository> _walletRepo = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepo = new();
    private readonly Mock<IRefundTicketRepository> _refundRepo = new();
    private readonly Mock<IWalletAuthorizationService> _authService = new();
    private readonly Mock<IIdentityServiceClient> _identityClient = new();
    private readonly UpdateWalletStatusCommandHandler _handler;

    public UpdateWalletStatusCommandHandlerTest()
    {
        _handler = new UpdateWalletStatusCommandHandler(
            _walletRepo.Object,
            _transactionRepo.Object,
            _refundRepo.Object,
            _authService.Object,
            _identityClient.Object);
    }

    private void SetupPermission(Guid adminId, bool success = true)
    {
        _identityClient.Setup(x => x.GetUserByIdAsync(adminId)).ReturnsAsync(new UserInfo { Role = new UserRole { RoleName = "Admin" } });
        _authService.Setup(x => x.ValidateStatusUpdatePermission(It.IsAny<string>(), It.IsAny<WalletStatus>()))
            .Returns(success ? Result.Success() : Result.Forbidden("No permission"));
    }

    [Fact]
    public async Task Handle_Should_ReturnForbidden_When_NoPermission()
    {
        var adminId = Guid.NewGuid();
        SetupPermission(adminId, false);
        var command = new UpdateWalletStatusCommand(Guid.NewGuid(), adminId, new UpdateWalletStatusRequest { NewStatus = WalletStatus.Locked });

        var result = await _handler.Handle(command, CancellationToken.None);
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_WalletMissing()
    {
        var adminId = Guid.NewGuid();
        SetupPermission(adminId);
        _walletRepo.Setup(x => x.GetForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Wallet?)null);
        var command = new UpdateWalletStatusCommand(Guid.NewGuid(), adminId, new UpdateWalletStatusRequest { NewStatus = WalletStatus.Locked });

        var result = await _handler.Handle(command, CancellationToken.None);
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_Idempotent()
    {
        var adminId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        SetupPermission(adminId);
        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(new Wallet { Id = walletId, Status = WalletStatus.Active });
        var command = new UpdateWalletStatusCommand(walletId, adminId, new UpdateWalletStatusRequest { NewStatus = WalletStatus.Active });

        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_TransitionInvalid()
    {
        var adminId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        SetupPermission(adminId);
        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(new Wallet { Id = walletId, Status = WalletStatus.Active });
        var command = new UpdateWalletStatusCommand(walletId, adminId, new UpdateWalletStatusRequest { NewStatus = WalletStatus.Inactive });

        var result = await _handler.Handle(command, CancellationToken.None);
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_When_HasPendingTransactions()
    {
        var adminId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        SetupPermission(adminId);
        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(new Wallet { Id = walletId, Status = WalletStatus.Active });
        _transactionRepo.Setup(x => x.HasPendingTransactionsAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var command = new UpdateWalletStatusCommand(walletId, adminId, new UpdateWalletStatusRequest { NewStatus = WalletStatus.Locked });

        var result = await _handler.Handle(command, CancellationToken.None);
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_ValidTransition()
    {
        var adminId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        SetupPermission(adminId);
        var wallet = new Wallet { Id = walletId, Status = WalletStatus.Active, Notes = "Old notes" };
        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        var command = new UpdateWalletStatusCommand(walletId, adminId, new UpdateWalletStatusRequest { NewStatus = WalletStatus.Locked, Reason = "Risk", AdminNotes = "Suspicious" });

        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        wallet.Status.Should().Be(WalletStatus.Locked);
        wallet.Notes.Should().Contain("Risk");
    }

    [Fact]
    public async Task Handle_Should_ReturnError_When_ConcurrencyException()
    {
        var adminId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        SetupPermission(adminId);
        _walletRepo.Setup(x => x.GetForUpdateAsync(walletId, It.IsAny<CancellationToken>())).ReturnsAsync(new Wallet { Id = walletId, Status = WalletStatus.Active });
        _walletRepo.Setup(x => x.UpdateAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateConcurrencyException());
        var command = new UpdateWalletStatusCommand(walletId, adminId, new UpdateWalletStatusRequest { NewStatus = WalletStatus.Locked });

        var result = await _handler.Handle(command, CancellationToken.None);
        result.Status.Should().Be(ResultStatus.Error);
    }
}
