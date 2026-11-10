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

func NewTooManyRequest(detail string) error {
	return &ErrorResponse{
		Status: http.StatusTooManyRequests,
		Title:  "Quá nhiều yêu cầu",
		Detail: detail,
	}
}

func NewInternalError(detail string) error {
	return &ErrorResponse{
		Status: http.StatusInternalServerError,
		Title:  "Lỗi hệ thống",
		Detail: detail,
	}
}
