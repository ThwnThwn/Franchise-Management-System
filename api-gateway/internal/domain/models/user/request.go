package user

import "time"

type UpdateUserRequest struct {
	Username    string     `json:"username"`
	Email       string     `json:"email"`
	Phone       string     `json:"phone"`
	Address     string     `json:"address"`
	FirstName   string     `json:"firstName"`
	LastName    string     `json:"lastName"`
	Gender      int32      `json:"gender"`
	DateOfBirth *time.Time `json:"dateOfBirth"`
	IsBlocked   bool       `json:"isBlocked"`
	RoleSlug    string     `json:"roleSlug"`
}

type UpdateUserRoleRequest struct {
	RoleSlug string `json:"roleSlug" binding:"required"`
}

type UpdateMeRequest struct {
	Email       string     `json:"email"`
	Phone       string     `json:"phone"`
	Address     string     `json:"address"`
	FirstName   string     `json:"firstName"`
	LastName    string     `json:"lastName"`
	Gender      int32      `json:"gender"`
	DateOfBirth *time.Time `json:"dateOfBirth"`
}
