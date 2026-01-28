package errorcode

import (
	"errors"
	"net/http"
)

var (
	// 400
	ErrInvalidId = errors.New("invalid id")

	// 401
	ErrInvalidToken       = errors.New("invalid token")
	ErrInvalidAccessToken = errors.New("invalid access token")

	// 429
	ErrOTPRateLimit       = errors.New("otp rate limit")
	ErrOTPTooManyAttempts = errors.New("maximum otp attempts reached")

	// 500
	ErrUnexpectedSigningToken = errors.New("unexpected signing token")
)

// Map lỗi nội bộ sang HTTP Status và Title
var InternalErrorMap = map[error]struct {
	Status int
	Title  string
	Detail string
}{
	ErrInvalidId: {
		Status: http.StatusBadRequest,
		Title:  "Yêu cầu không hợp lệ",
		Detail: ErrInvalidId.Error(),
	},
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
}
