package shift

import "api-gateway/internal/domain/models/common"

type GetAvailabilityParams struct {
	common.PaginationQuery
	FromDate    *string `form:"fromDate" json:"fromDate,omitempty"`
	ToDate      *string `form:"toDate" json:"toDate,omitempty"`
	IsAvailable *bool   `form:"isAvailable" json:"isAvailable,omitempty"`
	IsRecurring *bool   `form:"isRecurring" json:"isRecurring,omitempty"`
	DayOfWeek   *int32  `form:"dayOfWeek" json:"dayOfWeek,omitempty"`
	StoreId     *string `form:"storeId" json:"storeId,omitempty"`
	UserId      *string `form:"userId" json:"userId,omitempty"`
	Keyword     *string `form:"keyword" json:"keyword,omitempty"`
}

type CreateShiftAvailabilityRequest struct {
	IsAvailable  bool    `json:"isAvailable"`
	IsRecurring  bool    `json:"isRecurring"`
	SpecificDate *string `json:"specificDate,omitempty"`
	DayOfWeek    *int32  `json:"dayOfWeek,omitempty"`
	StartTime    string  `json:"startTime" binding:"required"`
	EndTime      string  `json:"endTime" binding:"required"`
	Note         *string `json:"note,omitempty"`
}

type UpdateShiftAvailabilityRequest struct {
	IsAvailable  *bool   `json:"isAvailable,omitempty"`
	IsRecurring  *bool   `json:"isRecurring,omitempty"`
	SpecificDate *string `json:"specificDate,omitempty"`
	DayOfWeek    *int32  `json:"dayOfWeek,omitempty"`
	StartTime    *string `json:"startTime,omitempty"`
	EndTime      *string `json:"endTime,omitempty"`
	Note         *string `json:"note,omitempty"`
}
