package shift

type AttendancePayload struct {
	Latitude  *float64 `json:"latitude,omitempty"`
	Longitude *float64 `json:"longitude,omitempty"`
	Note      *string  `json:"note,omitempty"`
}
