package order

import (
	"api-gateway/internal/domain/constants/enum"
	"api-gateway/internal/domain/models/common"
)

type CreatePaymentRequest struct {
	ReturnUrl     *string            `json:"returnUrl"`
	PaymentMethod enum.PaymentMethod `json:"paymentMethod"`
}

type GetAllPaymentsReq struct {
	common.PaginationQuery
}

type GetMyPaymentHistoryReq struct {
	common.PaginationQuery

	StartDate *string `form:"startDate"` // ISO-8601
	EndDate   *string `form:"endDate"`
	Status    *int32  `form:"status"` // 0=Pending, 1=Paid, 2=Failed; omit for all
}
