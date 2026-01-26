package abstractions

import (
	"api-gateway/internal/domain/models/auth"
	"context"
)

type IAuthUseCase interface {
	ExecuteLogin(ctx context.Context, req *auth.LoginRequest) (*auth.LoginResponse, error)
	ExecuteLogout(ctx context.Context, req *auth.LogoutRequest) (bool, error)
	ExecuteRegister(ctx context.Context, req *auth.RegisterRequest) (*string, error)
	ExecuteConfirmOtp(ctx context.Context, req *auth.ConfirmOtpRequest) (bool, *string, error)
	ExecuteRefreshToken(ctx context.Context, req *auth.RefreshTokenRequest) (*auth.RefreshTokenResponse, error)
}
