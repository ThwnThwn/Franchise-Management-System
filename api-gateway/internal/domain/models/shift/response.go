package shift

import (
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/store"
)

// ===============================
// Shift assignment
// ===============================

type ShiftAssignment struct {
	ID         string   `json:"id"`
	ShiftID    string   `json:"shiftId"`
	UserID     string   `json:"userId"`
	Status     int      `json:"status"` // ShiftAssignmentStatus enum
	HourlyWage *float64 `json:"hourlyWage"`

	FirstName string  `json:"firstName"`
	LastName  *string `json:"lastName,omitempty"`
	AvatarURL *string `json:"avatarUrl,omitempty"`
	Email     *string `json:"email,omitempty"`
}

type ShiftResponse struct {
	ID                   string             `json:"id"`
	StoreID              string             `json:"storeId"`
	ShiftDate            string             `json:"shiftDate"` // YYYY-MM-DD
	StartTime            string             `json:"startTime"` // HH:mm:ss
	EndTime              string             `json:"endTime"`
	RequiredStaffCount   int                `json:"requiredStaffCount"`
	BreakDurationMinutes int                `json:"breakDurationMinutes"`
	ShiftTemplateID      *string            `json:"shiftTemplateId,omitempty"`
	Status               int                `json:"status"` // ShiftStatus enum
	Note                 *string            `json:"note,omitempty"`
	AssignedStaffCount   int                `json:"assignedStaffCount"`
	Assignments          []*ShiftAssignment `json:"assignments"`
}

// ===============================
// Shift template
// ===============================

type ShiftTemplateResponse struct {
	ID                   string  `json:"id"`
	StoreID              *string `json:"storeId,omitempty"`
	Code                 string  `json:"code"`
	Name                 string  `json:"name"`
	StartTime            string  `json:"startTime"`
	EndTime              string  `json:"endTime"`
	BreakDurationMinutes int     `json:"breakDurationMinutes"`
	IsActive             bool    `json:"isActive"`
	CreatedAt            string  `json:"createdAt"`
}

// ===============================
// Shift assignment candidates
// ===============================

type CandidateValidation struct {
	IsRestTimeInvalid bool    `json:"isRestTimeInvalid"`
	RestHours         float64 `json:"restHours"`
	IsOverLimit       bool    `json:"isOverLimit"`
	WeeklyHours       float64 `json:"weeklyHours"`
	IsShuttleInvalid  bool    `json:"isShuttleInvalid"`
}

type StaffAssignmentCandidate struct {
	ID                string               `json:"id"`
	FirstName         string               `json:"firstName"`
	LastName          *string              `json:"lastName,omitempty"`
	AvatarURL         *string              `json:"avatarUrl,omitempty"`
	CurrentHourlyWage float64              `json:"currentHourlyWage"`
	IsAvailable       bool                 `json:"isAvailable"`
	Note              *string              `json:"note,omitempty"`
	IsAssigned        bool                 `json:"isAssigned"`
	IsConflict        bool                 `json:"isConflict"`
	Validation        *CandidateValidation `json:"validation,omitempty"`
}

// ===============================
// Shift rule
// ===============================

type ShiftRuleResponse struct {
	ID                    string  `json:"id"`
	StoreID               *string `json:"storeId,omitempty"`
	AllowShuttle          bool    `json:"allowShuttle"`
	AllowShiftSwap        bool    `json:"allowShiftSwap"`
	MinHoursBetweenShifts int     `json:"minHoursBetweenShifts"`
	MaxHoursPerWeek       int     `json:"maxHoursPerWeek"`
	UpdatedAt             string  `json:"updatedAt"`
}

type ShiftRuleSettingsResponse struct {
	Global ShiftRuleResponse   `json:"global"`
	Stores []ShiftRuleResponse `json:"stores"`
}

// ===============================
// Shift availability
// ===============================

