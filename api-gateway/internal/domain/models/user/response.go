package user

import (
	"api-gateway/internal/domain/models/loyalty"
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

	AvatarUrl *string                `json:"avatarUrl"`
	IsBlocked bool                   `json:"isBlocked"`
	Role      *role.RoleUserResponse `json:"role"`

<<<<<<< HEAD
	HomeStoreID       *string                `json:"homeStoreId"`
	CurrentHourlyWage *float64               `json:"currentHourlyWage"`
	Role              *role.RoleUserResponse `json:"role"`
=======
	HomeStoreId       *string  `json:"homeStoreId"`
	CurrentHourlyWage *float64 `json:"currentHourlyWage"`

	Loyalty    *loyalty.LoyaltyResponse `json:"loyalty"`
	OrderCount *int32                   `json:"orderCount"`
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960

	CreatedAt *time.Time `json:"createdAt"`
	UpdatedAt *time.Time `json:"updatedAt"`
}
