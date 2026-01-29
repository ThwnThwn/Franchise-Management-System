package errorcode

import (
	"errors"
	"net/http"
)

var (
	// 400
	ErrInvalidId = errors.New("Id không hợp lệ")

	// 401
	ErrInvalidToken       = errors.New("Token không hợp lệ")
	ErrInvalidAccessToken = errors.New("invalid access token")
	ErrMissingTokenInfo   = errors.New("Token thiếu thông tin yêu cầu")

	// 403
	ErrForbidden = errors.New("Không có quyền truy cập")

	// 429
	ErrOTPRateLimit       = errors.New("Quá số lần gửi OTP")
	ErrOTPTooManyAttempts = errors.New("Quá số lần nhập OTP")

	// 500
	ErrUnexpectedSigningToken = errors.New("Token không hợp lệ")
)

// Map lỗi nội bộ sang HTTP Status và Title
var InternalErrorMap = map[error]struct {
	Status int
	Title  string
	Detail string
}{
	// 400
	ErrInvalidId: {
		Status: http.StatusBadRequest,
		Title:  "Yêu cầu không hợp lệ",
		Detail: ErrInvalidId.Error(),
	},

	// 401
	ErrInvalidToken: {
		Status: http.StatusUnauthorized,
		Title:  "Lỗi xác thực",
		Detail: ErrInvalidToken.Error(),
	},
	ErrInvalidAccessToken: {
		Status: http.StatusUnauthorized,
		Title:  "Lỗi xác thực",
		Detail: ErrInvalidAccessToken.Error(),
	},

	// 403
	ErrForbidden: {
		Status: http.StatusForbidden,
		Title:  "Không có quyền truy cập",
		Detail: ErrForbidden.Error(),
	},

	// 429
	ErrOTPRateLimit: {
		Status: http.StatusTooManyRequests,
		Title:  "Quá nhiều yêu cầu",
		Detail: ErrOTPRateLimit.Error(),
	},
	ErrOTPTooManyAttempts: {
		Status: http.StatusTooManyRequests,
		Title:  "Quá nhiều yêu cầu",
		Detail: ErrOTPTooManyAttempts.Error(),
	},

	// 500
	ErrUnexpectedSigningToken: {
		Status: http.StatusInternalServerError,
		Title:  "Lỗi hệ thống",
		Detail: ErrUnexpectedSigningToken.Error(),
	},
}
