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

// ExecuteLogin implements abstractions.IAuthUseCase.
func (a *AuthUseCase) ExecuteLogin(ctx context.Context, req *auth.LoginRequest) (*auth.LoginResponse, error) {
	return a.service.Login(ctx, req)
}

// ExecuteLogout implements abstractions.IAuthUseCase.
func (a *AuthUseCase) ExecuteLogout(ctx context.Context, req *auth.LogoutRequest) (bool, error) {
	return a.service.Logout(ctx, req)
}

// ExecuteRefreshToken implements abstractions.IAuthUseCase.
func (a *AuthUseCase) ExecuteRefreshToken(ctx context.Context, req *auth.RefreshTokenRequest) (*auth.RefreshTokenResponse, error) {
	return a.service.RefreshToken(ctx, req)
}

// ExecuteRegister implements abstractions.IAuthUseCase.
func (a *AuthUseCase) ExecuteRegister(ctx context.Context, req *auth.RegisterRequest) (*string, error) {
	return a.service.Register(ctx, req)
}

// ExecuteConfirmOtp implements abstractions.IAuthUseCase.
func (a *AuthUseCase) ExecuteConfirmOtp(ctx context.Context, req *auth.ConfirmOtpRequest) (bool, *string, error) {
	return a.service.ConfirmOtp(ctx, req)
}
