package shift

import (
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/common"
)

type GetShiftsParams struct {
	StoreID  string  `form:"storeId" json:"storeId"`
	FromDate *string `form:"fromDate" json:"fromDate,omitempty"`
	ToDate   *string `form:"toDate" json:"toDate,omitempty"`
	Status   *int    `form:"status" json:"status,omitempty"` // ShiftStatus enum
}

// CreateShiftRequest (StoreId & CreatedBy lấy từ Context/JWT nếu cần, hoặc StoreId ở Body nếu Admin tạo)
type CreateShiftRequest struct {
	StoreId              string  `json:"storeId" binding:"required"`
	ShiftTemplateId      *string `json:"shiftTemplateId,omitempty"`
	ShiftDate            string  `json:"shiftDate" binding:"required"`
	StartTime            string  `json:"startTime" binding:"required"`
	EndTime              string  `json:"endTime" binding:"required"`
	RequiredStaffCount   int32   `json:"requiredStaffCount" binding:"required"`
	Note                 *string `json:"note"`
	BreakDurationMinutes int32   `json:"breakDurationMinutes"`
}

// UpdateShiftRequest (Id lấy từ Path)
type UpdateShiftRequest struct {
	// StoreId              *string `json:"storeId,omitempty"`
	ShiftTemplateId      *string `json:"shiftTemplateId,omitempty"`
	ShiftDate            *string `json:"shiftDate,omitempty"`
	StartTime            *string `json:"startTime,omitempty"`
	EndTime              *string `json:"endTime,omitempty"`
	RequiredStaffCount   *int32  `json:"requiredStaffCount,omitempty"`
	Note                 *string `json:"note,omitempty"`
	BreakDurationMinutes *int32  `json:"breakDurationMinutes,omitempty"`
	Status               *int32  `json:"status,omitempty"`
	// LockVersion          int32   `json:"lockVersion"` // Optimistic Locking
}

// ===============================
// My schedule (Staff)
// ===============================

type GetAgendaParams struct {
	StartDate string `form:"startDate" json:"startDate" binding:"required"`
	EndDate   string `form:"endDate" json:"endDate" binding:"required"`
}

// ===============================
// Time log
// ===============================
type GetTimeLogsParams struct {
	common.PaginationQuery
	StoreId *string             `json:"storeId,omitempty" form:"storeId"`
	StaffId *string             `json:"staffId,omitempty" form:"staffId"`
	Status  *enum.TimeLogStatus `json:"status,omitempty" form:"status"`

	// Format: YYYY-MM-DD
	FromDate string `json:"fromDate,omitempty" form:"fromDate"`
	ToDate   string `json:"toDate,omitempty" form:"toDate"`
}
