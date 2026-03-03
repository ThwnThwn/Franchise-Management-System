package grpcutils

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/pkg/utils/contextutils"
	"context"
	"fmt"

	"github.com/gin-gonic/gin"
	"google.golang.org/grpc/metadata"
)

func BuildAuthContext(c *gin.Context) (context.Context, error) {
	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return nil, errorcode.ErrInvalidToken
	}

	roleSlug, err := contextutils.GetRoleSlug(c)
	if err != nil {
		return nil, errorcode.ErrInvalidToken
	}

	md := metadata.Pairs(
		contextutils.HeaderUserId, fmt.Sprintf("%v", userId),
		contextutils.HeaderRoleslug, fmt.Sprintf("%v", roleSlug),
	)

	storeId, err := contextutils.GetStoreId(c)
	if err != nil {
		md[contextutils.HeaderStoreId] = []string{fmt.Sprintf("%v", storeId)}
	}

	return metadata.NewOutgoingContext(c.Request.Context(), md), nil
}
