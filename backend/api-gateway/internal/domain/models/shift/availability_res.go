package shift

// ShiftAvailabilityResponse dùng để trả về thông tin lịch rảnh
type ShiftAvailabilityResponse struct {
	Id           string  `json:"id"`
	UserId       string  `json:"userId"`
	IsAvailable  bool    `json:"isAvailable"`
	IsRecurring  bool    `json:"isRecurring"`
	SpecificDate *string `json:"specificDate,omitempty"`
	DayOfWeek    *int32  `json:"dayOfWeek,omitempty"` // 0-6
	StartTime    string  `json:"startTime"`
	EndTime      string  `json:"endTime"`
	Note         *string `json:"note,omitempty"`
	CreatedAt    string  `json:"createdAt"`
	UpdatedAt    string  `json:"updatedAt"`
}
