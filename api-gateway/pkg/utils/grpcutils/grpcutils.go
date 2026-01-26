package grpcutils

import (
	"api-gateway/internal/application/constants/errorcode"
	"context"

	"github.com/gin-gonic/gin"
	"google.golang.org/grpc/metadata"
)

func BuildAuthContext(c *gin.Context) (context.Context, error) {
	userID, exists := c.Get("userID")
	if !exists {
		return nil, errorcode.ErrInvalidToken
	}

	roleSlug, exists := c.Get("roleSlug")
	if !exists {
		return nil, errorcode.ErrInvalidToken
	}

	uidStr, ok1 := userID.(string)
	roleStr, ok2 := roleSlug.(string)

	if !ok1 || !ok2 {
		return nil, errorcode.ErrInvalidToken
	}

	md := metadata.Pairs(
		"x-user-id", uidStr,
		"x-role-slug", roleStr,
	)

	return metadata.NewOutgoingContext(c.Request.Context(), md), nil
}
