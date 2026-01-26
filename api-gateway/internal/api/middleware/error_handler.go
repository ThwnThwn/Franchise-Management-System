package middleware

import (
	"api-gateway/internal/domain/models/errormodel"
	"api-gateway/pkg/utils/errorutils"

	"github.com/gin-gonic/gin"
)

func GlobalErrorHandler() gin.HandlerFunc {
	return func(c *gin.Context) {
		c.Next()

		if len(c.Errors) > 0 {
			err := c.Errors.Last().Err

			status, title, detail := errorutils.MapErrorToProblem(err)

			c.JSON(status, errormodel.BackendError{
				Title:  title,
				Status: status,
				Detail: detail,
			})
			c.Abort()
		}
	}
}
