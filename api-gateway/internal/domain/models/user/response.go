package user

import (
	"api-gateway/internal/domain/models/role"
	"time"
)

type UserResponse struct {
	Id          string     `json:"id"`
	Username    string     `json:"username"`
	Email       string     `json:"email"`
	Phone       string     `json:"phone"`
	Address     string     `json:"address"`
	FirstName   string     `json:"firstName"`
	LastName    string     `json:"lastName"`
	Gender      int32      `json:"gender"`
	DateOfBirth *time.Time `json:"dateOfBirth"`

	AvatarUrl *string `json:"avatarUrl"`
	IsBlocked bool    `json:"isBlocked"`

	HomeStoreID       *string                `json:"homeStoreId"`
	CurrentHourlyWage *float64               `json:"currentHourlyWage"`
	Role              *role.RoleUserResponse `json:"role"`

	CreatedAt *time.Time `json:"createdAt"`
	UpdatedAt *time.Time `json:"updatedAt"`
}
