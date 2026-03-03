package franchise

import (
	"api-gateway/config"
	"api-gateway/pkg/logger"
	"crypto/tls"
	"fmt"
	"time"

	"go.uber.org/zap"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials"
	"google.golang.org/grpc/keepalive"
)

type clientFactory struct {
	conn *grpc.ClientConn
}

func NewFranchiseClientFactory(url config.FranchiseServiceUrl, l logger.Interface) (*clientFactory, func(), error) {
	// 1. Tạo config TLS chấp nhận mọi chứng chỉ (kể cả chứng chỉ đểu/tự ký)
	tlsConfig := &tls.Config{
		InsecureSkipVerify: true,
	}

	// 2. Tạo credentials từ config trên
	creds := credentials.NewTLS(tlsConfig)
	// 3. Giữ kết nối luôn "nóng", chống bị Docker ngắt trộm
	kacp := keepalive.ClientParameters{
		Time:                10 * time.Second,
		Timeout:             time.Second,
		PermitWithoutStream: true,
	}

	// 4. Gộp cấu hình Load Balancing và Luật Retry vào chung 1 chuỗi JSON
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

	conn, err := grpc.NewClient(string(url),
		grpc.WithTransportCredentials(creds),
		grpc.WithKeepaliveParams(kacp),                   // Kích hoạt Keepalive
		grpc.WithDefaultServiceConfig(serviceConfigJSON), // Kích hoạt LB + Retry
	)
	if err != nil {
		return nil, nil, fmt.Errorf("Cannot connect to gRPC server at %s: %w", url, err)
	}

	cleanup := func() {
		l.Info("Closing gRPC connection for Franchise Service", zap.String("url", string(url)))
		conn.Close()
	}

	return &clientFactory{conn: conn}, cleanup, nil
}
