using EngagementService.Application.Features.Voucher.Queries.GetMyVoucher;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EngagementService.Application.Tests.Features.VoucherTests;

public class GetMyVoucherHandlerTests
{
    private readonly Mock<IUserVoucherRepository> _userVoucherRepoMock;
    private readonly GetMyVoucherHandler _handler;

    public GetMyVoucherHandlerTests()
    {
        _userVoucherRepoMock = new Mock<IUserVoucherRepository>();
        _handler = new GetMyVoucherHandler(_userVoucherRepoMock.Object);
    }

    // 1. Phủ xanh Happy Path: User có nhiều Voucher (có cả trùng ID và khác ID)
    [Fact]
    public async Task Handle_ShouldReturnGroupedVouchers_WithCorrectCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetMyVoucherQuery(userId);

        var voucherId1 = Guid.NewGuid();
        var voucherId2 = Guid.NewGuid();

        var voucher1 = new Domain.Entities.Voucher { Id = voucherId1, Code = "V1" };
        var voucher2 = new Domain.Entities.Voucher { Id = voucherId2, Code = "V2" };

        // Giả lập người dùng có 3 bản ghi UserVoucher: 2 cái loại V1, 1 cái loại V2
        var userVouchers = new List<Domain.Entities.UserVoucher>
        {
            new Domain.Entities.UserVoucher { UserId = userId, Voucher = voucher1 },
            new Domain.Entities.UserVoucher { UserId = userId, Voucher = voucher1 },
            new Domain.Entities.UserVoucher { UserId = userId, Voucher = voucher2 }
        };

        _userVoucherRepoMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userVouchers);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Group lại còn 2 loại voucher duy nhất

        // Kiểm tra logic cập nhật CurrentQuantity
        var resultV1 = result.First(v => v.Id == voucherId1);
        resultV1.CurrentQuantity.Should().Be(2); // 2 bản ghi trùng ID

        var resultV2 = result.First(v => v.Id == voucherId2);
        resultV2.CurrentQuantity.Should().Be(1); // 1 bản ghi duy nhất

        _userVoucherRepoMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
    }

    // 2. Phủ nhánh: Danh sách rỗng (Dòng 17 - userVouchers rỗng)
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoVouchers()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetMyVoucherQuery(userId);

        _userVoucherRepoMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<Domain.Entities.UserVoucher>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _userVoucherRepoMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
    }
}