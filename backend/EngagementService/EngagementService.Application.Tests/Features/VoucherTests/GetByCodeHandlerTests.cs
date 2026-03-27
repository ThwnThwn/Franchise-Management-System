using EngagementService.Application.Constants;
using EngagementService.Application.Features.Voucher.Queries.GetByCode;
using EngagementService.Application.Persistences.Repository;
using FluentAssertions;
using Moq;

namespace EngagementService.Application.Tests.Features.VoucherTests;

public class GetVoucherByCodeHandlerTests
{
    private readonly Mock<IVoucherRepository> _voucherRepoMock;
    private readonly GetVoucherByCodeHandler _handler;

    public GetVoucherByCodeHandlerTests()
    {
        _voucherRepoMock = new Mock<IVoucherRepository>();
        _handler = new GetVoucherByCodeHandler(_voucherRepoMock.Object);
    }

    // 1. Phủ nhánh: voucher == null (Dòng 17)
    [Fact]
    public async Task Handle_ShouldThrowDirectoryNotFoundException_WhenVoucherNotFound()
    {
        // Arrange
        var code = "DISCOUNT10";
        var query = new GetVoucherByCodeQuery(code);

        _voucherRepoMock.Setup(x => x.GetVoucherByCodeAsync(code))
            .ReturnsAsync((Domain.Entities.Voucher)null!);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>()
            .WithMessage(VoucherMsg.NotFound);
    }

    // 2. Phủ nhánh: voucher.IsActive == false (Dòng 17)
    [Fact]
    public async Task Handle_ShouldThrowDirectoryNotFoundException_WhenVoucherIsInactive()
    {
        // Arrange
        var code = "DISCOUNT10";
        var query = new GetVoucherByCodeQuery(code);
        var voucher = new Domain.Entities.Voucher { Code = code, IsActive = false };

        _voucherRepoMock.Setup(x => x.GetVoucherByCodeAsync(code))
            .ReturnsAsync(voucher);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>()
            .WithMessage(VoucherMsg.NotFound);
    }

    // 3. Phủ xanh Happy Path: Voucher hợp lệ và đang Active (Dòng 18)
    [Fact]
    public async Task Handle_ShouldReturnVoucher_WhenVoucherIsActive()
    {
        // Arrange
        var code = "DISCOUNT10";
        var query = new GetVoucherByCodeQuery(code);
        var voucher = new Domain.Entities.Voucher { Code = code, IsActive = true };

        _voucherRepoMock.Setup(x => x.GetVoucherByCodeAsync(code))
            .ReturnsAsync(voucher);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be(code);
        result.IsActive.Should().BeTrue();

        _voucherRepoMock.Verify(x => x.GetVoucherByCodeAsync(code), Times.Once);
    }
}