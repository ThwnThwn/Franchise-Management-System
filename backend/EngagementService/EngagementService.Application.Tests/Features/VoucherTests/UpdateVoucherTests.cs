using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.Voucher;
using EngagementService.Application.Features.Voucher.Commands.Update;
using EngagementService.Application.Persistences.Repository;
using FluentAssertions;
using MediatR;
using Moq;

namespace EngagementService.Application.Tests.Features.VoucherTests;

public class UpdateVoucherHandlerTests
{
    private readonly Mock<IVoucherRepository> _voucherRepoMock;
    private readonly UpdateVoucherHandler _handler;

    public UpdateVoucherHandlerTests()
    {
        _voucherRepoMock = new Mock<IVoucherRepository>();
        _handler = new UpdateVoucherHandler(_voucherRepoMock.Object);
    }

    // 1. Phủ nhánh: EndDate <= DateTimeOffset.UtcNow (Dòng 20-21)
    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenEndDateIsInvalid()
    {
        // Arrange
        var pastDate = DateTimeOffset.UtcNow.AddMinutes(-1); // Thời gian đã qua
        var request = new UpdateVoucherRequest
        {
            Id = Guid.NewGuid(),
            EndDate = pastDate
        };
        var command = new UpdateVoucherCommand(request);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage(VoucherMsg.InvalidEndDate);

        _voucherRepoMock.Verify(x => x.UpdateVoucherAsync(It.IsAny<UpdateVoucherRequest>()), Times.Never);
    }

    // 2. Phủ xanh Happy Path: EndDate hợp lệ (Dòng 22-24)
    [Fact]
    public async Task Handle_ShouldUpdateVoucher_WhenRequestIsValid()
    {
        // Arrange
        var futureDate = DateTimeOffset.UtcNow.AddDays(7); // 7 ngày tới
        var request = new UpdateVoucherRequest
        {
            Id = Guid.NewGuid(),
            EndDate = futureDate,
            DiscountValue = 10000
        };
        var command = new UpdateVoucherCommand(request);

        _voucherRepoMock.Setup(x => x.UpdateVoucherAsync(request))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        // Lưu ý: UpdateVoucherAsync nhận Request DTO trực tiếp theo code của Duy
        _voucherRepoMock.Verify(x => x.UpdateVoucherAsync(request), Times.Once);
    }

    // 3. Phủ xanh Happy Path: EndDate là null (Không có hạn dùng)
    [Fact]
    public async Task Handle_ShouldUpdateVoucher_WhenEndDateIsNull()
    {
        // Arrange
        var request = new UpdateVoucherRequest
        {
            Id = Guid.NewGuid(),
            EndDate = null
        };
        var command = new UpdateVoucherCommand(request);

        _voucherRepoMock.Setup(x => x.UpdateVoucherAsync(request))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _voucherRepoMock.Verify(x => x.UpdateVoucherAsync(request), Times.Once);
    }
}