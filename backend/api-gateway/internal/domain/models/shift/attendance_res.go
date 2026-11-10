package shift

type UserCurrentShiftResponse struct {
	HasShiftToday bool `json:"hasShiftToday"`

	AssignmentId *string `json:"assignmentId,omitempty"`
	ShiftId      *string `json:"shiftId,omitempty"`
	CurrentLogId *string `json:"currentLogId,omitempty"`

	StoreId   string   `json:"storeId"`
	StoreName string   `json:"storeName"`
	StoreLat  *float64 `json:"storeLat,omitempty"`
	StoreLong *float64 `json:"storeLong,omitempty"`

	StartTime string `json:"startTime"`
	EndTime   string `json:"endTime"`

	CanCheckIn  bool `json:"canCheckIn"`
	CanCheckOut bool `json:"canCheckOut"`
	IsOvertime  bool `json:"isOvertime"`

	CheckInAt  *string `json:"checkInAt,omitempty"`
	CheckOutAt *string `json:"checkOutAt,omitempty"`
	ServerTime string  `json:"serverTime"`
}
