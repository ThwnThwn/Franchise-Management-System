using FluentAssertions;
using Moq;
using SalesService.Application.Features.WithdrawalRequests.Queries.GetWithdrawalById;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Application.Interfaces;
using SalesService.Application.Interfaces.Identity;
using SalesService.Domain.Entities;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Tests.Features.WithdrawalRequests;

public class GetWithdrawalByIdHandlerTests
{
    private readonly Mock<IWithdrawalRequestRepository> _repo = new();
    private readonly Mock<IAesEncryptionService> _aesService = new();
    private readonly Mock<IIdentityServiceClient> _identityClient = new();
    private readonly GetWithdrawalByIdHandler _handler;

    public GetWithdrawalByIdHandlerTests()
    {
        _handler = new GetWithdrawalByIdHandler(_repo.Object, _aesService.Object, _identityClient.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnNull_When_NotExist()
    {
        var id = Guid.NewGuid();
        _repo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WithdrawalRequest?)null);

        var query = new GetWithdrawalByIdQuery(id);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_ReturnDto_When_Exists()
    {
        var id = Guid.NewGuid();
        var withdrawal = new WithdrawalRequest { 
            Id = id, 
            RequestCode = "WDR1", 
            Amount = 100,
            EncryptedAccountNumber = "encrypted"
        };
        _repo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withdrawal);
        _aesService.Setup(x => x.Decrypt("encrypted")).Returns("123456");

        var query = new GetWithdrawalByIdQuery(id);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.RequestCode.Should().Be("WDR1");
        result.RawAccountNumber.Should().Be("123456");
    }
}
