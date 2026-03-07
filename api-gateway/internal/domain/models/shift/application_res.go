package shift

type ShiftApplicationItem struct {
	Id                string  `json:"id"`
	ShiftId           string  `json:"shiftId"`
	UserId            string  `json:"userId"`
	Status            int32   `json:"status"` // Map từ Enum ShiftApplicationStatus
	Note              string  `json:"note"`
	AppliedAt         string  `json:"appliedAt"`
	Username          string  `json:"username"`
	Email             string  `json:"email"`
	AvatarUrl         string  `json:"avatarUrl"`
	HourlyWage        float64 `json:"hourlyWage"`
	SmartScore        float64 `json:"smartScore"`        // Điểm thông minh (0.0 - 1.0)
	WeeklyHoursWorked float64 `json:"weeklyHoursWorked"` // Tổng giờ đã làm trong tuần
}
