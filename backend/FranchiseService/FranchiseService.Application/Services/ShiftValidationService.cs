using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Commons;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Services
{
    public class ShiftValidationService : IShiftValidationService
    {
        private readonly IShiftRepository _shiftRepo;
        public ShiftValidationService(IShiftRepository shiftRepo)
        {
            _shiftRepo = shiftRepo;
        }
        /// <summary>
        /// Validate bằng cách query DB (dùng cho AssignStaff, AutoFill, Apply).
        /// </summary>
        public async Task<List<ValidationWarning>> ValidateAssignment(
            Shift shift, Guid userId, int minHoursBreak, int maxHoursPerWeek,
            CancellationToken ct)
        {
            var warnings = new List<ValidationWarning>();
            // 1. Check Overlap → BLOCKING
            var dailyAssignments = await _shiftRepo.GetAssignmentsByUserAsync(
                userId, shift.ShiftDate, shift.ShiftDate, ct);
            foreach (var existing in dailyAssignments)
            {
                if (IShiftValidationService.IsOverlap(shift.StartTime, shift.EndTime,
                    existing.Shift.StartTime, existing.Shift.EndTime))
                {
                    warnings.Add(new ValidationWarning
                    {
                        Code = "OVERLAP",
                        Message = $"Trung gio voi ca {existing.Shift.StartTime:HH\\:mm}-{existing.Shift.EndTime:HH\\:mm}.",
                        Severity = "BLOCKING"
                    });
                }
            }
            // 2. Check Rest Time → WARNING
            var extendedAssignments = await _shiftRepo.GetAssignmentsByUserAsync(
                userId, shift.ShiftDate.AddDays(-1), shift.ShiftDate.AddDays(1), ct);
            var restCheck = CalculateMinRestHours(shift, extendedAssignments.ToList());
            if (restCheck.minGap < minHoursBreak)
            {
                warnings.Add(new ValidationWarning
                {
                    Code = "REST_TIME",
                    Message = $"Nghi chua du {minHoursBreak}h (chi co {restCheck.minGap:F1}h).",
                    Severity = "WARNING"
                });
            }
            // 3. Check Max Hours → WARNING
            int daysSinceMonday = (int)shift.ShiftDate.DayOfWeek - (int)DayOfWeek.Monday;
            if (daysSinceMonday < 0) daysSinceMonday += 7;
            var startOfWeek = shift.ShiftDate.AddDays(-daysSinceMonday);
            var endOfWeek = startOfWeek.AddDays(6);
            var weeklyAssignments = await _shiftRepo.GetAssignmentsByUserAsync(
                userId, startOfWeek, endOfWeek, ct);
            double totalHours = weeklyAssignments
                                .Sum(a => IShiftValidationService
                                            .CalculateNetHours(
                                             a.Shift.StartTime,
                                             a.Shift.EndTime,
                                             a.Shift.BreakDurationMinutes));

            double newHours = IShiftValidationService.CalculateNetHours(
                shift.StartTime, shift.EndTime, shift.BreakDurationMinutes);

            if (totalHours + newHours > maxHoursPerWeek)
            {
                warnings.Add(new ValidationWarning
                {
                    Code = "MAX_HOURS_WEEK",
                    Message = $"Vuot {maxHoursPerWeek}h/tuan ({totalHours:F1}h + {newHours:F1}h).",
                    Severity = "WARNING"
                });
            }
            return warnings;
        }
        /// <summary>
        /// Validate bằng in-memory data (dùng cho GetAvailableStaffs — đã batch fetch sẵn).
        /// </summary>
        public CandidateValidation ValidateCandidate(
            Shift shift, List<ShiftAssignment> userAssignments,
            int minHoursBreak, int maxHoursPerWeek)
        {
            var result = new CandidateValidation();
            // Overlap
            result.IsConflict = userAssignments.Any(a =>
                a.Shift.ShiftDate == shift.ShiftDate && a.ShiftId != shift.Id &&
                IShiftValidationService.IsOverlap(shift.StartTime, shift.EndTime,
                    a.Shift.StartTime, a.Shift.EndTime));


            // Rest Time
            var restResult = CalculateMinRestHours(shift,
                userAssignments.Where(a => a.ShiftId != shift.Id).ToList());
            result.IsRestTimeInvalid = restResult.minGap < minHoursBreak;
            result.RestHours = restResult.minGap;


            // Weekly Hours
            double weeklyHours = userAssignments
                .Where(a => a.ShiftId != shift.Id)
                .Sum(a => IShiftValidationService.CalculateNetHours(
                    a.Shift.StartTime, a.Shift.EndTime, a.Shift.BreakDurationMinutes));
            double newHours = IShiftValidationService.CalculateNetHours(
                shift.StartTime, shift.EndTime, shift.BreakDurationMinutes);
            result.IsOverLimit = (weeklyHours + newHours) > maxHoursPerWeek;
            result.WeeklyHours = weeklyHours;


            return result;
        }


        //Private Helpers
        private static (double minGap, bool hasNearby) CalculateMinRestHours(
            Shift newShift, IList<ShiftAssignment> existingAssignments)
        {
            if (!existingAssignments.Any()) return (double.MaxValue, false);

            var newStart = newShift.ShiftDate.ToDateTime(newShift.StartTime);
            var newEnd = newShift.ShiftDate.ToDateTime(newShift.EndTime);

            if (newShift.EndTime <= newShift.StartTime) newEnd = newEnd.AddDays(1);
            double minGap = double.MaxValue;

            foreach (var existing in existingAssignments)
            {
                var existStart = existing.Shift.ShiftDate.ToDateTime(existing.Shift.StartTime);
                var existEnd = existing.Shift.ShiftDate.ToDateTime(existing.Shift.EndTime);

                if (existing.Shift.EndTime <= existing.Shift.StartTime) existEnd = existEnd.AddDays(1);

                double gap;

                if (newStart >= existEnd) gap = (newStart - existEnd).TotalHours;
                else if (existStart >= newEnd) gap = (existStart - newEnd).TotalHours;
                else continue;
                if (gap < minGap) minGap = gap;
            }
            return (minGap, true);
        }
    }
}
