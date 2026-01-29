package middleware

import (
	"api-gateway/internal/application/constants/errorcode"
	"fmt"
	"slices"

	"github.com/gin-gonic/gin"
)

func RequireRoles(allowedRoles ...string) gin.HandlerFunc {
	return func(c *gin.Context) {
		roleSlug, exists := c.Get("roleSlug")
		if !exists {
			c.Error(errorcode.ErrInvalidAccessToken)
			c.Abort()
			return
		}

		userRole := fmt.Sprintf("%v", roleSlug)
		isAllowed := slices.Contains(allowedRoles, userRole)

		if !isAllowed {
			c.Error(errorcode.ErrForbidden)
			c.Abort()
			return
		}

		c.Next()
	}
}
