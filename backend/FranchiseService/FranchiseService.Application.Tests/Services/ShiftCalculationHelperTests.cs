using FluentAssertions;
using FranchiseService.Application.Services;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Services;

public class ShiftCalculationHelperTests
{
    [Fact]
    public void IsOverlap_ShouldReturnTrue_WhenTwoCaSangTrung()
    {
        var result = ShiftCalculationHelper.IsOverlap(
            new TimeOnly(6, 0), new TimeOnly(14, 0),
            new TimeOnly(10, 0), new TimeOnly(18, 0));

        result.Should().BeTrue();
    }

    [Fact]
    public void IsOverlap_ShouldReturnFalse_WhenCaSangVaCaChieuKhongTrung()
    {
        var result = ShiftCalculationHelper.IsOverlap(
            new TimeOnly(6, 0), new TimeOnly(14, 0),
            new TimeOnly(14, 0), new TimeOnly(22, 0));

        result.Should().BeFalse();
    }

    [Fact]
    public void IsOverlap_ShouldReturnTrue_WhenCaDemTrungVoiCaChieu()
    {
        var result = ShiftCalculationHelper.IsOverlap(
            new TimeOnly(22, 0), new TimeOnly(6, 0),
            new TimeOnly(23, 0), new TimeOnly(7, 0));

        result.Should().BeTrue();
    }

    [Fact]
    public void IsOverlap_ShouldReturnFalse_WhenCaDemVaCaSangKhongTrung()
    {
        var result = ShiftCalculationHelper.IsOverlap(
            new TimeOnly(22, 0), new TimeOnly(5, 0),
            new TimeOnly(6, 0), new TimeOnly(14, 0));

        result.Should().BeFalse();
    }

    [Fact]
    public void CalculateNetHours_ShouldReturn7_5_WhenCaSang8TiengNghi30Phut()
    {
        var result = ShiftCalculationHelper.CalculateNetHours(
            new TimeOnly(6, 0), new TimeOnly(14, 0), 30);

        result.Should().Be(7.5);
    }

    [Fact]
    public void CalculateNetHours_ShouldReturnCorrect_WhenCaDemQuaNuaDem()
    {
        var result = ShiftCalculationHelper.CalculateNetHours(
            new TimeOnly(22, 0), new TimeOnly(6, 0), 60);

        result.Should().Be(7.0);
    }

    [Fact]
    public void CalculateNetHours_ShouldReturnZero_WhenBreakLonHonCa()
    {
        var result = ShiftCalculationHelper.CalculateNetHours(
            new TimeOnly(9, 0), new TimeOnly(10, 0), 120);

        result.Should().Be(0);
    }

    [Theory]
    [InlineData(2025, 6, 2, 2025, 6, 2)]
    [InlineData(2025, 6, 4, 2025, 6, 2)]
    [InlineData(2025, 6, 8, 2025, 6, 2)]
    [InlineData(2025, 6, 1, 2025, 5, 26)]
    public void GetMondayOfWeek_ShouldReturnCorrectMonday(
        int year, int month, int day, int expectedYear, int expectedMonth, int expectedDay)
    {
        var date = new DateOnly(year, month, day);
        var result = ShiftCalculationHelper.GetMondayOfWeek(date);

        result.Should().Be(new DateOnly(expectedYear, expectedMonth, expectedDay));
    }

    [Fact]
    public void IsStaffBusy_ShouldReturnFalse_WhenKhongCoDuLieuLichRanhBan()
    {
        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2025, 6, 3),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var userId = Guid.NewGuid();
        var availsByUser = new Dictionary<Guid, List<StaffAvailability>>();

