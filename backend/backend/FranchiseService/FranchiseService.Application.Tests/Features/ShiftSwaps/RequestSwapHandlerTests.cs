using FluentAssertions;
using Moq;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.ShiftSwaps.Commands.RequestSwap;
using FranchiseService.Application.Features.ShiftSwaps.Helpers;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Application.Tests.Features.ShiftSwaps;

public class RequestSwapHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftSwapRequest>> _swapRepoMock;
    private readonly Mock<IGenericRepository<ShiftAssignment>> _assignmentRepoMock;
    private readonly Mock<IGenericRepository<Shift>> _shiftRepoMock;
    private readonly Mock<IShiftRuleService> _ruleServiceMock;
    private readonly Mock<IIdentityServiceClient> _identityClientMock;
    private readonly List<ShiftSwapRequest> _addedSwaps;
    private readonly RequestSwapHandler _handler;
    private readonly ShiftAssignment _assignment;
    private readonly Shift _shift;

    public RequestSwapHandlerTests()
    {
        _swapRepoMock = new Mock<IGenericRepository<ShiftSwapRequest>>();
        _assignmentRepoMock = new Mock<IGenericRepository<ShiftAssignment>>();
        _shiftRepoMock = new Mock<IGenericRepository<Shift>>();
        _ruleServiceMock = new Mock<IShiftRuleService>();
        _identityClientMock = new Mock<IIdentityServiceClient>();
        _addedSwaps = new List<ShiftSwapRequest>();

        _assignment = new ShiftAssignment
        {
            Id = Guid.NewGuid(),
            ShiftId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TimeLogs = new List<TimeLog>()
        };
        _shift = new Shift
        {
            Id = _assignment.ShiftId,
            StoreId = Guid.NewGuid(),
            ShiftDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            Status = (int)ShiftStatus.Published,
            ShiftAssignments = new List<ShiftAssignment>()
        };

        _assignmentRepoMock.Setup(r => r.GetByIdAsync(_assignment.Id)).ReturnsAsync(_assignment);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(_assignment.ShiftId)).ReturnsAsync(_shift);
        _ruleServiceMock.Setup(r => r.GetEffectiveRuleAsync(_shift.StoreId)).ReturnsAsync(new ShiftRule { AllowShiftSwap = true });

        _swapRepoMock
            .Setup(r => r.AddAsync(It.IsAny<ShiftSwapRequest>()))
            .Callback<ShiftSwapRequest>(s => _addedSwaps.Add(s))
            .ReturnsAsync(Guid.NewGuid());

        var helper = new SwapValidationHelper(_assignmentRepoMock.Object, _shiftRepoMock.Object, _ruleServiceMock.Object);
        _handler = new RequestSwapHandler(helper, _swapRepoMock.Object, _assignmentRepoMock.Object, _identityClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenSourceUserHasPendingSwap()
    {
        var targetUserId = Guid.NewGuid();
        _swapRepoMock
            .SetupSequence(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, bool>>>() ))
            .ReturnsAsync(new[]
            {
                new ShiftSwapRequest { Id = Guid.NewGuid(), SourceUserId = _assignment.UserId, Status = (int)SwapRequestStatus.PendingTarget, Reason = "pending" }
            });

        var act = () => _handler.Handle(new RequestSwapCommand
        {
            AssignmentId = _assignment.Id,
            SourceUserId = _assignment.UserId,
            TargetUserId = targetUserId,
            Reason = "Muon doi ca Arabica"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cho xu ly*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTargetUserNotInSameStore()
    {
        var targetUserId = Guid.NewGuid();
        _swapRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftSwapRequest>());
        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(targetUserId))
            .ReturnsAsync(new UserInfo { Id = targetUserId, HomeStoreId = Guid.NewGuid(), FirstName = "Ngoai cua hang" });

        var act = () => _handler.Handle(new RequestSwapCommand
        {
            AssignmentId = _assignment.Id,
            SourceUserId = _assignment.UserId,
            TargetUserId = targetUserId,
            Reason = "Doi ca Robusta"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cung cua hang*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTargetAlreadyAssignedInShift()
    {
        var targetUserId = Guid.NewGuid();
        _swapRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftSwapRequest>());
        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(targetUserId))
            .ReturnsAsync(new UserInfo { Id = targetUserId, HomeStoreId = _shift.StoreId, FirstName = "Ban da duoc gan" });
        _assignmentRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(new[]
            {
                new ShiftAssignment { Id = Guid.NewGuid(), ShiftId = _shift.Id, UserId = targetUserId, TimeLogs = new List<TimeLog>() }
            });

        var act = () => _handler.Handle(new RequestSwapCommand
        {
            AssignmentId = _assignment.Id,
            SourceUserId = _assignment.UserId,
            TargetUserId = targetUserId,
            Reason = "Doi ca Sua dac"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*da duoc gan ca nay*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTargetHasPendingSwap()
    {
        var targetUserId = Guid.NewGuid();
        _swapRepoMock
            .SetupSequence(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftSwapRequest>())
            .ReturnsAsync(new[]
            {
                new ShiftSwapRequest { Id = Guid.NewGuid(), SourceUserId = targetUserId, Status = (int)SwapRequestStatus.PendingManager, Reason = "pending" }
            });

        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(targetUserId))
            .ReturnsAsync(new UserInfo { Id = targetUserId, HomeStoreId = _shift.StoreId, FirstName = "Dang pending" });

        _assignmentRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftAssignment>());

        var act = () => _handler.Handle(new RequestSwapCommand
        {
            AssignmentId = _assignment.Id,
            SourceUserId = _assignment.UserId,
            TargetUserId = targetUserId,
            Reason = "Doi ca phin"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*yeu cau doi ca khac*");
    }

    [Fact]
    public async Task Handle_ShouldCreateSwapRequest_WhenInputValid()
    {
        var targetUserId = Guid.NewGuid();
        _swapRepoMock
            .SetupSequence(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftSwapRequest, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftSwapRequest>())
            .ReturnsAsync(Enumerable.Empty<ShiftSwapRequest>());

        _identityClientMock
            .Setup(c => c.GetUserByIdAsync(targetUserId))
            .ReturnsAsync(new UserInfo { Id = targetUserId, HomeStoreId = _shift.StoreId, FirstName = "Hop le" });

        _assignmentRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>() ))
            .ReturnsAsync(Enumerable.Empty<ShiftAssignment>());

        var result = await _handler.Handle(new RequestSwapCommand
        {
            AssignmentId = _assignment.Id,
            SourceUserId = _assignment.UserId,
            TargetUserId = targetUserId,
            Reason = "Doi ca toi pha Arabica"
        }, CancellationToken.None);

        result.SwapRequestId.Should().NotBeEmpty();
        _addedSwaps.Should().HaveCount(1);
        _addedSwaps[0].Status.Should().Be((int)SwapRequestStatus.PendingTarget);
        _addedSwaps[0].Reason.Should().Contain("Arabica");
    }
}
