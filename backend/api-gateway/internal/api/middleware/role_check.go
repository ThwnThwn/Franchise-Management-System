package middleware

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/pkg/utils/contextutils"
	"slices"

	"github.com/gin-gonic/gin"
)

func RequireRoles(allowedRoles ...string) gin.HandlerFunc {
	return func(c *gin.Context) {
		roleSlug, err := contextutils.GetRoleSlug(c)
		if err != nil {
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
