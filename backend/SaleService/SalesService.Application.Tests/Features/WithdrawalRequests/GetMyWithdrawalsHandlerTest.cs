using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Moq;
using SalesService.Application.Dtos.WithdrawalRequests;
using SalesService.Application.Features.WithdrawalRequests.Queries.GetMyWithdrawals;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Tests.Features.WithdrawalRequests;

public class GetMyWithdrawalsHandlerTest
{
    private readonly Mock<IWithdrawalRequestRepository> _repo = new();
    private readonly GetMyWithdrawalsHandler _handler;

    public GetMyWithdrawalsHandlerTest()
    {
        _handler = new GetMyWithdrawalsHandler(_repo.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnPagedResults_When_DataExists()
    {
        var userId = Guid.NewGuid();
        var query = new GetMyWithdrawalsQuery(userId, WithdrawalStatus.Completed, 1, 10);
        
        var withdrawal = new WithdrawalRequest 
        { 
            Id = Guid.NewGuid(), 
            RequestCode = "WDR-MY-1", 
            UserId = userId, 
            Amount = 500, 
            FeeAmount = 0, 
            BankCode = "BANK", 
            Status = WithdrawalStatus.Completed,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        var pagedEntities = PagedResult<WithdrawalRequest>.Create(
            new List<WithdrawalRequest> { withdrawal },
            1, 1, 10);
            
        _repo.Setup(x => x.GetPagedWithdrawalsAsync(userId, WithdrawalStatus.Completed, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedEntities);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].RequestCode.Should().Be("WDR-MY-1");
        result.TotalCount.Should().Be(1);
    }
}
