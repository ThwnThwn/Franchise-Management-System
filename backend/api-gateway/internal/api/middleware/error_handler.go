package middleware

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/pkg/utils/errorutils"

	"github.com/gin-gonic/gin"
)

func GlobalErrorHandler() gin.HandlerFunc {
	return func(c *gin.Context) {
		c.Next()

		if len(c.Errors) > 0 {
			err := c.Errors.Last().Err

			status, title, detail := errorutils.MapErrorToProblem(err)

			if !c.Writer.Written() {
				c.JSON(status, errorcode.ErrorResponse{
					Title:  title,
					Status: status,
					Detail: detail,
				})
			}

			c.Abort()
		}
	}
}
