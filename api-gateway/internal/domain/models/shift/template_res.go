package shift

type ShiftTemplateResponse struct {
	Id                   string  `json:"id"`
	StoreId              *string `json:"storeId,omitempty"`
	Code                 string  `json:"code"`
	Name                 string  `json:"name"`
	StartTime            string  `json:"startTime"`
	EndTime              string  `json:"endTime"`
	BreakDurationMinutes int32   `json:"breakDurationMinutes"`
	IsActive             bool    `json:"isActive"`
	CreatedAt            string  `json:"createdAt"`
}
