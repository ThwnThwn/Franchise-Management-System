package user

import (
	"api-gateway/internal/domain/models/role"

	"google.golang.org/protobuf/types/known/timestamppb"
)

type UserResponse struct {
	Id          string                 `json:"id"`
	Email       string                 `json:"email"`
	Username    string                 `json:"username"`
	Phone       string                 `json:"phone"`
	Address     string                 `json:"address"`
	IsBlocked   bool                   `json:"isBlocked"`
	FirstName   string                 `json:"firstName"`
	LastName    string                 `json:"lastName"`
	Gender      int32                  `json:"gender"`
	DateOfBirth *timestamppb.Timestamp `json:"dateOfBirth"`
	Role        *role.RoleUserResponse `json:"role"`
	CreatedAt   *timestamppb.Timestamp `json:"createdAt"`
	UpdatedAt   *timestamppb.Timestamp `json:"updatedAt"`
}
