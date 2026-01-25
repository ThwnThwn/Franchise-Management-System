package middleware

import (
	"api-gateway/pkg/utils/errorutils"

	"github.com/gin-gonic/gin"
)

func GlobalErrorHandler() gin.HandlerFunc {
	return func(c *gin.Context) {
		c.Next()

		if len(c.Errors) > 0 {
			err := c.Errors.Last().Err

			// Sử dụng hàm nâng cấp
			status, title, detail := errorutils.MapErrorToProblem(err)

			c.JSON(status, gin.H{
				"title":  title,
				"detail": detail,
				// "status": status,
			})
			c.Abort()
		}
	}
}
