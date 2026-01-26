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
		case codes.Unauthenticated: // 16
			return http.StatusUnauthorized, "Unauthorized", detail
		case codes.PermissionDenied: // 7
			return http.StatusForbidden, "Forbidden", detail
		case codes.InvalidArgument: // 3
			return http.StatusBadRequest, "Bad Request", detail
		case codes.NotFound: // 5
			return http.StatusNotFound, "Not Found", detail
		case codes.AlreadyExists: // 6
			return http.StatusConflict, "Conflict", detail
		case codes.DeadlineExceeded: // 4
			return http.StatusRequestTimeout, "Request Timeout", detail
		case codes.Unavailable: // 14
			return http.StatusServiceUnavailable, "Service Unavailable", "The server is currently unable to handle the request"
		case codes.ResourceExhausted: // 8
			return http.StatusTooManyRequests, "Too Many Requests", detail
		}
	}

	return http.StatusInternalServerError, "Internal Server Error", err.Error()
}

func GrpcToHttpStatus(err error) (int, string, string) {
	st, ok := status.FromError(err)
	if !ok {
		return http.StatusInternalServerError, "Internal Server Error", "An unexpected error occurred"
	}

	detail := st.Message()

	switch st.Code() {
	case codes.Unauthenticated: // 16
		return http.StatusUnauthorized, "Unauthorized", detail
	case codes.PermissionDenied: // 7
		return http.StatusForbidden, "Forbidden", detail
	case codes.InvalidArgument: // 3
		return http.StatusBadRequest, "Bad Request", detail
	case codes.NotFound: // 5
		return http.StatusNotFound, "Not Found", detail
	case codes.AlreadyExists: // 6
		return http.StatusConflict, "Conflict", detail
	case codes.DeadlineExceeded: // 4
		return http.StatusRequestTimeout, "Request Timeout", detail
	case codes.Unavailable: // 14
		return http.StatusServiceUnavailable, "Service Unavailable", "The server is currently unable to handle the request"
	default:
		return http.StatusInternalServerError, "Internal Server Error", detail
	}
}
