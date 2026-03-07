package shift

// SaveShiftRuleRequest
// Request body để lưu cấu hình rule
type SaveShiftRuleRequest struct {
	StoreId                *string `json:"storeId"`
	AllowShuttle           bool    `json:"allowShuttle"`
	AllowShiftSwap         bool    `json:"allowShiftSwap"`
	MinHoursBetweenShifts  int32   `json:"minHoursBetweenShifts"`
	MaxHoursPerWeek        int32   `json:"maxHoursPerWeek"`
	GracePeriodMinutes     int32   `json:"gracePeriodMinutes"`
	DailyOtThresholdHours  int32   `json:"dailyOtThresholdHours"`
	WeeklyOtThresholdHours int32   `json:"weeklyOtThresholdHours"`
}
