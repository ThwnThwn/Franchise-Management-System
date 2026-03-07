package shift

import "api-gateway/internal/domain/constants/enum"

type AttendancePayload struct {
	Latitude  *float64 `json:"latitude,omitempty"`
	Longitude *float64 `json:"longitude,omitempty"`
	Note      *string  `json:"note,omitempty"`
}

type UpdateTimeLogRequest struct {
	Status      enum.TimeLogStatus `json:"status" binding:"required"`
	ManagerNote *string            `json:"managerNote,omitempty"`
}
