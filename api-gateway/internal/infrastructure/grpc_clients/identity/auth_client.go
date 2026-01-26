package identity

import (
	identityGW "api-gateway/internal/application/gateways/identity"
	authPb "api-gateway/pkg/pb/identity"
	"context"
)

type AuthGrpcClient struct {
	client authPb.AuthGrpcClient
}

// ProvideAuthClient trích xuất Auth Client từ Factory
func ProvideAuthClient(f *identityClientFactory) identityGW.IAuthService {
	return &AuthGrpcClient{client: authPb.NewAuthGrpcClient(f.conn)}
}

// Login implements identity.IAuthService.
func (a *AuthGrpcClient) Login(ctx context.Context, req *authPb.LoginRequest) (*authPb.LoginResponse, error) {
	res, err := a.client.Login(ctx, req)
	if err != nil {
		return nil, err
	}
	return res, nil
}

// Logout implements identity.IAuthService.
func (a *AuthGrpcClient) Logout(ctx context.Context, req *authPb.LogoutRequest) (*authPb.LogoutResponse, error) {
	res, err := a.client.Logout(ctx, req)
	if err != nil {
		return nil, err
	}
	return res, nil
}

// RefreshToken implements identity.IAuthService.
func (a *AuthGrpcClient) RefreshToken(ctx context.Context, req *authPb.RefreshTokenRequest) (*authPb.RefreshTokenResponse, error) {
	res, err := a.client.RefreshToken(ctx, req)
	if err != nil {
		return nil, err
	}
	return res, nil
}

// Register implements identity.IAuthService.
func (a *AuthGrpcClient) Register(ctx context.Context, req *authPb.RegisterRequest) (*authPb.RegisterResponse, error) {
	res, err := a.client.Register(ctx, req)
	if err != nil {
		return nil, err
	}
	return res, nil
}

// ConfirmOtp implements identity.IAuthService.
func (a *AuthGrpcClient) ConfirmOtp(ctx context.Context, req *authPb.ConfirmOtpRequest) (*authPb.ConfirmOtpResponse, error) {
	res, err := a.client.ConfirmOtp(ctx, req)
	if err != nil {
		return nil, err
	}
	return res, nil
}
