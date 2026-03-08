package shift

// StaffCandidate: Ứng viên khả dụng cho ca làm
type StaffAssignmentCandidate struct {
	Id                string               `json:"id"`
	FirstName         string               `json:"firstName"`
	LastName          *string              `json:"lastName"`
	AvatarUrl         *string              `json:"avatarUrl"`
	CurrentHourlyWage float64              `json:"currentHourlyWage"`
	IsAvailable       bool                 `json:"isAvailable"`
	Note              *string              `json:"note"`
	IsAssigned        bool                 `json:"isAssigned"`
	IsConflict        bool                 `json:"isConflict"`
	Validation        *CandidateValidation `json:"validation"`
}

type CandidateValidation struct {
	IsRestTimeInvalid bool    `json:"isRestTimeInvalid"`
	RestHours         float64 `json:"restHours"`
	IsOverLimit       bool    `json:"isOverLimit"`
	WeeklyHours       float64 `json:"weeklyHours"`
	IsShuttleInvalid  bool    `json:"isShuttleInvalid"`
}

type AssignStaffResponse struct {
	Success  bool                    `json:"success"`
	Warnings []*ValidationWarningDto `json:"warnings"`
}
type ValidationWarningDto struct {
	Code     string `json:"code"`
	Message  string `json:"message"`
	Severity string `json:"severity"`
}
