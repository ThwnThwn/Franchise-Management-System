package shift

type GetShiftTemplatesParams struct {
	StoreId  *string `form:"storeId" json:"storeId,omitempty"`
	IsActive *bool   `form:"isActive" json:"isActive,omitempty"`
	Keyword  *string `form:"keyword" json:"keyword,omitempty"`
}

type CreateShiftTemplateRequest struct {
	StoreId              *string `json:"storeId,omitempty"`
	Code                 string  `json:"code" binding:"required"`
	Name                 string  `json:"name" binding:"required"`
	StartTime            string  `json:"startTime" binding:"required"`
	EndTime              string  `json:"endTime" binding:"required"`
	BreakDurationMinutes int32   `json:"breakDurationMinutes"`
}

type UpdateShiftTemplateRequest struct {
	Code                 *string `json:"code,omitempty"`
	Name                 *string `json:"name,omitempty"`
	StartTime            *string `json:"startTime,omitempty"`
	EndTime              *string `json:"endTime,omitempty"`
	BreakDurationMinutes *int32  `json:"breakDurationMinutes,omitempty"`
	IsActive             *bool   `json:"isActive,omitempty"`
}
