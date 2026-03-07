package shift

// RequestSwapRequest: A gửi yêu cầu đổi ca cho B
// AssignmentId (ca của A) lấy từ Path Param
// SourceUserId lấy từ JWT
type RequestSwapRequest struct {
	TargetUserId string `json:"targetUserId" binding:"required"`
	Reason       string `json:"reason"`
}

// RespondSwapRequest: B đồng ý hoặc từ chối
// SwapRequestId lấy từ Path Param, UserId lấy từ JWT
type RespondSwapRequest struct {
	Accept       bool    `json:"accept"`
	RejectReason *string `json:"rejectReason,omitempty"`
}

// ApproveSwapRequest: Manager duyệt ca cuối cùng
// SwapRequestId lấy từ Path Param, ReviewedBy lấy từ JWT
type ApproveSwapRequest struct {
	Approve      bool    `json:"approve"`
	RejectReason *string `json:"rejectReason,omitempty"`
}
