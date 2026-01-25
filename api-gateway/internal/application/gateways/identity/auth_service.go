package identity

import (
	"api-gateway/pkg/pb/identity"
	"context"
)

type IAuthService interface {
	Login(ctx context.Context, req *identity.LoginRequest) (*identity.LoginResponse, error)
	Register(ctx context.Context, req *identity.RegisterRequest) (*identity.RegisterResponse, error)
	RefreshToken(ctx context.Context, req *identity.RefreshTokenRequest) (*identity.RefreshTokenResponse, error)
	Logout(ctx context.Context, req *identity.LogoutRequest) (*identity.LogoutResponse, error)
}
