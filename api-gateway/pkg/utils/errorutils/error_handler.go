package errorutils

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/domain/models/errormodel"
	"errors"
	"net/http"

	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"
)

func MapErrorToProblem(err error) (int, string, string) {
	var appErr *errormodel.BackendError
	if errors.As(err, &appErr) {
		return appErr.Status, appErr.Title, appErr.Detail
	}

	for internalErr, info := range errorcode.InternalErrorMap {
		if errors.Is(err, internalErr) {
			return info.Status, info.Title, info.Detail
		}
	}

	st, ok := status.FromError(err)
	if ok {
		detail := st.Message()
		switch st.Code() {
		case codes.OK:
			return http.StatusOK, "Thành công", "Yêu cầu đã được xử lý"

		case codes.InvalidArgument: // 3 -> 400
			return http.StatusBadRequest, "Dữ liệu không hợp lệ", detail

		case codes.Unauthenticated: // 16 -> 401
			return http.StatusUnauthorized, "Chưa xác thực", "Vui lòng đăng nhập để tiếp tục"

		case codes.PermissionDenied: // 7 -> 403
			return http.StatusForbidden, "Không có quyền truy cập", "Bạn không có quyền thực hiện thao tác này"

		case codes.NotFound: // 5 -> 404
			return http.StatusNotFound, "Không tìm thấy", detail

		case codes.AlreadyExists: // 6 -> 409
			return http.StatusConflict, "Dữ liệu đã tồn tại", detail

		case codes.ResourceExhausted: // 8 -> 429
			return http.StatusTooManyRequests, "Quá nhiều yêu cầu", "Hệ thống đang bận, vui lòng thử lại sau"

		case codes.DeadlineExceeded: // 4 -> 504 (hoặc 408)
			return http.StatusGatewayTimeout, "Hết thời gian chờ", "Yêu cầu xử lý quá lâu"

		case codes.Unavailable: // 14 -> 503
			return http.StatusServiceUnavailable, "Dịch vụ tạm thời gián đoạn", "Máy chủ hiện không khả dụng, vui lòng thử lại sau"

		case codes.Aborted: // 10 -> 409
			return http.StatusConflict, "Thao tác bị hủy", detail

		case codes.Internal: // 13 -> 500
			fallthrough
		default:
			return http.StatusInternalServerError, "Lỗi máy chủ nội bộ", "Đã có lỗi không mong muốn xảy ra"
		}
	}

	return http.StatusInternalServerError, "Lỗi hệ thống", err.Error()
}
