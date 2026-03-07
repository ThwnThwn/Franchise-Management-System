package shift

// ShiftRuleSettingsResponse
// Trả về cấu hình rule Global và danh sách Rule theo Store
type ShiftRuleSettingsResponse struct {
	Global *ShiftRuleResponse   `json:"global"`
	Stores []*ShiftRuleResponse `json:"stores"`
}

// ShiftRuleResponse
// Chi tiết một bản ghi cấu hình quy tắc ca làm
type ShiftRuleResponse struct {
	Id      string  `json:"id"`
	StoreId *string `json:"storeId"`

	AllowShuttle           bool  `json:"allowShuttle"`
	AllowShiftSwap         bool  `json:"allowShiftSwap"`
	MinHoursBetweenShifts  int32 `json:"minHoursBetweenShifts"`
	MaxHoursPerWeek        int32 `json:"maxHoursPerWeek"`
	GracePeriodMinutes     int32 `json:"gracePeriodMinutes"`
	DailyOtThresholdHours  int32 `json:"dailyOtThresholdHours"`
	WeeklyOtThresholdHours int32 `json:"weeklyOtThresholdHours"`

	UpdatedAt string `json:"updatedAt"`
}
