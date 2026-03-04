package errorcode

// 1. Struct duy nhất cho toàn bộ hệ thống
type ErrorResponse struct {
	Status int    `json:"status"`
	Title  string `json:"title"`
	Detail string `json:"detail"`
}

// 2. Hàm bắt buộc để Go công nhận đây là 1 error
func (e *ErrorResponse) Error() string {
	return e.Detail
}

// 3. Hàm Factory đa năng để đẻ ra lỗi linh hoạt ở bất cứ đâu
func NewErrorResponse(status int, title, detail string) error {
	return &ErrorResponse{
		Status: status,
		Title:  title,
		Detail: detail,
	}
}
