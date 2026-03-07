package shift

type ShiftApplicationItem struct {
	Id                string  `json:"id"`
	ShiftId           string  `json:"shiftId"`
	UserId            string  `json:"userId"`
	Status            int     `json:"status"` // Map từ Enum ShiftApplicationStatus
	Note              string  `json:"note"`
	AppliedAt         string  `json:"appliedAt"`
	UserName          string  `json:"userName"`
	UserEmail         string  `json:"userEmail"`
	AvatarUrl         string  `json:"avatarUrl"`
	HourlyWage        float64 `json:"hourlyWage"`
	SmartScore        float64 `json:"smartScore"`        // Điểm thông minh (0.0 - 1.0)
	WeeklyHoursWorked float64 `json:"weeklyHoursWorked"` // Tổng giờ đã làm trong tuần
}
