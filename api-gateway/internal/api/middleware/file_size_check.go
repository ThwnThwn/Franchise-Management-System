package middleware

import (
	"api-gateway/internal/application/constants/errorcode"

	"github.com/gin-gonic/gin"
)

func MaxUploadSize(size int) gin.HandlerFunc {
	return func(c *gin.Context) {
		if c.Request.ContentLength > int64(size) {
			c.Error(errorcode.ErrFileTooLarge)
			c.Abort()
			return
		}
		c.Next()
	}
}
