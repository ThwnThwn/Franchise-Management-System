package abstractions

import (
	"api-gateway/internal/domain/constants/jwttype"
	"api-gateway/internal/domain/models/auth"
	"context"
)

type IAuthUseCase interface {
	Login(ctx context.Context, req *auth.LoginRequest) (*auth.LoginResponse, error)
	Logout(ctx context.Context, req *auth.LogoutRequest) (bool, error)
	Register(ctx context.Context, req *auth.RegisterRequest) (string, error)
	ConfirmRegisterOtp(ctx context.Context, req *auth.ConfirmOtpRequest) (bool, string, error)
	RefreshToken(ctx context.Context, req *auth.RefreshTokenRequest) (*auth.RefreshTokenResponse, error)
	ForgotPassword(ctx context.Context, email string) (string, error)
	ConfirmForgotPasswordOtp(ctx context.Context, req *auth.ConfirmOtpRequest) (string, error)
	NewPassword(ctx context.Context, newPassword string) (bool, string, error)
	ReSendOtp(ctx context.Context, email string, jwtType jwttype.JwtType) (string, error)
}
