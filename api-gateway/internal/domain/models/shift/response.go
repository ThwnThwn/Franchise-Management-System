package shift

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
