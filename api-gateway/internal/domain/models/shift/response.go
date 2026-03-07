package shift

import (
	"api-gateway/internal/domain/models/store"
)

type ShiftResponse struct {
	Id                   string             `json:"id"`
	StoreId              string             `json:"storeId"`
	ShiftDate            string             `json:"shiftDate"` // YYYY-MM-DD
	StartTime            string             `json:"startTime"` // HH:mm
	EndTime              string             `json:"endTime"`   // HH:mm
	RequiredStaffCount   int32              `json:"requiredStaffCount"`
	BreakDurationMinutes int32              `json:"breakDurationMinutes"`
	ShiftTemplateId      *string            `json:"shiftTemplateId,omitempty"`
	Status               int32              `json:"status"` // 0:Draft, 1:Published, 2:Completed, 3:Cancelled
	Note                 *string            `json:"note,omitempty"`
	AssignedStaffCount   int32              `json:"assignedStaffCount"`
	Assignments          []*ShiftAssignment `json:"assignments,omitempty"`
}

type ShiftAssignment struct {
	Id         string   `json:"id"`
	ShiftId    string   `json:"shiftId"`
	UserId     string   `json:"userId"`
	Status     int32    `json:"status"`
	HourlyWage *float64 `json:"hourlyWage,omitempty"`
	FirstName  string   `json:"firstName"`
	LastName   *string  `json:"lastName,omitempty"`
	AvatarUrl  *string  `json:"avatarUrl,omitempty"`
	Email      string   `json:"email"`
}

// ===============================
// My schedule & Agenda (Staff)
// ===============================

type CoWorkerDTO struct {
	Id        string  `json:"id"`
	FirstName string  `json:"firstName"`
	LastName  *string `json:"lastName,omitempty"`
	Avatar    *string `json:"avatar,omitempty"`
	Phone     *string `json:"phone,omitempty"`
}

type StaffShiftAgenda struct {
	Id string `json:"id"` // assignmentId

	StartTime  string                    `json:"startTime"`
	EndTime    string                    `json:"endTime"`
	Date       string                    `json:"date"`
	Store      store.StoreDetailResponse `json:"store"`
	CoWorkers  []CoWorkerDTO             `json:"coWorkers"`
	CheckInAt  *string                   `json:"checkInAt,omitempty"`
	CheckOutAt *string                   `json:"checkOutAt,omitempty"`

	ShiftId string `json:"shiftId"`
}

// ===============================
// Time log
// ===============================
type TimeLogResponse struct {
	Id                string `json:"id"`
	ShiftAssignmentId string `json:"shiftAssignmentId"`

	CheckInAt       *string  `json:"checkInAt,omitempty"`
	CheckInImageUrl *string  `json:"checkInImageUrl,omitempty"`
	CheckInLat      *float64 `json:"checkInLat,omitempty"`
	CheckInLong     *float64 `json:"checkInLong,omitempty"`

	CheckOutAt       *string  `json:"checkOutAt,omitempty"`
	CheckOutImageUrl *string  `json:"checkOutImageUrl,omitempty"`
	CheckOutLat      *float64 `json:"checkOutLat,omitempty"`
	CheckOutLong     *float64 `json:"checkOutLong,omitempty"`

	RegularMinutes  *int32 `json:"regularMinutes"`
	DailyOtMinutes  *int32 `json:"dailyOtMinutes,omitempty"`
	WeeklyOtMinutes *int32 `json:"weeklyOtMinutes,omitempty"`

	Status      int32   `json:"status"` // Map từ Enum TimeLogStatus
	ManagerNote *string `json:"managerNote,omitempty"`

	CreatedAt string `json:"createdAt"`
	UpdatedAt string `json:"updatedAt"`

	Staff StaffTimeLogRes `json:"staff"`
	Shift ShiftTimeLogRes `json:"shift"`
}

// StaffTimeLogRes: Thông tin nhân viên trong bản ghi chấm công
type StaffTimeLogRes struct {
	Id        string  `json:"id"`
	FirstName *string `json:"firstName,omitempty"`
	LastName  *string `json:"lastName,omitempty"`
	AvatarUrl *string `json:"avatarUrl,omitempty"`
	Email     *string `json:"email,omitempty"`
}

// ShiftTimeLogRes: Thông tin ca làm việc liên quan đến bản ghi chấm công
type ShiftTimeLogRes struct {
	Id        string  `json:"id"`
	ShiftDate string  `json:"shiftDate"` // YYYY-MM-DD
	StartTime string  `json:"startTime"` // HH:mm
	EndTime   string  `json:"endTime"`   // HH:mm
	Note      *string `json:"note,omitempty"`
}

// ===============================
// Tu dong xep ca (Auto-Fill)
// ===============================
type AutoFillShiftsResponse struct {
	Success          bool     `json:"success"`
	FilledCount      int32    `json:"filledCount"`      // Số slot đã điền được người
	TotalSlots       int32    `json:"totalSlots"`       // Tổng số slot trống cần xử lý
	UnfilledShiftIds []string `json:"unfilledShiftIds"` // Danh sách ID các ca vẫn "mồ côi"
}
