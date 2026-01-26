package identity

import (
	identityGW "api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/models/auth"
	"api-gateway/internal/domain/models/role"
	"api-gateway/internal/domain/models/user"
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
func (a *AuthGrpcClient) Login(ctx context.Context, req *auth.LoginRequest) (*auth.LoginResponse, error) {
	res, err := a.client.Login(ctx, &authPb.LoginRequest{
		EmailOrUsername: req.EmailOrUsername,
		Password:        req.Password,
	})
	if err != nil {
		return nil, err
	}
	userRes := res.User
	return &auth.LoginResponse{
		RefreshTokenResponse: auth.RefreshTokenResponse{
			AccessToken:  res.AccessToken,
			RefreshToken: res.RefreshToken,
		},
		User: &user.UserResponse{
			Id:        userRes.Id,
			Email:     userRes.Email,
			Username:  userRes.Username,
			Phone:     userRes.Phone,
			Address:   userRes.Address,
			IsBlocked: userRes.IsBlocked,
			Role: &role.RoleUserResponse{
				Id:   userRes.Role.Id,
				Name: userRes.Role.Name,
			},
			CreatedAt: userRes.CreatedAt,
			UpdatedAt: userRes.UpdatedAt,
		},
	}, nil
}

// Logout implements identity.IAuthService.
func (a *AuthGrpcClient) Logout(ctx context.Context, req *auth.LogoutRequest) (bool, error) {
	res, err := a.client.Logout(ctx, &authPb.LogoutRequest{
		AccessToken:  req.AccessToken,
		RefreshToken: req.RefreshToken,
	})
	return res.Success, err
}

// RefreshToken implements identity.IAuthService.
func (a *AuthGrpcClient) RefreshToken(ctx context.Context, req *auth.RefreshTokenRequest) (*auth.RefreshTokenResponse, error) {
	res, err := a.client.RefreshToken(ctx, &authPb.RefreshTokenRequest{
		RefreshToken: req.RefreshToken,
	})
	if err != nil {
		return nil, err
	}
	return &auth.RefreshTokenResponse{
		AccessToken:  res.AccessToken,
		RefreshToken: res.RefreshToken,
	}, nil
}

// Register implements identity.IAuthService.
func (a *AuthGrpcClient) Register(ctx context.Context, req *auth.RegisterRequest) (*string, error) {
	res, err := a.client.Register(ctx, &authPb.RegisterRequest{
		Email:    req.Email,
		Password: req.Password,
		Phone:    req.Phone,
		Address:  req.Address,
	})
	if err != nil {
		return nil, err
	}
	return &res.Message, nil
}

// ConfirmOtp implements identity.IAuthService.
func (a *AuthGrpcClient) ConfirmOtp(ctx context.Context, req *auth.ConfirmOtpRequest) (bool, *string, error) {
	res, err := a.client.ConfirmOtp(ctx, &authPb.ConfirmOtpRequest{
		Email: req.Email,
		Otp:   req.Otp,
	})
	if err != nil {
		return false, nil, err
	}
	return res.Success, &res.Message, nil
}
