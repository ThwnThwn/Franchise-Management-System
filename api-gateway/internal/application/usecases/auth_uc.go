package usecases

import (
	"api-gateway/internal/application/abstractions"
	idenGW "api-gateway/internal/application/gateways/identity"
	idenPb "api-gateway/pkg/pb/identity"
	"context"
)

type AuthUseCase struct {
	service idenGW.IAuthService
}

func NewAuthUseCase(service idenGW.IAuthService) abstractions.IAuthUseCase {
	return &AuthUseCase{service: service}
}

// ExecuteLogin implements abstractions.IAuthUseCase.
func (a *AuthUseCase) ExecuteLogin(ctx context.Context, req *idenPb.LoginRequest) (*idenPb.LoginResponse, error) {
	return a.service.Login(ctx, req)
}

// ExecuteLogout implements abstractions.IAuthUseCase.
func (a *AuthUseCase) ExecuteLogout(ctx context.Context, req *idenPb.LogoutRequest) (*idenPb.LogoutResponse, error) {
	return a.service.Logout(ctx, req)
}

// ExecuteRefreshToken implements abstractions.IAuthUseCase.
func (a *AuthUseCase) ExecuteRefreshToken(ctx context.Context, req *idenPb.RefreshTokenRequest) (*idenPb.RefreshTokenResponse, error) {
	return a.service.RefreshToken(ctx, req)
}

// ExecuteRegister implements abstractions.IAuthUseCase.
func (a *AuthUseCase) ExecuteRegister(ctx context.Context, req *idenPb.RegisterRequest) (*idenPb.RegisterResponse, error) {
	return a.service.Register(ctx, req)
}
