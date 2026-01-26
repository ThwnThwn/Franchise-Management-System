package user

import (
	"api-gateway/internal/domain/models/role"
	"time"
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
	DateOfBirth *time.Time             `json:"dateOfBirth"`
	Role        *role.RoleUserResponse `json:"role"`
	CreatedAt   *time.Time             `json:"createdAt"`
	UpdatedAt   *time.Time             `json:"updatedAt"`
}
