package abstractions

import (
	"api-gateway/pkg/pb/identity"
	"context"
)

type IAuthUseCase interface {
	ExecuteLogin(ctx context.Context, req *identity.LoginRequest) (*identity.LoginResponse, error)
	ExecuteLogout(ctx context.Context, req *identity.LogoutRequest) (*identity.LogoutResponse, error)
	ExecuteRegister(ctx context.Context, req *identity.RegisterRequest) (*identity.RegisterResponse, error)
	ExecuteRefreshToken(ctx context.Context, req *identity.RefreshTokenRequest) (*identity.RefreshTokenResponse, error)
}
