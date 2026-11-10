package shift

type SwapRequestItem struct {
	Id             string  `json:"id"`
	SourceUserId   string  `json:"sourceUserId"`
	SourceUserName string  `json:"sourceUserName"`
	TargetUserId   string  `json:"targetUserId"`
	TargetUserName string  `json:"targetUserName"`
	Status         int32   `json:"status"` // Map từ Enum SwapStatus
	Reason         *string `json:"reason,omitempty"`
	RejectReason   *string `json:"rejectReason,omitempty"`
	ExpiresAt      string  `json:"expiresAt"`
	CreatedAt      string  `json:"createdAt"`
	ShiftId        string  `json:"shiftId"`
	ShiftDate      string  `json:"shiftDate"`
	StartTime      string  `json:"startTime"`
	EndTime        string  `json:"endTime"`
}
