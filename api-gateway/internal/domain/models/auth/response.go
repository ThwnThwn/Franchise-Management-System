package auth

import (
	"google.golang.org/protobuf/types/known/timestamppb"
)

type LoginResponse struct {
	RefreshTokenResponse
	User *UserResponse `json:"user"`
}

type RefreshTokenResponse struct {
	AccessToken  string `json:"accessToken"`
	RefreshToken string `json:"refreshToken"`
}

type UserResponse struct {
	Id        string                 `json:"id"`
	Email     string                 `json:"email"`
	Username  string                 `json:"username"`
	Phone     string                 `json:"phone"`
	Address   string                 `json:"address"`
	IsBlocked bool                   `json:"isBlocked"`
	Role      *RoleUserResponse      `json:"role"`
	CreatedAt *timestamppb.Timestamp `json:"createdAt"`
	UpdatedAt *timestamppb.Timestamp `json:"updatedAt"`
}

type RoleUserResponse struct {
	Id   string `json:"id"`
	Name string `json:"name"`
}