type ShiftAvailabilityResponse struct {
	ID           string  `json:"id"`
	UserID       string  `json:"userId"`
	IsAvailable  bool    `json:"isAvailable"`
	IsRecurring  bool    `json:"isRecurring"`
	SpecificDate *string `json:"specificDate,omitempty"`
	DayOfWeek    *int    `json:"dayOfWeek,omitempty"` // 0-6
	StartTime    string  `json:"startTime"`
	EndTime      string  `json:"endTime"`
	Note         *string `json:"note,omitempty"`
	CreatedAt    string  `json:"createdAt"`
	UpdatedAt    string  `json:"updatedAt"`
}

// ===============================
// My schedule & Agenda (Staff)
// ===============================

type GetUserCurrentShiftResponse struct {
	HasShiftToday bool `json:"hasShiftToday"`

	// Dùng omitempty để FE nhận được undefined thay vì null
	AssignmentId *string `json:"assignmentId,omitempty"`
	ShiftId      *string `json:"shiftId,omitempty"`
	CurrentLogId *string `json:"currentLogId,omitempty"`

	// Thông tin Store
	StoreId   string   `json:"storeId"`
	StoreName string   `json:"storeName"`
	StoreLat  *float64 `json:"storeLat,omitempty"`
	StoreLong *float64 `json:"storeLong,omitempty"`

	// Giờ làm việc định mức
	StartTime string `json:"startTime"`
	EndTime   string `json:"endTime"`

	// Logic flags
	CanCheckIn  bool `json:"canCheckIn"`
	CanCheckOut bool `json:"canCheckOut"`
	IsOvertime  bool `json:"isOvertime"`

	// Timestamps
	CheckInAt  *string `json:"checkInAt,omitempty"`
	CheckOutAt *string `json:"checkOutAt,omitempty"`
	ServerTime string  `json:"serverTime"`
}

type CoWorkerDTO struct {
	ID        string  `json:"id"`
	FirstName string  `json:"firstName"`
	LastName  *string `json:"lastName,omitempty"`
	Avatar    *string `json:"avatar,omitempty"`
	Phone     *string `json:"phone,omitempty"`
}

type StaffShiftAgenda struct {
	ID         string                    `json:"id"` // assignmentId
	StartTime  string                    `json:"startTime"`
	EndTime    string                    `json:"endTime"`
	Date       string                    `json:"date"`
	Store      store.StoreDetailResponse `json:"store"`
	CoWorkers  []CoWorkerDTO             `json:"coWorkers"`
	CheckInAt  *string                   `json:"checkInAt,omitempty"`
	CheckOutAt *string                   `json:"checkOutAt,omitempty"`
}

// ===============================
// Time log
// ===============================
type TimeLogResponse struct {
	ID                string `json:"id"`
	ShiftAssignmentID string `json:"shiftAssignmentId"`

	// Check-in info (Optional)
	CheckInAt       *string  `json:"checkInAt,omitempty"`
	CheckInImageURL *string  `json:"checkInImageUrl,omitempty"`
	CheckInLat      *float64 `json:"checkInLat,omitempty"`
	CheckInLong     *float64 `json:"checkInLong,omitempty"`

	// Check-out info (Optional)
	CheckOutAt       *string  `json:"checkOutAt,omitempty"`
	CheckOutImageURL *string  `json:"checkOutImageUrl,omitempty"`
	CheckOutLat      *float64 `json:"checkOutLat,omitempty"`
	CheckOutLong     *float64 `json:"checkOutLong,omitempty"`

	TotalWorkMinutes *int               `json:"totalWorkMinutes,omitempty"`
	Status           enum.TimeLogStatus `json:"status"`
	ManagerNote      *string            `json:"managerNote,omitempty"`

	CreatedAt string `json:"createdAt"`
	UpdatedAt string `json:"updatedAt"`

	// Staff info
	Staff struct {
		Id        string  `json:"id"`
		FirstName *string `json:"firstName,omitempty"`
		LastName  *string `json:"lastName,omitempty"`
		AvatarUrl *string `json:"avatarUrl,omitempty"`
		Email     *string `json:"email,omitempty"`
	} `json:"staff"`

	Shift struct {
		Id        string  `json:"id"`
		ShiftDate string  `json:"shiftDate"`
		StartTime string  `json:"startTime"`
		EndTime   string  `json:"endTime"`
		Note      *string `json:"note"`
	} `json:"shift"`
}
