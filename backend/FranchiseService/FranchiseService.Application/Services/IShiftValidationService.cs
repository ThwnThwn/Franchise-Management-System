using FranchiseService.Domain.Commons;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Services
{
    /// <summary>
    /// Validate 1 user cho 1 shift. Trả list warnings (rỗng = hợp lệ).
    /// Dùng chung cho: AssignStaff, GetCandidates, AutoFill, ApplyForShift.
    /// </summary>
    public interface IShiftValidationService
    {
        Task<List<ValidationWarning>> ValidateAssignment(Shift shift, Guid userId, int minHoursBreak, int maxHoursPerWeek, CancellationToken ct);
        /// <summary>
        /// Batch validate cho GetAvailableStaffs (dùng in-memory data, không query thêm).
        /// </summary>
        CandidateValidation ValidateCandidate(Shift shift, List<ShiftAssignment> userAssignments, int minHoursBreak, int maxHoursPerWeek);

        //Static helpers dùng chung
        static bool IsOverlap(TimeOnly s1, TimeOnly e1, TimeOnly s2, TimeOnly e2)
        {
            int start1 = s1.Hour * 60 + s1.Minute, end1 = e1.Hour * 60 + e1.Minute;
            if (end1 <= start1) end1 += 1440;
            int start2 = s2.Hour * 60 + s2.Minute, end2 = e2.Hour * 60 + e2.Minute;
            if (end2 <= start2) end2 += 1440;
            return start1 < end2 && end1 > start2;
        }
        static double CalculateNetHours(TimeOnly start, TimeOnly end, int breakMinutes)
        {
            double grossHours;
            if (end <= start)
                grossHours = (1440 - (start.Hour * 60 + start.Minute) + (end.Hour * 60 + end.Minute)) / 60.0;
            else
                grossHours = (end.Hour * 60 + end.Minute - start.Hour * 60 - start.Minute) / 60.0;
            return Math.Max(0, grossHours - (breakMinutes / 60.0));
        }
    }

    /// <summary>
    /// Kết quả validate cho GetAvailableStaffs (trả về trực tiếp cho FE hiện UI).
    /// </summary>
    public class CandidateValidation
    {
        public bool IsConflict { get; set; }
        public bool IsRestTimeInvalid { get; set; }
        public double RestHours { get; set; }
        public bool IsOverLimit { get; set; }
        public double WeeklyHours { get; set; }
    }
}
