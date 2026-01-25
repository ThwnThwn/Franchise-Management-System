package errorcode

import (
	"errors"
	"net/http"
)

var (
	// 401
	ErrInvalidToken = errors.New("invalid token")

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
	ErrInvalidToken: {
		Status: http.StatusUnauthorized,
		Title:  "Unauthorized",
		Detail: ErrInvalidToken.Error(),
	},
	ErrOTPRateLimit: {
		Status: http.StatusTooManyRequests,
		Title:  "Too Many Requests",
		Detail: ErrOTPRateLimit.Error(),
	},
	ErrOTPTooManyAttempts: {
		Status: http.StatusTooManyRequests,
		Title:  "Too Many Requests",
		Detail: ErrOTPTooManyAttempts.Error(),
	},
}
