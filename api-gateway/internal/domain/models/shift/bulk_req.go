package shift

type BulkCreateManualRequest struct {
	StoreId              string  `json:"storeId" binding:"required"`
	StartTime            string  `json:"startTime"`
	EndTime              string  `json:"endTime"`
	DaysOfWeek           []int32 `json:"daysOfWeek"`
	StartDate            string  `json:"startDate"`
	EndDate              string  `json:"endDate"`
	RequiredStaffCount   int32   `json:"requiredStaffCount"`
	BreakDurationMinutes int32   `json:"breakDurationMinutes"`
	Note                 *string `json:"note,omitempty"`
}

type BulkCreateFromTemplateRequest struct {
	StoreId            string `json:"storeId" binding:"required"`
	TemplateId         string `json:"templateId" binding:"required"`
	StartDate          string `json:"startDate" binding:"required"`
	EndDate            string `json:"endDate" binding:"required"`
	RequiredStaffCount int32  `json:"requiredStaffCount"`
}
