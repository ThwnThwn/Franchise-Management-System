using FluentAssertions;
using Moq;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Services;

public class ShiftApplicationAutoRejectServiceTests
{
    private readonly Mock<IGenericRepository<ShiftApplication>> _appRepoMock;
    private readonly ShiftApplicationAutoRejectService _service;

    public ShiftApplicationAutoRejectServiceTests()
    {
        _appRepoMock = new Mock<IGenericRepository<ShiftApplication>>();
        _service = new ShiftApplicationAutoRejectService(_appRepoMock.Object);
    }

    [Fact]
    public async Task RejectPendingApplicationsAsync_ShouldRejectAllPending_WhenNoExcludeId()
    {
        var shiftId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        var pendingApps = new List<ShiftApplication>
        {
            new()
            {
                Id = Guid.NewGuid(), ShiftId = shiftId,
                UserId = Guid.NewGuid(),
                Status = (int)ShiftApplicationStatus.Pending,
                Note = "Em ranh ca chieu, xin nhan ca pha Espresso",
                Shift = null!
            },
            new()
            {
                Id = Guid.NewGuid(), ShiftId = shiftId,
                UserId = Guid.NewGuid(),
                Status = (int)ShiftApplicationStatus.Pending,
                Note = "Em xin lam ca pha Cappuccino",
                Shift = null!
            }
        };

        _appRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftApplication, bool>>>()))
            .ReturnsAsync(pendingApps);

        await _service.RejectPendingApplicationsAsync(shiftId, null, managerId);

        foreach (var app in pendingApps)
        {
            app.Status.Should().Be((int)ShiftApplicationStatus.Rejected);
            app.ReviewedBy.Should().Be(managerId);
            app.ReviewNote.Should().Contain("du nguoi");
        }

        _appRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ShiftApplication>()), Times.Exactly(2));
    }

    [Fact]
    public async Task RejectPendingApplicationsAsync_ShouldDoNothing_WhenNoPendingApps()
    {
        _appRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftApplication, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<ShiftApplication>());

        await _service.RejectPendingApplicationsAsync(Guid.NewGuid(), null, Guid.NewGuid());

        _appRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ShiftApplication>()), Times.Never);
    }

    [Fact]
    public async Task RejectPendingApplicationsAsync_ShouldExcludeSpecifiedApp()
    {
        var shiftId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var excludeId = Guid.NewGuid();

        var onlyApp = new ShiftApplication
        {
            Id = Guid.NewGuid(), ShiftId = shiftId,
            UserId = Guid.NewGuid(),
            Status = (int)ShiftApplicationStatus.Pending,
            Shift = null!
        };

        _appRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftApplication, bool>>>()))
            .ReturnsAsync(new[] { onlyApp });

        await _service.RejectPendingApplicationsAsync(shiftId, excludeId, managerId);

        _appRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ShiftApplication>()), Times.Once);
    }
}
