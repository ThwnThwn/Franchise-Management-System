package usecases

import (
	"api-gateway/internal/application/abstractions"
	idenGW "api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/models/auth"
	"context"
)

type AuthUseCase struct {
	service idenGW.IAuthService
}

func NewAuthUseCase(service idenGW.IAuthService) abstractions.IAuthUseCase {
	return &AuthUseCase{service: service}
}

// Login implements abstractions.IAuthUseCase.
func (a *AuthUseCase) Login(ctx context.Context, req *auth.LoginRequest) (*auth.LoginResponse, error) {
	return a.service.Login(ctx, req)
}

// Logout implements abstractions.IAuthUseCase.
func (a *AuthUseCase) Logout(ctx context.Context, req *auth.LogoutRequest) (bool, error) {
	return a.service.Logout(ctx, req)
}

// RefreshToken implements abstractions.IAuthUseCase.
func (a *AuthUseCase) RefreshToken(ctx context.Context, req *auth.RefreshTokenRequest) (*auth.RefreshTokenResponse, error) {
	return a.service.RefreshToken(ctx, req)
}

// Register implements abstractions.IAuthUseCase.
func (a *AuthUseCase) Register(ctx context.Context, req *auth.RegisterRequest) (string, error) {
	return a.service.Register(ctx, req)
}

// ConfirmOtp implements abstractions.IAuthUseCase.
func (a *AuthUseCase) ConfirmOtp(ctx context.Context, req *auth.ConfirmOtpRequest) (bool, string, error) {
	return a.service.ConfirmOtp(ctx, req)
}
