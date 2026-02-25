package api

import "github.com/gin-gonic/gin"

// Chuyển handler bình thường sang kiểu trả về error
type HandlerFuncErr func(*gin.Context) error

func WrapHandler(fn HandlerFuncErr) gin.HandlerFunc {
	return func(c *gin.Context) {
		if err := fn(c); err != nil {
			c.Error(err)
			c.Abort()
		}
	}
}
