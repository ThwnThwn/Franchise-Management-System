package shift

// ===============================
// Shift
// ===============================

type GetShiftsParams struct {
	StoreID  string  `form:"storeId" json:"storeId"`
	FromDate *string `form:"fromDate" json:"fromDate,omitempty"`
	ToDate   *string `form:"toDate" json:"toDate,omitempty"`
	Status   *int    `form:"status" json:"status,omitempty"` // ShiftStatus enum
}

type CreateShiftRequest struct {
	StoreID              string  `json:"storeId" binding:"required"`
	ShiftDate            string  `json:"shiftDate" binding:"required"` // YYYY-MM-DD
	StartTime            string  `json:"startTime" binding:"required"` // HH:mm
	EndTime              string  `json:"endTime" binding:"required"`
	RequiredStaffCount   int     `json:"requiredStaffCount" binding:"required"`
	BreakDurationMinutes int     `json:"breakDurationMinutes"`
	Note                 *string `json:"note,omitempty"`
	ShiftTemplateID      *string `json:"shiftTemplateId,omitempty"`
}

type UpdateShiftRequest struct {
	ShiftDate            *string `json:"shiftDate,omitempty"`
	StartTime            *string `json:"startTime,omitempty"`
	EndTime              *string `json:"endTime,omitempty"`
	RequiredStaffCount   *int    `json:"requiredStaffCount,omitempty"`
	BreakDurationMinutes *int    `json:"breakDurationMinutes,omitempty"`
	Note                 *string `json:"note,omitempty"`
	Status               *int    `json:"status,omitempty"`
	ShiftTemplateID      *string `json:"shiftTemplateId,omitempty"`
}

// ===============================
// Shift bulk
// ===============================

type BulkCreateManualRequest struct {
	StoreID              string  `json:"storeId" binding:"required"`
	StartTime            string  `json:"startTime" binding:"required"` // HH:mm
	EndTime              string  `json:"endTime" binding:"required"`
	DaysOfWeek           []int   `json:"daysOfWeek" binding:"required"` // [1, 3, 5]
	StartDate            string  `json:"startDate" binding:"required"`  // YYYY-MM-DD
	EndDate              string  `json:"endDate" binding:"required"`
	RequiredStaffCount   int     `json:"requiredStaffCount"`
	BreakDurationMinutes int     `json:"breakDurationMinutes"`
	Note                 *string `json:"note,omitempty"`
}

type BulkCreateFromTemplateRequest struct {
	StoreID            string `json:"storeId" binding:"required"`
	TemplateID         string `json:"templateId" binding:"required"`
	StartDate          string `json:"startDate" binding:"required"`
	EndDate            string `json:"endDate" binding:"required"`
	RequiredStaffCount int    `json:"requiredStaffCount"`
}

// ===============================
// Shift template
// ===============================

type GetShiftTemplatesParams struct {
	StoreID  *string `form:"storeId" json:"storeId,omitempty"`
	IsActive *bool   `form:"isActive" json:"isActive,omitempty"`
	Keyword  *string `form:"keyword" json:"keyword,omitempty"`
}

type CreateShiftTemplateRequest struct {
	StoreID              *string `json:"storeId,omitempty"`
	Code                 string  `json:"code" binding:"required"`
	Name                 string  `json:"name" binding:"required"`
	StartTime            string  `json:"startTime" binding:"required"`
	EndTime              string  `json:"endTime" binding:"required"`
	BreakDurationMinutes int     `json:"breakDurationMinutes"`
}

type UpdateShiftTemplateRequest struct {
	Code                 *string `json:"code,omitempty"`
	Name                 *string `json:"name,omitempty"`
	StartTime            *string `json:"startTime,omitempty"`
	EndTime              *string `json:"endTime,omitempty"`
	BreakDurationMinutes *int    `json:"breakDurationMinutes,omitempty"`
	IsActive             *bool   `json:"isActive,omitempty"`
}

// ===============================
// Shift rule
// ===============================

type SaveShiftRuleRequest struct {
	StoreID               *string `json:"storeId,omitempty"`
	AllowShuttle          bool    `json:"allowShuttle"`
	AllowShiftSwap        bool    `json:"allowShiftSwap"`
	MinHoursBetweenShifts int     `json:"minHoursBetweenShifts"`
	MaxHoursPerWeek       int     `json:"maxHoursPerWeek"`
}

// ===============================
// Shift availability
// ===============================

type GetAvailabilityParams struct {
	FromDate    *string `form:"fromDate" json:"fromDate,omitempty"`
	ToDate      *string `form:"toDate" json:"toDate,omitempty"`
	IsAvailable *bool   `form:"isAvailable" json:"isAvailable,omitempty"`
	IsRecurring *bool   `form:"isRecurring" json:"isRecurring,omitempty"`
	DayOfWeek   *int    `form:"dayOfWeek" json:"dayOfWeek,omitempty"`
	StoreID     *string `form:"storeId" json:"storeId,omitempty"`
	UserID      *string `form:"userId" json:"userId,omitempty"`
	Keyword     *string `form:"keyword" json:"keyword,omitempty"`
}

type CreateShiftAvailabilityRequest struct {
	IsAvailable  bool    `json:"isAvailable"`
	IsRecurring  bool    `json:"isRecurring"`
	SpecificDate *string `json:"specificDate,omitempty"`
	DayOfWeek    *int    `json:"dayOfWeek,omitempty"`
	StartTime    string  `json:"startTime" binding:"required"`
	EndTime      string  `json:"endTime" binding:"required"`
	Note         *string `json:"note,omitempty"`
}

type UpdateShiftAvailabilityRequest struct {
	IsAvailable  *bool   `json:"isAvailable,omitempty"`
	IsRecurring  *bool   `json:"isRecurring,omitempty"`
	SpecificDate *string `json:"specificDate,omitempty"`
	DayOfWeek    *int    `json:"dayOfWeek,omitempty"`
	StartTime    *string `json:"startTime,omitempty"`
	EndTime      *string `json:"endTime,omitempty"`
	Note         *string `json:"note,omitempty"`
}

// ===============================
// My schedule (Staff)
// ===============================

type AttendancePayload struct {
	Latitude  *float64 `json:"latitude,omitempty"`
	Longitude *float64 `json:"longitude,omitempty"`
	Note      *string  `json:"note,omitempty"`
}

type GetAgendaParams struct {
	StartDate string `form:"startDate" json:"startDate" binding:"required"`
	EndDate   string `form:"endDate" json:"endDate" binding:"required"`
}
