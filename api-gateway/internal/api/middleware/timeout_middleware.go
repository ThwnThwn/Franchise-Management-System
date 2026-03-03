package middleware

import (
	"context"
	"time"

	"github.com/gin-gonic/gin"
)

// GlobalTimeoutMiddleware ép thời gian xử lý tối đa cho MỌI request
func GlobalTimeoutMiddleware(timeout time.Duration) gin.HandlerFunc {
	return func(c *gin.Context) {
		// Bọc context hiện tại của request bằng 1 context có timeout
		ctx, cancel := context.WithTimeout(c.Request.Context(), timeout)
		defer cancel() // Hàm này tự dọn rác khi request xong hoặc bị hủy

		// Ghi đè cái context mới này vào lại Gin Request
		c.Request = c.Request.WithContext(ctx)

		// Đi tiếp vào các handler bên trong
		c.Next()
	}
}
