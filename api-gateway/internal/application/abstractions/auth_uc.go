package abstractions

import (
	"api-gateway/internal/domain/models/auth"
	"context"
)

type IAuthUseCase interface {
	Login(ctx context.Context, req *auth.LoginRequest) (*auth.LoginResponse, error)
	Logout(ctx context.Context, req *auth.LogoutRequest) (bool, error)
	Register(ctx context.Context, req *auth.RegisterRequest) (string, error)
	ConfirmOtp(ctx context.Context, req *auth.ConfirmOtpRequest) (bool, string, error)
	RefreshToken(ctx context.Context, req *auth.RefreshTokenRequest) (*auth.RefreshTokenResponse, error)
}
