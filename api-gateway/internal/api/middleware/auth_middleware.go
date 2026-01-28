package middleware

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/pkg/utils/jwtutils"
	"strings"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

func ValidateToken(secret []byte) gin.HandlerFunc {
	return func(c *gin.Context) {
		authHeader := c.GetHeader("Authorization")
		if authHeader == "" {
			c.Error(errorcode.ErrInvalidAccessToken)
			c.Abort()
			return
		}

		if !strings.HasPrefix(authHeader, "Bearer ") {
			c.Error(errorcode.ErrInvalidAccessToken)
			c.Abort()
			return
		}

		token := strings.TrimPrefix(authHeader, "Bearer ")
		claims, err := jwtutils.ValidateToken(secret, token)
		if err != nil {
			c.Error(errorcode.ErrInvalidAccessToken)
			c.Abort()
			return
		}

		userID, err := uuid.Parse(claims.Subject)
		if err != nil {
			c.Error(errorcode.ErrUnexpectedSigningToken)
			c.Abort()
			return
		}

		c.Set("userID", userID)
		c.Set("accessToken", token)
		// c.Set("roleSlug", claims.RoleSlug)
		c.Next()
	}
}
