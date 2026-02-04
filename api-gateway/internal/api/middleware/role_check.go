package middleware

import (
	"api-gateway/internal/application/constants/errorcode"
	"slices"

	"github.com/gin-gonic/gin"
)

func RequireRoles(allowedRoles ...string) gin.HandlerFunc {
	return func(c *gin.Context) {
		roleSlug := c.GetString("roleSlug")
		if roleSlug == "" {
			c.Error(errorcode.ErrInvalidAccessToken)
			c.Abort()
			return
		}

		if !slices.Contains(allowedRoles, roleSlug) {
			c.Error(errorcode.ErrForbidden)
			c.Abort()
			return
		}

		c.Next()
	}
}
