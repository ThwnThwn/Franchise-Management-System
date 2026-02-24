namespace FranchiseService.Application.Features.Shifts.DTOs
{
    // DTO dai dien cho mot ung vien nhan vien co the gan vao ca lam
    // Du lieu validation duoc tinh toan dua tren context cua ca lam (shiftId/thoi gian/cua hang)
    public class StaffAssignmentCandidateDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? AvatarUrl { get; set; }
        public decimal CurrentHourlyWage { get; set; }

        // TRANG THAI KHA DUNG (AVAILABILITY)
        // Dua tren lich dang ky ranh/ban cua nhan vien (Staff Availability)
        public bool IsAvailable { get; set; }

        // Ghi chu tu lich ranh (VD: "Chi lam duoc ca toi", "Ban viec gia dinh")
        public string? Note { get; set; }

        // TRANG THAI GAN CA
        // true neu nhan vien da nam trong danh sach gan cua ca nay
        public bool IsAssigned { get; set; }

        // RULE 1: CONFLICT TIME (TRUNG LICH)
        // true neu nhan vien da co mot ca lam khac giao thoa ve mat thoi gian voi ca hien tai
        public bool IsConflict { get; set; }

        // BO DU LIEU KIEM TRA QUY DINH (BUSINESS RULES VALIDATION)
        // BE tinh toan dua tren Settings cua Store (min_rest, max_hours, allow_shuttle)
        public CandidateValidationDto? Validation { get; set; }
    }

    // Ket qua kiem tra quy dinh cho tung ung vien
    public class CandidateValidationDto
    {
        // RULE 2: REST PERIOD (THOI GIAN NGHI)
        // true neu khoang nghi tu ca truoc den ca nay < quy dinh
        public bool IsRestTimeInvalid { get; set; }

        // So gio nghi thuc te giua 2 ca (Dung de FE render message)
        public double RestHours { get; set; }

        // RULE 3: WEEKLY CAP (GIOI HAN GIO TUAN)
        // true neu tong gio trong tuan (bao gom ca nay) > quy dinh
        public bool IsOverLimit { get; set; }

        // Tong so gio nhan vien da gan trong tuan hien tai (truoc ca nay)
        public double WeeklyHours { get; set; }

        // RULE 4: SHUTTLE CHECK (DIEU CHUYEN)
        // true neu: Ca o Store A, nhan vien thuoc Store B
        // NHUNG mot trong hai Store khong cho phep dieu chuyen (allow_shuttle = false)
        public bool IsShuttleInvalid { get; set; }
    }
}
