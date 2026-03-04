package errorcode

import "net/http"

func NewBadRequest(detail string) error {
	return &ErrorResponse{
		Status: http.StatusBadRequest,
		Title:  "Thao tác không thành công",
		Detail: detail,
	}
}

func NewNotFound(detail string) error {
	return &ErrorResponse{
		Status: http.StatusNotFound,
		Title:  "Không tìm thấy dữ liệu",
		Detail: detail,
	}
}
