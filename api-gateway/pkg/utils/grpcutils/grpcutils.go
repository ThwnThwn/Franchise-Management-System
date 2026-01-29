package grpcutils

import (
	"api-gateway/internal/application/constants/errorcode"
	"context"
	"fmt"

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

	md := metadata.Pairs(
		"x-user-id", fmt.Sprintf("%v", userID),
		"x-role-slug", fmt.Sprintf("%v", roleSlug),
	)
	return metadata.NewOutgoingContext(c.Request.Context(), md), nil
}
