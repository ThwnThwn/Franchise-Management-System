package middleware

import (
	"api-gateway/global"
	"net/http"

	"github.com/gin-gonic/gin"
)

func CORSMiddleware() gin.HandlerFunc {
	return func(c *gin.Context) {
		// 1. Cho phép tất cả các nguồn (hoặc cụ thể domain Frontend của bạn)
		c.Writer.Header().Set("Access-Control-Allow-Origin", global.Config.FrontEndOrigin)

		// 2. Cho phép gửi kèm Credentials (nếu cần cookie/token)
		c.Writer.Header().Set("Access-Control-Allow-Credentials", "true")

		// 3. Cho phép các Headers này
		c.Writer.Header().Set("Access-Control-Allow-Headers", "Content-Type, Content-Length, Accept-Encoding, X-CSRF-Token, Authorization, accept, origin, Cache-Control, X-Requested-With")

		// 4. QUAN TRỌNG: Liệt kê TẤT CẢ method mà bạn muốn hỗ trợ
		c.Writer.Header().Set("Access-Control-Allow-Methods", "POST, OPTIONS, GET, PUT, DELETE, PATCH, HEAD")

		// 5. Xử lý Preflight Request (OPTIONS)
		// Trình duyệt sẽ gửi OPTIONS trước khi gửi PUT/DELETE/POST
		if c.Request.Method == "OPTIONS" {
			c.AbortWithStatus(http.StatusNoContent) // Trả về 204 No Content là OK
			return
		}

		c.Next()
	}
}
