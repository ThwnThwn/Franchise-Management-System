package identity

import (
	idenGW "api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/constants/jwttype"
	"api-gateway/internal/domain/models/auth"
	"api-gateway/pkg/mapper"
	idenPb "api-gateway/pkg/pb/identity"
	"context"
)

type AuthGrpcClient struct {
	client idenPb.AuthGrpcClient
}

// ProvideAuthClient trích xuất Auth Client từ Factory
func ProvideAuthClient(f *identityClientFactory) idenGW.IAuthService {
	return &AuthGrpcClient{client: idenPb.NewAuthGrpcClient(f.conn)}
}

// Login implements identity.IAuthService.
func (a *AuthGrpcClient) Login(ctx context.Context, req *auth.LoginRequest) (*auth.LoginResponse, error) {
	res, err := a.client.Login(ctx, &idenPb.LoginRequest{
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
		User: mapper.MapUserPbToUserModel(userRes),
	}, nil
}

// Logout implements identity.IAuthService.
func (a *AuthGrpcClient) Logout(ctx context.Context, req *auth.LogoutRequest) (bool, error) {
	res, err := a.client.Logout(ctx, &idenPb.LogoutRequest{
		AccessToken:  req.AccessToken,
		RefreshToken: req.RefreshToken,
	})
	if err != nil {
		return false, err
	}

	return res.Success, nil
}

// RefreshToken implements identity.IAuthService.
func (a *AuthGrpcClient) RefreshToken(ctx context.Context, req *auth.RefreshTokenRequest) (*auth.RefreshTokenResponse, error) {
	res, err := a.client.RefreshToken(ctx, &idenPb.RefreshTokenRequest{
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
func (a *AuthGrpcClient) Register(ctx context.Context, req *auth.RegisterRequest) (string, error) {
	res, err := a.client.Register(ctx, &idenPb.RegisterRequest{
		Email:       req.Email,
		Username:    req.Username,
		FirstName:   req.FirstName,
		LastName:    req.LastName,
		Password:    req.Password,
		Phone:       req.Phone,
		Address:     req.Address,
		Gender:      req.Gender,
		DateOfBirth: req.DateOfBirth,
	})
	if err != nil {
		return "", err
	}
	return res.Message, nil
}

// ConfirmRegisterOtp implements identity.IAuthService.
func (a *AuthGrpcClient) ConfirmRegisterOtp(ctx context.Context, req *auth.ConfirmOtpRequest) (bool, string, error) {
	res, err := a.client.ConfirmRegisterOtp(ctx, &idenPb.ConfirmOtpRequest{
		Email: req.Email,
		Otp:   req.Otp,
	})
	if err != nil {
		return false, "", err
	}
	return res.Success, res.Message, nil
}

// ConfirmForgotPasswordOtp implements identity.IAuthService.
func (a *AuthGrpcClient) ConfirmForgotPasswordOtp(ctx context.Context, req *auth.ConfirmOtpRequest) (string, error) {
	res, err := a.client.ConfirmForgetPasswordOtp(ctx, &idenPb.ConfirmOtpRequest{
		Email: req.Email,
		Otp:   req.Otp,
	})
	if err != nil {
		return "", err
	}
	return res.AccessForgetPasswordToken, nil
}

// ForgotPassword implements identity.IAuthService.
func (a *AuthGrpcClient) ForgotPassword(ctx context.Context, email string) (string, error) {
	res, err := a.client.ForgetPassword(ctx, &idenPb.ForgetPasswordRequest{Email: email})
	if err != nil {
		return "", err
	}
	return res.Message, nil
}

// NewPassword implements identity.IAuthService.
func (a *AuthGrpcClient) NewPassword(ctx context.Context, newPassword string) (bool, string, error) {
	res, err := a.client.NewPassword(ctx, &idenPb.NewPasswordRequest{NewPassword: newPassword})
	if err != nil {
		return false, "", err
	}
	return res.Success, res.Message, nil
}

// ReSendOtp implements identity.IAuthService.
func (a *AuthGrpcClient) ReSendOtp(ctx context.Context, email string, jwtType jwttype.JwtType) (string, error) {
	res, err := a.client.ReSendOtp(ctx, &idenPb.ReSendOtpRequest{
		Email: email,
		Type:  string(jwtType),
	})
	if err != nil {
		return "", err
	}
	return res.Message, nil
}
