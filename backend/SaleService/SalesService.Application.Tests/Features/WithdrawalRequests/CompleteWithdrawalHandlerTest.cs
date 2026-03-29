using FluentAssertions;
using Moq;
using SalesService.Application.Dtos;
using SalesService.Application.Features.WithdrawalRequests.Commands.CompleteWithdrawal;
using SalesService.Application.Interfaces;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SalesService.Application.Tests.Features.WithdrawalRequests;

public class CompleteWithdrawalHandlerTest
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IWithdrawalRequestRepository> _repo = new();
    private readonly Mock<IImageService> _imageService = new();
    private readonly CompleteWithdrawalHandler _handler;

    public CompleteWithdrawalHandlerTest()
    {
        _handler = new CompleteWithdrawalHandler(_uow.Object, _repo.Object, _imageService.Object);
    }

    [Fact]
    public async Task Handle_ShouldComplete_WhenApprovedAndNoFile()
    {
        var requestId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var cmd = new CompleteWithdrawalCommand(requestId, adminId, Array.Empty<byte>(), "BANK-TX-1");
        var withdrawal = new WithdrawalRequest { Id = requestId, Status = WithdrawalStatus.Approved, TransactionId = Guid.NewGuid() };

        _repo.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withdrawal);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Should().BeTrue();
        _repo.Verify(x => x.ApplyComplete(withdrawal, adminId, withdrawal.TransactionId.Value, "BANK-TX-1", null, null), Times.Once);
        _uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUploadImage_WhenFileProvided()
    {
        var requestId = Guid.NewGuid();
        var cmd = new CompleteWithdrawalCommand(requestId, Guid.NewGuid(), new byte[] { 1, 2, 3 }, "BANK-TX-1");
        var withdrawal = new WithdrawalRequest { Id = requestId, Status = WithdrawalStatus.Approved, RequestCode = "WDR-1" };

        _repo.Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(withdrawal);
        _imageService.Setup(x => x.UploadImageFromBytesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), "image/jpeg"))
            .ReturnsAsync(new ImageUploadResult { Url = "http://image.url", PublicId = "pid" });

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Should().BeTrue();
        _repo.Verify(x => x.ApplyComplete(It.IsAny<WithdrawalRequest>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), "http://image.url", "pid"), Times.Once);
    }
}
