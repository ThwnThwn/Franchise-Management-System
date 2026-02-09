package errorcode

import (
	"api-gateway/global"
	"errors"
	"fmt"
	"net/http"
)

// Global error variables
var (
	// 400
	ErrInvalidId          error
	ErrInvalidImageFormat error
	ErrImageRequired      error

	// 401
	ErrInvalidToken       error
	ErrInvalidAccessToken error
	ErrMissingTokenInfo   error

	// 403
	ErrForbidden error

	// 494
	ErrTokenNotFound error

	// 413
	ErrFileTooLarge error

	// 429
	ErrOTPRateLimit       error
	ErrOTPTooManyAttempts error

	// 500
	ErrUnexpectedSigningToken error
)

type ErrorResponse struct {
	Status int
	Title  string
	Detail string
}

var InternalErrorMap map[error]ErrorResponse

// InitErrorMap initializes all error variables and their corresponding HTTP mapping.
// This must be called AFTER global.LoadConfig() in main.go.
func InitErrorMap() {
	// --- 400 Bad Request ---
	ErrInvalidId = errors.New("Id không hợp lệ")
	ErrInvalidImageFormat = errors.New("Vui lòng chỉ upload ảnh định dạng JPG, PNG hoặc WebP")
	ErrImageRequired = errors.New("Vui lòng chọn ảnh để tải lên")

	// --- 401 Unauthorized ---
	ErrInvalidToken = errors.New("Token không hợp lệ")
	ErrInvalidAccessToken = errors.New("invalid access token")
	ErrMissingTokenInfo = errors.New("Token thiếu thông tin yêu cầu")

	// --- 403 Forbidden ---
	ErrForbidden = errors.New("Không có quyền truy cập")

	// --- 404 Not Found ---
	ErrTokenNotFound = errors.New("không tìm thấy token")

	// --- 413 Payload Too Large ---
	// Dynamically generated based on Environment Variable
	ErrFileTooLarge = fmt.Errorf("Vui lòng chọn tệp nhỏ hơn %d MB", global.Config.MaxUploadSizeMb)

	// --- 429 Too Many Requests ---
	ErrOTPRateLimit = errors.New("Quá số lần gửi OTP")
	ErrOTPTooManyAttempts = errors.New("Quá số lần nhập OTP")

	// --- 500 Internal Server Error ---
	ErrUnexpectedSigningToken = errors.New("Token không hợp lệ")

	// Initialize the Internal Mapping Table
	InternalErrorMap = map[error]ErrorResponse{
		// --- 400 ---
		ErrInvalidId: {
			Status: http.StatusBadRequest,
			Title:  "Thao tác không thành công",
			Detail: ErrInvalidId.Error(),
		},
		ErrInvalidImageFormat: {
			Status: http.StatusBadRequest,
			Title:  "Thao tác không thành công",
			Detail: ErrInvalidImageFormat.Error(),
		},
		ErrImageRequired: {
			Status: http.StatusBadRequest,
			Title:  "Thao tác không thành công",
			Detail: ErrImageRequired.Error(),
		},

		// --- 401 ---
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
		ErrMissingTokenInfo: {
			Status: http.StatusUnauthorized,
			Title:  "Lỗi xác thực",
			Detail: ErrMissingTokenInfo.Error(),
		},

		// --- 403 ---
		ErrForbidden: {
			Status: http.StatusForbidden,
			Title:  "Không có quyền truy cập",
			Detail: ErrForbidden.Error(),
		},

		// --- 404 ---
		ErrTokenNotFound: {
			Status: http.StatusNotFound,
			Title:  "Không tìm thấy",
			Detail: ErrTokenNotFound.Error(),
		},

		// --- 413 ---
		ErrFileTooLarge: {
			Status: http.StatusRequestEntityTooLarge,
			Title:  "Tệp quá lớn",
			Detail: ErrFileTooLarge.Error(),
		},

		// --- 429 ---
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

		// --- 500 ---
		ErrUnexpectedSigningToken: {
			Status: http.StatusInternalServerError,
			Title:  "Lỗi hệ thống",
			Detail: ErrUnexpectedSigningToken.Error(),
		},
	}
}
