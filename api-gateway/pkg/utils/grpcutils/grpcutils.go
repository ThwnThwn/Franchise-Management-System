package grpcutils

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/pkg/logger"
	"api-gateway/pkg/utils/contextutils"
	"context"
	"fmt"
	"time"

	"github.com/gin-gonic/gin"
	"go.uber.org/zap"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
	"google.golang.org/grpc/keepalive"
	"google.golang.org/grpc/metadata"
)

func BuildAuthContext(c *gin.Context) (context.Context, error) {
	if md, ok := metadata.FromOutgoingContext(c.Request.Context()); ok {
		if len(md.Get(contextutils.HeaderUserId)) > 0 {
			return c.Request.Context(), nil
		}
	}

	// 2. Logic build như cũ của ní
	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return nil, errorcode.ErrInvalidAccessToken
	}

	roleSlug, err := contextutils.GetRoleSlug(c)
	if err != nil {
		return nil, errorcode.ErrInvalidAccessToken
	}

	md := metadata.Pairs(
		contextutils.HeaderUserId, fmt.Sprintf("%v", userId),
		contextutils.HeaderRoleslug, fmt.Sprintf("%v", roleSlug),
	)

	storeId, err := contextutils.GetStoreId(c)
	if err == nil {
		md[contextutils.HeaderStoreId] = []string{fmt.Sprintf("%v", storeId)}
	}

	return metadata.NewOutgoingContext(c.Request.Context(), md), nil
}

// NewClientConn là hàm dùng chung để khởi tạo kết nối gRPC
func NewClientConn(url string, serviceName string, l logger.Interface, extraOpts ...grpc.DialOption) (*grpc.ClientConn, func(), error) {
	// Cấu hình Keepalive chung
	kacp := keepalive.ClientParameters{
		Time:                10 * time.Second,
		Timeout:             time.Second,
		PermitWithoutStream: true,
	}

	// Policy cho Retry và Load Balancing
	serviceConfigJSON := `{
        "loadBalancingConfig": [{"round_robin":{}}],
        "methodConfig": [{
            "name": [{"service": ""}],
            "retryPolicy": {
                "maxAttempts": 3,
                "initialBackoff": "0.1s",
                "maxBackoff": "1s",
                "backoffMultiplier": 2,
                "retryableStatusCodes": ["UNAVAILABLE"]
            }
        }]
    }`

	// Các option mặc định
	dialOpts := []grpc.DialOption{
		grpc.WithTransportCredentials(insecure.NewCredentials()),
		grpc.WithKeepaliveParams(kacp),
		grpc.WithDefaultServiceConfig(serviceConfigJSON),
	}

	// Thêm các option riêng
	dialOpts = append(dialOpts, extraOpts...)

	conn, err := grpc.NewClient(url, dialOpts...)
	if err != nil {
		return nil, nil, fmt.Errorf("could not connect to %s at %s: %w", serviceName, url, err)
	}

	cleanup := func() {
		l.Info("Closing gRPC connection", zap.String("service", serviceName), zap.String("url", url))
		conn.Close()
	}

	return conn, cleanup, nil
}
