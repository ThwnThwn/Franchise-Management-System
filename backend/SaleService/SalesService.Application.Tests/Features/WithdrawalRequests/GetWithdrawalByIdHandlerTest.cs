using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Moq;
using SalesService.Application.Dtos.WithdrawalRequests;
using SalesService.Application.Features.WithdrawalRequests.Queries.GetWithdrawalById;
using SalesService.Application.Interfaces;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Tests.Features.WithdrawalRequests;

public class GetWithdrawalByIdHandlerTest
{
    private readonly Mock<IWithdrawalRequestRepository> _repo = new();
    private readonly Mock<IAesEncryptionService> _aesService = new();
    private readonly Mock<IIdentityServiceClient> _identityClient = new();
    private readonly GetWithdrawalByIdHandler _handler;

    public GetWithdrawalByIdHandlerTest()
    {
        _handler = new GetWithdrawalByIdHandler(_repo.Object, _aesService.Object, _identityClient.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnNull_When_Withdrawal_NotExist()
    {
        var id = Guid.NewGuid();
        _repo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WithdrawalRequest?)null);

        var result = await _handler.Handle(new GetWithdrawalByIdQuery(id), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_ReturnDetail_When_Exists()
    {
        var id = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var withdrawal = new WithdrawalRequest 
        { 
            Id = id, 
            EncryptedAccountNumber = "ENCRYPTED", 
            MaskedAccountNumber = "1234****", 
            ProcessedBy = adminId,
            Amount = 100,
            FeeAmount = 5,
            Status = WithdrawalStatus.Completed
        };
        
        _repo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withdrawal);
        _aesService.Setup(x => x.Decrypt("ENCRYPTED")).Returns("RAW-ACC-1");
        _identityClient.Setup(x => x.GetUserByIdAsync(adminId)).ReturnsAsync(new UserInfo { Username = "AdminUser" });

        var result = await _handler.Handle(new GetWithdrawalByIdQuery(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.RawAccountNumber.Should().Be("RAW-ACC-1");
        result.ProcessedByName.Should().Be("AdminUser");
    }

    [Fact]
    public async Task Handle_Should_UseMasked_When_DecryptFails()
    {
        var id = Guid.NewGuid();
        var withdrawal = new WithdrawalRequest { Id = id, EncryptedAccountNumber = "BAD", MaskedAccountNumber = "MASKED" };
        
        _repo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withdrawal);
        _aesService.Setup(x => x.Decrypt(It.IsAny<string>())).Throws(new Exception("Fail"));

        var result = await _handler.Handle(new GetWithdrawalByIdQuery(id), CancellationToken.None);

        result!.RawAccountNumber.Should().Be("MASKED");
    }
}