        var result = ShiftCalculationHelper.IsStaffBusy(shift, userId, availsByUser);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsStaffBusy_ShouldReturnTrue_WhenNhanVienBaoBanTrungCa()
    {
        var userId = Guid.NewGuid();
        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2025, 6, 3),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var availsByUser = new Dictionary<Guid, List<StaffAvailability>>
        {
            [userId] = new List<StaffAvailability>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    IsAvailable = false,
                    IsRecurring = false,
                    SpecificDate = new DateOnly(2025, 6, 3),
                    StartTime = new TimeOnly(7, 0),
                    EndTime = new TimeOnly(12, 0)
                }
            }
        };

        var result = ShiftCalculationHelper.IsStaffBusy(shift, userId, availsByUser);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsStaffBusy_ShouldReturnFalse_WhenNhanVienBaoRanhTrungCa()
    {
        var userId = Guid.NewGuid();
        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2025, 6, 3),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var availsByUser = new Dictionary<Guid, List<StaffAvailability>>
        {
            [userId] = new List<StaffAvailability>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    IsAvailable = true,
                    IsRecurring = false,
                    SpecificDate = new DateOnly(2025, 6, 3),
                    StartTime = new TimeOnly(7, 0),
                    EndTime = new TimeOnly(12, 0)
                }
            }
        };

        var result = ShiftCalculationHelper.IsStaffBusy(shift, userId, availsByUser);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsStaffBusy_ShouldReturnTrue_WhenLichLapLaiBanTrungThu()
    {
        var userId = Guid.NewGuid();
        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            ShiftDate = new DateOnly(2025, 6, 3),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var availsByUser = new Dictionary<Guid, List<StaffAvailability>>
        {
            [userId] = new List<StaffAvailability>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    IsAvailable = false,
                    IsRecurring = true,
                    DayOfWeek = (short)new DateOnly(2025, 6, 3).DayOfWeek,
                    StartTime = new TimeOnly(6, 0),
                    EndTime = new TimeOnly(18, 0)
                }
            }
        };

        var result = ShiftCalculationHelper.IsStaffBusy(shift, userId, availsByUser);
        result.Should().BeTrue();
    }

    [Fact]
    public void GetAssignmentsInWeek_ShouldReturnEmpty_WhenKhongCoPhanCong()
    {
        var userId = Guid.NewGuid();
        var assignsByUser = new Dictionary<Guid, List<ShiftAssignment>>();

        var result = ShiftCalculationHelper.GetAssignmentsInWeek(
            userId, new DateOnly(2025, 6, 2), new DateOnly(2025, 6, 8), assignsByUser);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAssignmentsInWeek_ShouldFilterCorrectWeek()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var mondayShift = new Shift
        {
            Id = Guid.NewGuid(), StoreId = storeId,
            ShiftDate = new DateOnly(2025, 6, 2),
            StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };
        var tuesdayShift = new Shift
        {
            Id = Guid.NewGuid(), StoreId = storeId,
            ShiftDate = new DateOnly(2025, 6, 3),
            StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };
        var nextWeekShift = new Shift
        {
            Id = Guid.NewGuid(), StoreId = storeId,
            ShiftDate = new DateOnly(2025, 6, 10),
            StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0),
            ShiftAssignments = new List<ShiftAssignment>()
        };

        var assignsByUser = new Dictionary<Guid, List<ShiftAssignment>>
        {
            [userId] = new List<ShiftAssignment>
            {
                new() { Id = Guid.NewGuid(), ShiftId = mondayShift.Id, UserId = userId, Shift = mondayShift, TimeLogs = new List<TimeLog>() },
                new() { Id = Guid.NewGuid(), ShiftId = tuesdayShift.Id, UserId = userId, Shift = tuesdayShift, TimeLogs = new List<TimeLog>() },
                new() { Id = Guid.NewGuid(), ShiftId = nextWeekShift.Id, UserId = userId, Shift = nextWeekShift, TimeLogs = new List<TimeLog>() }
            }
        };

        var result = ShiftCalculationHelper.GetAssignmentsInWeek(
            userId, new DateOnly(2025, 6, 2), new DateOnly(2025, 6, 8), assignsByUser);

        result.Should().HaveCount(2);
    }

    [Fact]
    public void MatchesDate_ShouldReturnTrue_WhenRecurringMatchesDayOfWeek()
    {
        var avail = new StaffAvailability
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsRecurring = true,
            DayOfWeek = (short)System.DayOfWeek.Tuesday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0)
        };

        var result = ShiftCalculationHelper.MatchesDate(avail, new DateOnly(2025, 6, 3));
        result.Should().BeTrue();
    }

    [Fact]
    public void MatchesDate_ShouldReturnFalse_WhenRecurringDoesNotMatchDayOfWeek()
    {
        var avail = new StaffAvailability
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsRecurring = true,
            DayOfWeek = (short)System.DayOfWeek.Wednesday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0)
        };

        var result = ShiftCalculationHelper.MatchesDate(avail, new DateOnly(2025, 6, 3));
        result.Should().BeFalse();
    }

    [Fact]
    public void MatchesDate_ShouldReturnTrue_WhenSpecificDateMatches()
    {
        var avail = new StaffAvailability
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsRecurring = false,
            SpecificDate = new DateOnly(2025, 6, 3),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0)
        };

        var result = ShiftCalculationHelper.MatchesDate(avail, new DateOnly(2025, 6, 3));
        result.Should().BeTrue();
    }

    [Fact]
    public void MatchesDate_ShouldReturnFalse_WhenSpecificDateDoesNotMatch()
    {
        var avail = new StaffAvailability
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsRecurring = false,
            SpecificDate = new DateOnly(2025, 6, 4),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0)
        };

        var result = ShiftCalculationHelper.MatchesDate(avail, new DateOnly(2025, 6, 3));
        result.Should().BeFalse();
    }
}
