package auth

import "api-gateway/internal/domain/models/user"

type LoginResponse struct {
	RefreshTokenResponse
	User *user.UserResponse `json:"user"`
}

type RefreshTokenResponse struct {
	AccessToken  string `json:"accessToken"`
	RefreshToken string `json:"refreshToken"`
}
